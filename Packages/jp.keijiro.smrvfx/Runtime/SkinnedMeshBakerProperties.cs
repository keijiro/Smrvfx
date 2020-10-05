using UnityEngine;

namespace Smrvfx {

public sealed partial class SkinnedMeshBaker : MonoBehaviour
{
    //--------------------------------------------------------------
    // Uncharted Limbo-Added
    //--------------------------------------------------------------
    // Exposed the _sources field to external scripts
    #region Public properties

    public SkinnedMeshRenderer[] Sources
    {
        get => _sources;
        set => _sources = value;
    }

    public Matrix4x4 GlobalTransformationMatrix
    {
        get
        {
            if (root == null)
                return Matrix4x4.identity;

            return root.localToWorldMatrix;
        }
    }

    #endregion
    //--------------------------------------------------------------

    #region Editor-only property
    //--------------------------------------------------------------
    // Uncharted Limbo-Added
    //--------------------------------------------------------------
    // Added a separate root transformation object, instead of using the first source
    [SerializeField] Transform  root;
    //--------------------------------------------------------------
    [SerializeField] SkinnedMeshRenderer [] _sources = null;
    [SerializeField] int _pointCount = 65536;

    void OnValidate()
      => _pointCount = Mathf.Max(64, _pointCount);

    #endregion

    #region Hidden asset reference

    [SerializeField, HideInInspector] ComputeShader _compute = null;

    #endregion

    #region Runtime-only properties

    public Texture PositionMap => _positionMap;
    public Texture VelocityMap => _velocityMap;
    public Texture NormalMap => _normalMap;
    public int VertexCount => _pointCount;

    #endregion

    #region Runtime utility

    bool IsValid
      => _sources != null && _sources.Length > 0;

    #endregion
}

} // namespace Smrvfx
