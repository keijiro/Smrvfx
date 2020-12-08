using UnityEngine;
using System.Linq;

namespace Smrvfx {

[ExecuteInEditMode]
public sealed partial class SkinnedMeshBaker : MonoBehaviour
{
    #region Internal objects

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

    void OnDisable()
      => DisposeInternals();

    void OnDestroy()
      => DisposeInternals();

    void LateUpdate()
    {
        if (!IsValid) return;

        // Lazy initialization
        if (_tempMesh == null) InitializeInternals();

        // Current transform matrix
        _rootMatrix.current = _sources[0].transform.localToWorldMatrix;

        // Bake the sources into the buffers.
        var offset = 0;
        foreach (var source in _sources)
            offset += BakeSource(source, offset);

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

    void InitializeInternals()
    {
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

        // Root matrix
        var l2w = _sources[0].transform.localToWorldMatrix;
        _rootMatrix = (l2w, l2w);

        // Temporary mesh object
        _tempMesh = new Mesh();
        _tempMesh.hideFlags = HideFlags.DontSave;
    }

    void DisposeInternals()
    {
        _samplePoints?.Dispose();
        _samplePoints = null;

        _positionBuffer1?.Dispose();
        _positionBuffer1 = null;

        _positionBuffer2?.Dispose();
        _positionBuffer2 = null;

        _normalBuffer?.Dispose();
        _normalBuffer = null;

        ObjectUtil.Destroy(_positionMap);
        _positionMap = null;

        ObjectUtil.Destroy(_velocityMap);
        _velocityMap = null;

        ObjectUtil.Destroy(_normalMap);
        _normalMap = null;

        ObjectUtil.Destroy(_tempMesh);
        _tempMesh = null;
    }

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
