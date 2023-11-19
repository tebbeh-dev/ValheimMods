using HarmonyLib;
using UnityEngine;

namespace TebFireTweaks
{
    // Set maxfuel on creation of what ever calling cookingstation
    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.Awake))]
    static class CookingStationInfiniteAwake
    {
        [HarmonyPostfix]
        static void CookingStationInfiniteAwakeState(CookingStation __instance)
        {
            // If Cooking Speed has a value other than default
            if (TebFireTweaks.GetCookingCookingSpeedInfinite() != 0)
            {
                float configValue = TebFireTweaks.GetCookingCookingSpeedInfinite();
                float newValue = (1 + (configValue / 100));

                for (int i = 0; i < __instance.m_conversion.Count; i++)
                {
                    __instance.m_conversion[i].m_cookTime *= newValue;
                }
            }
        }
    }

    // Set maxfuel on cookingstation if going below or equal with 1
    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateFuel))]
    static class CookingStationInfiniteSetFuel
    {
        [HarmonyPrefix]
        private static void CookingStationSetFuel(CookingStation __instance)
        {
            if (!TebFireTweaks.GetCookingStationInfinite())
            {
                return;
            }

            if (__instance.GetFuel() <= 1)
            {
                __instance.SetFuel(__instance.m_maxFuel);
            }
        }
    }

    [HarmonyPatch(typeof(CookingStation), nameof(CookingStation.UpdateCooking))]
    static class CookingStationUpdateCooking
    {
        private static Vector3 userPoint;

        [HarmonyPrefix]
        private static void CookingStationUpdateCookingTweak(CookingStation __instance, ZNetView ___m_nview)
        {

            for (int i = 0; i < __instance.m_slots.Length; i++)
            {

                string @string = ___m_nview.GetZDO().GetString("slot" + i.ToString(), "");
                float @float = ___m_nview.GetZDO().GetFloat("slot" + i.ToString(), 0f);
                int @int = ___m_nview.GetZDO().GetInt("slotstatus" + i.ToString(), 0);

                string itemName;
                float time;
                CookingStation.Status status;

                __instance.GetSlot(i, out itemName, out time, out status);

                float cookingTimeStandardValue = __instance.m_conversion[i].m_cookTime;

                if (itemName != "")
                {
                    if (status == CookingStation.Status.Done)
                    {
                        __instance.RPC_RemoveDoneItem(i, userPoint);
                    }
                }
            }


            if (!TebFireTweaks.GetCookingStationInfinite())
            {
                return;
            }

            if (__instance.GetFuel() <= 0)
            {
                __instance.SetFuel(__instance.m_maxFuel);
            }
        }
    }
}
