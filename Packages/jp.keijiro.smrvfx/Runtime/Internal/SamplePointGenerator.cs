using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace Smrvfx {

//
// Single-method static class for generating sample points using C# jobs
//
static class SamplePointGenerator
{
    #region Public method

    public static NativeArray<SamplePoint>
      Generate(CombinedMesh mesh, int pointCount)
    {
        var output = MemoryUtil.Array<SamplePoint>(pointCount);
        new GenerationJob
          { Vertices = mesh.Vertices, Indices = mesh.Indices,
            TotalArea = CalculateTotalArea(mesh), Output = output }.Run();
        return output;
    }

    #endregion

    #region Private method

    public static float CalculateTotalArea(CombinedMesh mesh)
    {
        using (var temp = MemoryUtil.Array<float>(1))
        {
            new AccumulationJob
              { Vertices = mesh.Vertices, Indices = mesh.Indices,
                Output = temp }.Run();
            return temp[0];
        }
    }

    #endregion

    #region Triangle area accumulation job

    [BurstCompile(CompileSynchronously = true,
                  FloatMode = FloatMode.Fast,
                  FloatPrecision = FloatPrecision.Low)]
    struct AccumulationJob : IJob
    {
        [ReadOnly] public NativeArray<float3> Vertices;
        [ReadOnly] public NativeArray<int> Indices;
        [WriteOnly] public NativeArray<float> Output;

        public void Execute()
        {
            var area = 0.0f;
            for (var i = 0; i < Indices.Length; i +=3 )
            {
                var v1 = Vertices[Indices[i + 0]];
                var v2 = Vertices[Indices[i + 1]];
                var v3 = Vertices[Indices[i + 2]];
                area += MathUtil.TriangleArea(v1, v2, v3);
            }
            Output[0] = area;
        }
    }

    #endregion

    #region Sample point generation job

    [BurstCompile(CompileSynchronously = true,
                  FloatMode = FloatMode.Fast,
                  FloatPrecision = FloatPrecision.Low)]
    struct GenerationJob : IJob
    {
        [ReadOnly] public NativeArray<float3> Vertices;
        [ReadOnly] public NativeArray<int> Indices;
        [WriteOnly] public NativeArray<SamplePoint> Output;

        public float TotalArea;

        public void Execute()
        {
            var rnd = new Random(39208); // Meaningless magic number
            rnd.NextUInt4();             // Warming up

            var areaPerSample = TotalArea / (Output.Length - 0.5f);

            var acc = 0.0f;
            var offs = 0;

            for (var i = 0; i < Indices.Length; i += 3)
            {
                var i1 = Indices[i + 0];
                var i2 = Indices[i + 1];
                var i3 = Indices[i + 2];

                var v1 = Vertices[i1];
                var v2 = Vertices[i2];
                var v3 = Vertices[i3];

                acc += MathUtil.TriangleArea(v1, v2, v3);

                for (; acc > areaPerSample; acc -= areaPerSample)
                {
                    var r = rnd.NextFloat2();
                    if (r.x + r.y > 1) r = 1 - r;

                    Output[offs++] =
                      new SamplePoint(i1, 1 - r.x - r.y, i2, r.x, i3, r.y);
                }
            }
        }
    }

    #endregion
}

} // namespace Smrvfx
