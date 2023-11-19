using HarmonyLib;

namespace TebFireTweaks
{
    // Set fireplaces to have infinite fuel when they load
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Awake))]
    static class FireplaceInfinite
    {
        [HarmonyPostfix]
        static void FireplaceInfiniteClass(Fireplace __instance)
        {
            if (TebFireTweaks.GetFireplaceInfinite())
            {
                __instance.m_infiniteFuel = true;
            }
        }
    }

    // Change the result of the CheckWet method
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.CheckWet))]
    static class FireplaceWeatherBlock
    {
        [HarmonyPrefix]
        static bool FireplaceWeatherBlockClass(Fireplace __instance)
        {
            if (TebFireTweaks.GetFireplaceWeatherBlock())
            {
                __instance.m_wet = false;
                return false;
            }

            return true;
        }
    }
}
