using UnityEngine;
using System.Collections.Generic;

namespace Smrvfx
{
    public class SkinnedMeshBaker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] SkinnedMeshRenderer _source = null;
        [SerializeField] RenderTexture _positionMap = null;
        [SerializeField] RenderTexture _normalMap = null;
        [SerializeField] ComputeShader _compute = null;

        #endregion

        #region Temporary objects

        Mesh _mesh;

        int[] _dimensions = new int[2];

        List<Vector3> _positionList = new List<Vector3>();
        List<Vector3> _normalList = new List<Vector3>();

        ComputeBuffer _positionBuffer;
        ComputeBuffer _normalBuffer;

        RenderTexture _tempPositionMap;
        RenderTexture _tempNormalMap;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _mesh = new Mesh();
        }

        void OnDestroy()
        {
            Destroy(_mesh);
            _mesh = null;

            if (_positionBuffer != null)
            {
                _positionBuffer.Dispose();
                _positionBuffer = null;
            }

            if (_normalBuffer != null)
            {
                _normalBuffer.Dispose();
                _normalBuffer = null;
            }

            if (_tempPositionMap != null)
            {
                Destroy(_tempPositionMap);
                _tempPositionMap = null;
            }

            if (_tempNormalMap != null)
            {
                Destroy(_tempNormalMap);
                _tempNormalMap = null;
            }
        }

        void Update()
        {
            if (_source == null) return;

            _source.BakeMesh(_mesh);
            _mesh.GetVertices(_positionList);
            _mesh.GetNormals(_normalList);

            if (!CheckConsistency()) return;

            TransferData();
        }

        #endregion

        #region Private methods

        void TransferData()
        {
            var vcount = _positionList.Count;
            var vcount_x3 = vcount * 3;

            _dimensions[0] = _positionMap.width;
            _dimensions[1] = _positionMap.height;

            // Release the temporary objects when the size of them don't match
            // the input.

            if (_positionBuffer != null &&
                _positionBuffer.count != vcount_x3)
            {
                _positionBuffer.Dispose();
                _positionBuffer = null;
            }

            if (_normalBuffer != null &&
                _normalBuffer.count != vcount_x3)
            {
                _normalBuffer.Dispose();
                _normalBuffer = null;
            }

            if (_tempPositionMap != null &&
                (_tempPositionMap.width != _dimensions[0] ||
                 _tempPositionMap.height != _dimensions[1]))
            {
                Destroy(_tempPositionMap);
                _tempPositionMap = null;
            }

            if (_tempNormalMap != null &&
                (_tempNormalMap.width != _dimensions[0] ||
                 _tempNormalMap.height != _dimensions[1]))
            {
                Destroy(_tempNormalMap);
                _tempNormalMap = null;
            }

            // Lazy initialization of temporary objects

            if (_positionBuffer == null)
                _positionBuffer = new ComputeBuffer(vcount_x3, sizeof(float));

            if (_normalBuffer == null)
                _normalBuffer = new ComputeBuffer(vcount_x3, sizeof(float));

            if (_tempPositionMap == null)
            {
                _tempPositionMap = new RenderTexture(
                    _positionMap.width, _positionMap.height, 0,
                    RenderTextureFormat.ARGBHalf
                );
                _tempPositionMap.enableRandomWrite = true;
                _tempPositionMap.Create();
            }

            if (_tempNormalMap == null)
            {
                _tempNormalMap = new RenderTexture(
                    _positionMap.width, _positionMap.height, 0,
                    RenderTextureFormat.ARGBHalf
                );
                _tempNormalMap.enableRandomWrite = true;
                _tempNormalMap.Create();
            }

            // Set data and execute the transfer task.

            _compute.SetInt("VertexCount", vcount);
            _compute.SetInts("MapDimensions", _dimensions);
            _compute.SetMatrix("Transform", _source.transform.localToWorldMatrix);

            _positionBuffer.SetData(_positionList);
            _normalBuffer.SetData(_normalList);

            _compute.SetBuffer(0, "PositionBuffer", _positionBuffer);
            _compute.SetBuffer(0, "NormalBuffer", _normalBuffer);

            _compute.SetTexture(0, "PositionMap", _tempPositionMap);
            _compute.SetTexture(0, "NormalMap", _tempNormalMap);

            _compute.Dispatch(0, _dimensions[0] / 8, _dimensions[0] / 8, 1);

            Graphics.CopyTexture(_tempPositionMap, _positionMap);
            Graphics.CopyTexture(_tempNormalMap, _normalMap);
        }

        bool _warned;

        bool CheckConsistency()
        {
            if (_warned) return false;

            if (_positionMap.width % 8 != 0 || _positionMap.height % 8 != 0)
            {
                Debug.LogError("Position map dimensions should be a multiple of 8.");
                _warned = true;
            }

            if (_normalMap.width != _positionMap.width ||
                _normalMap.height != _positionMap.height)
            {
                Debug.LogError("Position/normal map dimensions should match.");
                _warned = true;
            }

            if (_positionMap.format != RenderTextureFormat.ARGBHalf)
            {
                Debug.LogError("Position map format should be ARGBHalf");
                _warned = true;
            }

            if (_normalMap.format != RenderTextureFormat.ARGBHalf)
            {
                Debug.LogError("Normal map format should be ARGBHalf");
                _warned = true;
            }

            return !_warned;
        }

        #endregion
    }
}
