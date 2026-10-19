using UnityEngine;

namespace GameAssembly.Core
{
    public static class LayerService
    {
        public static bool IsLayerEquals(LayerMask objectLayer, LayerMask requiredLayer) => ((1 << objectLayer) & requiredLayer) > 0;
    }
}