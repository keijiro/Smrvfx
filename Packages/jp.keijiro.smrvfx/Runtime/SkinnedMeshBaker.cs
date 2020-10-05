using UnityEngine;
using System.Linq;
using Unity.Mathematics;

namespace Smrvfx {

public sealed partial class SkinnedMeshBaker : MonoBehaviour
{
    #region Temporary objects

    ComputeBuffer _samplePoints;
    ComputeBuffer _positionBuffer1;
    ComputeBuffer _positionBuffer2;
    ComputeBuffer _normalBuffer;

    RenderTexture _positionMap;
    RenderTexture _velocityMap;
    RenderTexture _normalMap;

    (Matrix4x4 current, Matrix4x4 previous) _rootMatrix;
    Mesh _tempMesh;

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        if (!IsValid) return;

        using (var mesh = new CombinedMesh(_sources))
        {
            // Sample point generation
            using (var points = SamplePointGenerator.Generate
                                  (mesh, _pointCount))
            {
                _samplePoints = new ComputeBuffer
                  (_pointCount, SamplePoint.SizeInByte);
                _samplePoints.SetData(points);
            }

            // Intermediate buffer allocation
            var vcount = mesh.Vertices.Length;
            var float3size = sizeof(float) * 3;
            _positionBuffer1 = new ComputeBuffer(vcount, float3size);
            _positionBuffer2 = new ComputeBuffer(vcount, float3size);
            _normalBuffer = new ComputeBuffer(vcount, float3size);
        }

        // Destination render texture allocation
        var width = 256;
        var height = (((_pointCount + width - 1) / width + 7) / 8) * 8;
        _positionMap = RenderTextureUtil.AllocateFloat(width, height);
        _velocityMap = RenderTextureUtil.AllocateHalf(width, height);
        _normalMap = RenderTextureUtil.AllocateHalf(width, height);

        // Etc.
        //--------------------------------------------------------------
        // Uncharted Limbo-Changed
        //--------------------------------------------------------------
        // Added a separate root transformation object, instead of using the first source
        var l2w = GlobalTransformationMatrix;
        // var l2w = _sources[0].transform.localToWorldMatrix;
        //--------------------------------------------------------------

        _rootMatrix = (l2w, l2w);
        _tempMesh = new Mesh();
    }

    void OnDestroy()
    {
        if (!IsValid) return;

        _samplePoints.Dispose();
        _positionBuffer1.Dispose();
        _positionBuffer2.Dispose();
        _normalBuffer.Dispose();

        Destroy(_positionMap);
        Destroy(_velocityMap);
        Destroy(_normalMap);

        Destroy(_tempMesh);
    }

    void LateUpdate()
    {
        if (!IsValid) return;

        // Current transform matrix
        //--------------------------------------------------------------
        // Uncharted Limbo-Changed
        //--------------------------------------------------------------
        // Added a separate root transformation object, instead of using the first source
        _rootMatrix.current = GlobalTransformationMatrix;
        //_rootMatrix.current = _sources[0].transform.localToWorldMatrix;
        //--------------------------------------------------------------

        // Bake the sources into the buffers.
        var offset = 0;
        foreach (var source in _sources) offset += BakeSource(source, offset);

        // ComputeBuffer -> RenderTexture
        TransferData();

        // Position buffer swapping
        (_positionBuffer1, _positionBuffer2)
          = (_positionBuffer2, _positionBuffer1);

        // Transform matrix history
        _rootMatrix.previous = _rootMatrix.current;
    }

    #endregion

    #region Private methods

    int BakeSource(SkinnedMeshRenderer source, int offset)
    {
        source.BakeMesh(_tempMesh);

        using (var dataArray = Mesh.AcquireReadOnlyMeshData(_tempMesh))
        {
            var data = dataArray[0];
            var vcount = data.vertexCount;

            using (var pos = MemoryUtil.TempJobArray<Vector3>(vcount))
            using (var nrm = MemoryUtil.TempJobArray<Vector3>(vcount))
            {
                data.GetVertices(pos);
                data.GetNormals(nrm);

                //--------------------------------------------------------------
                // Uncharted Limbo-Added
                //--------------------------------------------------------------
                // The vertices are transformed by the source's transformation matrix
                JobUtil.TransformVertices(pos.Reinterpret<float3>(),source.localToWorldMatrix );
                //--------------------------------------------------------------

                _positionBuffer1.SetData(pos, 0, offset, vcount);
                _normalBuffer.SetData(nrm, 0, offset, vcount);

                return vcount;
            }
        }
    }

    void TransferData()
    {
        _compute.SetInt("SampleCount", _pointCount);
        _compute.SetMatrix("Transform", _rootMatrix.current);
        _compute.SetMatrix("OldTransform", _rootMatrix.previous);
        _compute.SetFloat("FrameRate", 1 / Time.deltaTime);

        _compute.SetBuffer(0, "SamplePoints", _samplePoints);
        _compute.SetBuffer(0, "PositionBuffer", _positionBuffer1);
        _compute.SetBuffer(0, "OldPositionBuffer", _positionBuffer2);
        _compute.SetBuffer(0, "NormalBuffer", _normalBuffer);

        _compute.SetTexture(0, "PositionMap", _positionMap);
        _compute.SetTexture(0, "VelocityMap", _velocityMap);
        _compute.SetTexture(0, "NormalMap", _normalMap);

        var width = _positionMap.width;
        var height = _positionMap.height;
        _compute.Dispatch(0, width / 8, height / 8, 1);
    }

    #endregion
}

} // namespace Smrvfx
