using System.Collections;
using System.Collections.Generic;
using Smrvfx;
using UnityEngine;

namespace Smrvfx
{
    /// <summary>
    /// Class used to indirectly set an array of SkinnedMeshRenderers to a SkinnedMeshBaker
    /// </summary>
    public class SkinnedMeshBakerObjectInjector : MonoBehaviour 
    {
        public SkinnedMeshBaker smrBaker; 
        public SkinnedMeshRenderer[] skins;

        private void Awake()
        {
            if (smrBaker == null)
                return;

            if (skins == null)
                return;

            smrBaker.Sources = skins;
        }
    }
}
