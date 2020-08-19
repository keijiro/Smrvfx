using UnityEngine;

namespace Smrvfx {

struct Bundle : System.IDisposable
{
    public SkinnedMeshRenderer Source;
    public Mesh BakedMesh;

    public Bundle(SkinnedMeshRenderer source)
    {
        Source = source;
        BakedMesh = new Mesh();
        Source.BakeMesh(BakedMesh);
    }

    public void Dispose()
    {
        Object.Destroy(BakedMesh);
        BakedMesh = null;
    }

    public void Bake()
      => Source.BakeMesh(BakedMesh);
}

} // namespace Smrvfx
