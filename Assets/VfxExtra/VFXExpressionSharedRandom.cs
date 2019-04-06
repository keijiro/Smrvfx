using UnityEngine.Experimental.VFX;
using UnityEditor.VFX;

namespace VfxExtra
{
    class VFXExpressionSharedRandom : VFXExpression
    {
        public VFXExpressionSharedRandom() : this(VFXValue<float>.Default) {}
        public VFXExpressionSharedRandom(VFXExpression hash) : base(Flags.None, hash) {}

        public override VFXExpressionOperation operation { get { return VFXExpressionOperation.GenerateFixedRandom; }}

        sealed protected override VFXExpression Evaluate(VFXExpression[] constParents)
        {
            var oldState = UnityEngine.Random.state;
            UnityEngine.Random.InitState((int)constParents[0].Get<uint>());

            var result = VFXValue.Constant(UnityEngine.Random.value);

            UnityEngine.Random.state = oldState;

            return result;
        }

        public override string GetCodeString(string[] parents)
        {
            return string.Format("FixedRand({0})", parents[0]);
        }
    }
}
