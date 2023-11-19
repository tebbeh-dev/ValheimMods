using HarmonyLib;

namespace TebFireTweaks
{
    // Set fireplaces to have infinite fuel when they load
    [HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
    static class SmelterInfiniteAwake
    {
        [HarmonyPostfix]
        static void SmelterInfiniteAwakeState(Smelter __instance)
        {
            if (!TebFireTweaks.GetSmelterInfinite())
            {
                return;
            }

            __instance.SetFuel(__instance.m_maxFuel);
        }
    }

    // Set fireplaces to have infinite fuel when they load
    [HarmonyPatch(typeof(Smelter), nameof(Smelter.UpdateState))]
    static class SmelterInfiniteUpdateState
    {
        [HarmonyPostfix]
        static void SmelterInfiniteClass(Smelter __instance)
        {
            if (!TebFireTweaks.GetSmelterInfinite())
            {
                return;
            }

            if (!__instance.IsActive())
            {
                __instance.IsActive();
                __instance.SetFuel(__instance.m_maxFuel);
            }
        }
    }
}
