using HarmonyLib;
using UnityEngine;
using static PrivilegeManager;

namespace Tebbeh.TebBoatTweaks
{
    static class SailSpeed
    {
        [HarmonyPatch(typeof(Ship), nameof(Ship.GetSailForce))]
        private class ChangeShipBaseSpeed
        {
            private static void Postfix(ref Vector3 __result)
            {
                __result *= TebBoatTweaks.GetSailSpeed();
            }
        }
    }
    
    [HarmonyPatch(typeof(Ship), nameof(Ship.Start))]
    static class RudderSpeed
    {
        private static void Prefix(Ship __instance)
        {
            int backwardForce = TebBoatTweaks.GetRowingSpeed();
            __instance.m_backwardForce *= backwardForce;
        }
    }
}