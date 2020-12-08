Smrvfx
------

![gif](https://i.imgur.com/HWwnljE.gif)
![gif](https://i.imgur.com/Tk1IlOb.gif)

**Smrvfx** is a Unity sample project that shows how to use an animated [skinned
mesh] as a particle source in a [visual effect graph].

[skinned mesh]: https://docs.unity3d.com/Manual/class-SkinnedMeshRenderer.html
[visual effect graph]: https://unity.com/visual-effect-graph

System Requirements
-------------------

- Unity 2020.1

How To Install
--------------

This package uses the [scoped registry] feature to resolve package dependencies.
Please add the following sections to the manifest file (Packages/manifest.json).

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html

To the `scopedRegistries` section:

```
{
  "name": "Keijiro",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.keijiro" ]
}
```

To the `dependencies` section:

```
"jp.keijiro.smrvfx": "1.1.6"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.smrvfx": "1.1.6",
...
```
