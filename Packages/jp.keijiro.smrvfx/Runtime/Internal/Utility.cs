using UnityEngine;
using Unity.Collections;

namespace Smrvfx {

static class MemoryUtil
{
    public static NativeArray<T> Array<T>(int length) where T : struct
        => new NativeArray<T>(length, Allocator.Persistent,
                              NativeArrayOptions.UninitializedMemory);

    public static NativeArray<T> TempArray<T>(int length) where T : struct
        => new NativeArray<T>(length, Allocator.Temp,
                              NativeArrayOptions.UninitializedMemory);

    public static NativeArray<T> TempJobArray<T>(int length) where T : struct
        => new NativeArray<T>(length, Allocator.TempJob,
                              NativeArrayOptions.UninitializedMemory);
}

static class RenderTextureUtil
{
    public static RenderTexture AllocateHalf(int width, int height)
      => Allocate(width, height, RenderTextureFormat.ARGBHalf);

    public static RenderTexture AllocateFloat(int width, int height)
      => Allocate(width, height, RenderTextureFormat.ARGBFloat);

    public static RenderTexture
      Allocate(int width, int height, RenderTextureFormat format)
    {
        var rt = new RenderTexture(width, height, 0, format);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

} // namespace Smrvfx
