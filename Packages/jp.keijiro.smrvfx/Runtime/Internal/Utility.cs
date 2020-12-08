using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace Smrvfx {

static class MathUtil
{
    public static float TriangleArea(float3 v1, float3 v2, float3 v3)
      => math.length(math.cross(v2 - v1, v3 - v1)) / 2;
}

static class ObjectUtil
{
    public static void Destroy(Object o)
    {
        if (o == null) return;
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}

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
        rt.hideFlags = HideFlags.DontSave;
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

} // namespace Smrvfx
