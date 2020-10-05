Smrvfx
------

![gif](https://i.imgur.com/HWwnljE.gif)
![gif](https://i.imgur.com/Tk1IlOb.gif)

**Smrvfx** is a Unity sample project that shows how to use an animated [skinned
mesh] as a particle source in a [visual effect graph].

[skinned mesh]: https://docs.unity3d.com/Manual/class-SkinnedMeshRenderer.html
[visual effect graph]: https://unity.com/visual-effect-graph

Uncharted Limbo's additions
-------------------
- Corrected transformation matrices when **multiple** Skinned Mesh Renderers are used as input, by applying an `IJobParallelFor` job on the vertices NativeArray before these are sent to their respective ComputeBuffer.

System Requirements
-------------------

- Unity 2020.1

