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

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _positionMapProperty = "PositionMap";

        public bool _bindVelocityMap = false;

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _velocityMapProperty = "VelocityMap";

        public bool _bindNormalMap = false;

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _normalMapProperty = "NormalMap";

        public bool _bindVertexCount = false;

        [VFXPropertyBinding("System.UInt32"), SerializeField]
        ExposedProperty _vertexCountProperty = "VertexCount";

        public SkinnedMeshBaker Target = null;

        public override bool IsValid(VisualEffect component)
          => Target != null &&
           component.HasTexture(_positionMapProperty) &&
           (!_bindVelocityMap || component.HasTexture(_velocityMapProperty)) &&
           (!_bindNormalMap   || component.HasTexture(_normalMapProperty)) &&
           (!_bindVertexCount || component.HasUInt(_vertexCountProperty));

        public override void UpdateBinding(VisualEffect component)
        {
            if (Target.PositionMap == null) return;

            component.SetTexture(_positionMapProperty, Target.PositionMap);

            if (_bindVelocityMap)
                component.SetTexture(_velocityMapProperty, Target.VelocityMap);

            if (_bindNormalMap)
                component.SetTexture(_normalMapProperty, Target.NormalMap);

            if (_bindVertexCount)
                component.SetUInt(_vertexCountProperty, (uint)Target.VertexCount);
        }

        public override string ToString()
          => $"Skinned Mesh : '{_positionMapProperty}' -> {Target?.name ?? "(null)"}";
    }
}
