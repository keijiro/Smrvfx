using UnityEngine;

namespace Smrvfx {

public sealed partial class SkinnedMeshBaker : MonoBehaviour
{
    #region Editor-only property

    [SerializeField] SkinnedMeshRenderer [] _sources = null;
    [SerializeField] int _pointCount = 65536;

    void OnValidate()
    {
        _pointCount = Mathf.Max(64, _pointCount);

        // We assume that someone changed the values/references in the
        // serialized fields, so let us dispose the internal objects to
        // re-initialize them with the new values/references. #BADCODE
        DisposeInternals();
    }

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
