using UnityEngine;
using System.Collections.Generic;

namespace Smrvfx
{
    public sealed class SkinnedMeshBaker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] SkinnedMeshRenderer _source = null;
        [SerializeField] RenderTexture _positionMap = null;
        [SerializeField] RenderTexture _velocityMap = null;
        [SerializeField] RenderTexture _normalMap = null;
        [SerializeField] ComputeShader _compute = null;

        #endregion

        #region Temporary objects

        Mesh _mesh;
        Matrix4x4 _previousTransform = Matrix4x4.identity;

        int[] _dimensions = new int[2];

        List<Vector3> _positionList = new List<Vector3>();
        List<Vector3> _normalList = new List<Vector3>();

        ComputeBuffer _positionBuffer1;
        ComputeBuffer _positionBuffer2;
        ComputeBuffer _normalBuffer;

        RenderTexture _tempPositionMap;
        RenderTexture _tempVelocityMap;
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

            Utility.TryDispose(_positionBuffer1);
            Utility.TryDispose(_positionBuffer2);
            Utility.TryDispose(_normalBuffer);

            Utility.TryDestroy(_tempPositionMap);
            Utility.TryDestroy(_tempVelocityMap);
            Utility.TryDestroy(_tempNormalMap);

            _positionBuffer1 = null;
            _positionBuffer2 = null;
            _normalBuffer = null;

            _tempPositionMap = null;
            _tempVelocityMap = null;
            _tempNormalMap = null;
        }

        void Update()
        {
            if (_source == null) return;

            _source.BakeMesh(_mesh);
            _mesh.GetVertices(_positionList);
            _mesh.GetNormals(_normalList);

            if (!CheckConsistency()) return;

            TransferData();

            Utility.SwapBuffer(ref _positionBuffer1, ref _positionBuffer2);
            _previousTransform = _source.transform.localToWorldMatrix;
        }

        #endregion

        #region Private methods

        void TransferData()
        {
            var mapWidth = _positionMap.width;
            var mapHeight = _positionMap.height;

            var vcount = _positionList.Count;
            var vcount_x3 = vcount * 3;

            // Release the temporary objects when the size of them don't match
            // the input.

            if (_positionBuffer1 != null &&
                _positionBuffer1.count != vcount_x3)
            {
                _positionBuffer1.Dispose();
                _positionBuffer2.Dispose();
                _normalBuffer.Dispose();

                _positionBuffer1 = null;
                _positionBuffer2 = null;
                _normalBuffer = null;
            }

            if (_tempPositionMap != null &&
                (_tempPositionMap.width != mapWidth ||
                 _tempPositionMap.height != mapHeight))
            {
                Destroy(_tempPositionMap);
                Destroy(_tempVelocityMap);
                Destroy(_tempNormalMap);

                _tempPositionMap = null;
                _tempVelocityMap = null;
                _tempNormalMap = null;
            }

            // Lazy initialization of temporary objects

            if (_positionBuffer1 == null)
            {
                _positionBuffer1 = new ComputeBuffer(vcount_x3, sizeof(float));
                _positionBuffer2 = new ComputeBuffer(vcount_x3, sizeof(float));
                _normalBuffer = new ComputeBuffer(vcount_x3, sizeof(float));
            }

            if (_tempPositionMap == null)
            {
                _tempPositionMap = Utility.CreateRenderTexture(_positionMap);
                _tempVelocityMap = Utility.CreateRenderTexture(_positionMap);
                _tempNormalMap = Utility.CreateRenderTexture(_positionMap);
            }

            // Set data and execute the transfer task.

            _compute.SetInt("VertexCount", vcount);
            _compute.SetMatrix("Transform", _source.transform.localToWorldMatrix);
            _compute.SetMatrix("OldTransform", _previousTransform);
            _compute.SetFloat("FrameRate", 1 / Time.deltaTime);

            _positionBuffer1.SetData(_positionList);
            _normalBuffer.SetData(_normalList);

            _compute.SetBuffer(0, "PositionBuffer", _positionBuffer1);
            _compute.SetBuffer(0, "OldPositionBuffer", _positionBuffer2);
            _compute.SetBuffer(0, "NormalBuffer", _normalBuffer);

            _compute.SetTexture(0, "PositionMap", _tempPositionMap);
            _compute.SetTexture(0, "VelocityMap", _tempVelocityMap);
            _compute.SetTexture(0, "NormalMap", _tempNormalMap);

            _compute.Dispatch(0, mapWidth / 8, mapHeight / 8, 1);

            Graphics.CopyTexture(_tempPositionMap, _positionMap);
            Graphics.CopyTexture(_tempVelocityMap, _velocityMap);
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

            if (_positionMap.format != RenderTextureFormat.ARGBHalf &&
                _positionMap.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Position map format should be ARGBHalf or ARGBFloat.");
                _warned = true;
            }

            if (_normalMap.format != RenderTextureFormat.ARGBHalf &&
                _normalMap.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Normal map format should be ARGBHalf or ARGBFloat.");
                _warned = true;
            }

            return !_warned;
        }

        #endregion
    }
}
