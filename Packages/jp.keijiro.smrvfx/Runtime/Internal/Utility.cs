using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;

namespace Smrvfx {

static class MathUtil
{
    public static float TriangleArea(float3 v1, float3 v2, float3 v3)
      => math.length(math.cross(v2 - v1, v3 - v1)) / 2;
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
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}

//--------------------------------------------------------------
// Uncharted Limbo-Added
//--------------------------------------------------------------
internal static class JobUtil
{

    public static void TransformVertices(NativeArray<float3> vertices, float4x4 matrix)
    {
        new MatrixTransformationJob
        {
            verts = vertices,
            trans = matrix
        }.Schedule(vertices.Length, 128).Complete();
    }


    [BurstCompile]
     struct MatrixTransformationJob:IJobParallelFor
    {
        public NativeArray<float3> verts;
        public float4x4            trans;

        public void Execute(int index)
        {
            var pos = new float4(verts[index],1);
            verts[index] = math.mul(trans, pos).xyz;
        }
    }
}

} // namespace Smrvfx
