using UnityEngine;
using System.Collections.Generic;

namespace Smrvfx
{
    public sealed class SkinnedMeshBaker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] SkinnedMeshRenderer _source = null;
        [SerializeField] ComputeShader _compute = null;

        #endregion

        #region Public properties

        public Texture PositionMap => _positionMap;
        public Texture VelocityMap => _velocityMap;
        public Texture NormalMap => _normalMap;
        public int VertexCount => _mesh != null ? _mesh.vertexCount : 0;

        #endregion

        #region Temporary objects

        Mesh _mesh;
        Matrix4x4 _previousTransform = Matrix4x4.identity;

        List<Vector3> _positionList = new List<Vector3>();
        List<Vector3> _normalList = new List<Vector3>();

        ComputeBuffer _positionBuffer1;
        ComputeBuffer _positionBuffer2;
        ComputeBuffer _normalBuffer;

        RenderTexture _positionMap;
        RenderTexture _velocityMap;
        RenderTexture _normalMap;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _mesh = new Mesh();

            _source.BakeMesh(_mesh);

            var vcount = _mesh.vertexCount;
            var vcount_x3 = vcount * 3;

            _positionBuffer1 = new ComputeBuffer(vcount_x3, sizeof(float));
            _positionBuffer2 = new ComputeBuffer(vcount_x3, sizeof(float));
            _normalBuffer = new ComputeBuffer(vcount_x3, sizeof(float));

            var width = 256;
            var height = (((vcount + width - 1) / width + 7) / 8) * 8;

            _positionMap = NewFloatRenderTexture(width, height);
            _velocityMap = NewHalfRenderTexture(width, height);
            _normalMap = NewHalfRenderTexture(width, height);
        }

        void OnDestroy()
        {
            Destroy(_mesh);

            _positionBuffer1.Dispose();
            _positionBuffer2.Dispose();
            _normalBuffer.Dispose();

            Destroy(_positionMap);
            Destroy(_velocityMap);
            Destroy(_normalMap);
        }

        void Update()
        {
            _source.BakeMesh(_mesh);

            _mesh.GetVertices(_positionList);
            _mesh.GetNormals(_normalList);

            TransferData();
            SwapPositionBuffers();

            _previousTransform = _source.transform.localToWorldMatrix;
        }

        #endregion

        #region Render texture utilities

        static RenderTexture NewRenderTexture
          (int width, int height, RenderTextureFormat format)
        {
            var rt = new RenderTexture(width, height, 0, format);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        static RenderTexture NewHalfRenderTexture(int width, int height)
          => NewRenderTexture(width, height, RenderTextureFormat.ARGBHalf);

        static RenderTexture NewFloatRenderTexture(int width, int height)
          => NewRenderTexture(width, height, RenderTextureFormat.ARGBFloat);

        #endregion

        #region Buffer operations

        void TransferData()
        {
            var width = _positionMap.width;
            var height = _positionMap.height;

            var vcount = _positionList.Count;
            var vcount_x3 = vcount * 3;

            var l2w = _source.transform.localToWorldMatrix;

            _compute.SetInt("VertexCount", vcount);
            _compute.SetMatrix("Transform", l2w);
            _compute.SetMatrix("OldTransform", _previousTransform);
            _compute.SetFloat("FrameRate", 1 / Time.deltaTime);

            _positionBuffer1.SetData(_positionList);
            _normalBuffer.SetData(_normalList);

            _compute.SetBuffer(0, "PositionBuffer", _positionBuffer1);
            _compute.SetBuffer(0, "OldPositionBuffer", _positionBuffer2);
            _compute.SetBuffer(0, "NormalBuffer", _normalBuffer);

            _compute.SetTexture(0, "PositionMap", _positionMap);
            _compute.SetTexture(0, "VelocityMap", _velocityMap);
            _compute.SetTexture(0, "NormalMap", _normalMap);

            _compute.Dispatch(0, width / 8, height / 8, 1);
        }

        void SwapPositionBuffers()
        {
            var temp = _positionBuffer1;
            _positionBuffer1 = _positionBuffer2;
            _positionBuffer2 = temp;
        }

        #endregion
    }
}
