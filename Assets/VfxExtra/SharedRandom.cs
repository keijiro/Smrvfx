using UnityEngine;
using UnityEditor.VFX;

namespace VfxExtra
{
    [VFXInfo(category = "Random")]
    class SharedRandom : VFXOperator
    {
        override public string name { get { return "Shared Random Number"; } }

        public class InputProperties
        {
            [Tooltip("The minimum value to be generated.")]
            public float min = 0.0f;
            [Tooltip("The maximum value to be generated.")]
            public float max = 1.0f;
            [Tooltip("Seed to compute the constant random")]
            public uint seed = 0u;
        }

        public class OutputProperties
        {
            [Tooltip("A random number between 0 and 1.")]
            public float r = 0;
        }

        protected override sealed VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            return new[] { VFXOperatorUtility.Lerp(inputExpression[0], inputExpression[1], new VFXExpressionSharedRandom(inputExpression[2])) };
        }
    }
}
