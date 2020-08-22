using System.Runtime.InteropServices;

namespace Smrvfx {

// Sample point structure with barycentric coordinates
[StructLayout(LayoutKind.Sequential)]
struct SamplePoint
{
    public int Index1, Index2, Index3, __Padding1;
    public float Weight1, Weight2, Weight3, __Padding2;

    public static int SizeInByte => sizeof(int) * 4 + sizeof(float) * 4;

    public SamplePoint(int i1, float w1, int i2, float w2, int i3, float w3)
    {
        Index1 = i1; Index2 = i2; Index3 = i3; __Padding1 = 0;
        Weight1 = w1; Weight2 = w2; Weight3 = w3; __Padding2 = 0;
    }
}

} // namespace Smrvfx
