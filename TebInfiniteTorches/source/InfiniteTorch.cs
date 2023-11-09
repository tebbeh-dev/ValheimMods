using HarmonyLib;

namespace TebInfiniteTorches
{
    // Set fireplaces to have infinite fuel when they load
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Awake))]
    static class InfiniteFuel
    {
        [HarmonyPostfix]
        static void InfiniteFuelPrefix(Fireplace __instance)
        {
            if (Plugin.GetInfiniteFuel())
            {
                __instance.m_infiniteFuel = true;
            }
        }
    }

    // Change the result of the CheckWet method
    [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.CheckWet))]
    static class WeatherBlock
    {
        [HarmonyPrefix]
        static bool WeatherBlockPrefix(Fireplace __instance)
        {
            if (Plugin.GetWeather())
            {
                __instance.m_wet = false;
                return false;
            }

            return true;
        }
    }
}
