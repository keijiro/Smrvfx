using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace Smrvfx {

//
// Aggregation class for combining meshes and submeshes
//
class CombinedMesh : System.IDisposable
{
    #region Public fields

    public NativeArray<float3> Vertices;
    public NativeArray<float3> Normals;
    public NativeArray<int> Indices;

    #endregion

    #region Constructor

    public CombinedMesh(IEnumerable<Mesh> meshes)
      => Initialize(meshes.ToArray());

    public CombinedMesh(IEnumerable<SkinnedMeshRenderer> sources)
      => Initialize(sources.Select(smr => smr.sharedMesh).ToArray());

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
        if (Vertices.IsCreated) Vertices.Dispose();
        if (Normals.IsCreated) Normals.Dispose();
        if (Indices.IsCreated) Indices.Dispose();
    }

    #endregion

    #region Private methods

    void Initialize(Mesh[] meshes)
    {
        // Managed mesh array -> Read-only mesh data array
        using (var dataArray = Mesh.AcquireReadOnlyMeshData(meshes))
        {
            // Vertex/index count calculation
            var vtotal = 0;
            var itotal = 0;

            for (var i = 0; i < dataArray.Length; i++)
            {
                var data = dataArray[i];
                vtotal += data.vertexCount;
                itotal += CountIndices(data);
            }

            // Buffer allocation
            Vertices = MemoryUtil.Array<float3>(vtotal);
            Normals = MemoryUtil.Array<float3>(vtotal);
            Indices = MemoryUtil.Array<int>(itotal);

            // Vertex/index data retrieval
            var voffs = 0;
            var ioffs = 0;

            for (var i = 0; i < dataArray.Length; i++)
            {
                var data = dataArray[i];

                var vcount = data.vertexCount;
                var icount = CountIndices(data);

                var varray = Vertices.GetSubArray(voffs, vcount);
                var iarray = Indices.GetSubArray(ioffs, icount);

                data.GetVertices(varray.Reinterpret<Vector3>());
                ConcatenateIndices(data, iarray, voffs);

                voffs += vcount;
                ioffs += icount;
            }
        }
    }

    #endregion

    #region Private method

    static int CountIndices(in Mesh.MeshData mesh)
    {
        var count = 0;
        for (var i = 0; i < mesh.subMeshCount; i++)
        {
            var desc = mesh.GetSubMesh(i);
            count += desc.indexCount;
        }
        return count;
    }

    static void ConcatenateIndices
      (in Mesh.MeshData mesh, NativeArray<int> output, int indexOffset)
    {
        new ConcatenationJob
          { Output = output, Mesh = mesh, IndexOffset = indexOffset }.Run();
    }

    #endregion

    #region Index array concatenation job

    [BurstCompile(CompileSynchronously = true)]
    struct ConcatenationJob : IJob
    {
        public NativeArray<int> Output;
        public Mesh.MeshData Mesh;
        public int IndexOffset;

        public void Execute()
        {
            var filled = 0;
            for (var i = 0; i < Mesh.subMeshCount; i++)
            {
                var desc = Mesh.GetSubMesh(i);
                var sub = Output.GetSubArray(filled, desc.indexCount);
                Mesh.GetIndices(sub, i);
                for (var j = 0; j < sub.Length; j++) sub[j] += IndexOffset;
                filled += desc.indexCount;
            }
        }
    }

    #endregion
}

} // namespace Smrvfx
