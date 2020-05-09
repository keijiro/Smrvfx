using UnityEngine;

namespace Smrvfx
{
    internal static class Utility
    {
        public static RenderTexture CreateRenderTexture(RenderTexture source)
        {
            var rt = new RenderTexture(source.width, source.height, 0, source.format);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        public static void TryDispose(System.IDisposable obj)
        {
            if (obj == null) return;
            obj.Dispose();
        }

        public static void TryDestroy(UnityEngine.Object obj)
        {
            if (obj == null) return;
            UnityEngine.Object.Destroy(obj);
        }

        public static void SwapBuffer(ref ComputeBuffer b1, ref ComputeBuffer b2)
        {
            var temp = b1;
            b1 = b2;
            b2 = temp;
        }
    }
}
