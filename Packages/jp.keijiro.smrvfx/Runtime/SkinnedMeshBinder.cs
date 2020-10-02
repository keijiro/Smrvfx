using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace Smrvfx
{
    [AddComponentMenu("VFX/Property Binders/Smrvfx/Skinned Mesh Binder")]
    [VFXBinder("Smrvfx/Skinned Mesh")]
    sealed class VFXSkinnedMeshBinder : VFXBinderBase
    {
        public string PositionMapProperty {
            get => (string)_positionMapProperty;
            set => _positionMapProperty = value;
        }

        public string VelocityMapProperty {
            get => (string)_velocityMapProperty;
            set => _velocityMapProperty = value;
        }

        public string NormalMapProperty {
            get => (string)_normalMapProperty;
            set => _normalMapProperty = value;
        }

        public string VertexCountProperty {
            get => (string)_vertexCountProperty;
            set => _vertexCountProperty = value;
        }

        public string BoundsProperty {
            get => (string)_boundsProperty;
            set
            {
                _boundsProperty = value;
                UpdateSubProperties();
            }
        }

        [Header("Position Map")]
        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _positionMapProperty = "PositionMap";

        [Header("Velocity Map")]
        public bool _bindVelocityMap = false;

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _velocityMapProperty = "VelocityMap";

        [Header("Normal Map")]
        public bool _bindNormalMap = false;

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _normalMapProperty = "NormalMap";

        [Header("Vertex Count")]
        public bool _bindVertexCount = false;

        [VFXPropertyBinding("System.UInt32"), SerializeField]
        ExposedProperty _vertexCountProperty = "VertexCount";

        [Header("Bounds")]
        public bool _bindBounds = false;

        [Range(0, 10)]
        [Tooltip("Expands the bounding box in every direction by given distance.")]
        public float _boundsExpansion;

        [Tooltip("Name of the AABox property to bind to.")]
        [VFXPropertyBinding("UnityEditor.VFX.AABox"), SerializeField]
        ExposedProperty _boundsProperty = "Bounds_size";

        [Space(32)]
        public SkinnedMeshBaker Target = null;

        ExposedProperty _boundsCenterProperty;
        ExposedProperty _boundsSizeProperty;

        protected override void OnEnable()
        {
            UpdateSubProperties();
            base.OnEnable();
        }

        public override bool IsValid(VisualEffect component)
          => Target != null &&
           component.HasTexture(_positionMapProperty) &&
           (!_bindVelocityMap || component.HasTexture(_velocityMapProperty)) &&
           (!_bindNormalMap   || component.HasTexture(_normalMapProperty)) &&
           (!_bindVertexCount || component.HasUInt(_vertexCountProperty)) &&
           (!_bindBounds      || component.HasVector3(_boundsCenterProperty) && component.HasVector3(_boundsSizeProperty));

        public override void UpdateBinding(VisualEffect component)
        {
            component.SetTexture(_positionMapProperty, Target.PositionMap);

            if (_bindVelocityMap)
                component.SetTexture(_velocityMapProperty, Target.VelocityMap);

            if (_bindNormalMap)
                component.SetTexture(_normalMapProperty, Target.NormalMap);

            if (_bindVertexCount)
                component.SetUInt(_vertexCountProperty, (uint)Target.VertexCount);

            if (_bindBounds)
            {
                var bounds = Target.Bounds;
                bounds.Expand(_boundsExpansion);
                component.SetVector3(_boundsCenterProperty, bounds.center);
                component.SetVector3(_boundsSizeProperty, bounds.size);
            }
        }

        void OnValidate()
            => UpdateSubProperties();

        void UpdateSubProperties()
        {
            _boundsCenterProperty = _boundsProperty + "_center";
            _boundsSizeProperty = _boundsProperty + "_size";
        }

        public override string ToString()
          => $"Skinned Mesh : '{_positionMapProperty}' -> {Target?.name ?? "(null)"}";
    }
}
