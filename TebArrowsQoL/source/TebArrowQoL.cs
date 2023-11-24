using HarmonyLib;
using UnityEngine;

namespace TebArrowsQoL
{
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Setup))]
    static class GetArrowBackOnHit
    {
        [HarmonyPostfix]
        static void ChanceToGetArrowBackOnHit(Projectile __instance)
        {
            if (TebArrowsQoL.GetChanceToSaveArrowsOn())
            {
                Player player = Player.m_localPlayer;
                string arrowType = player.GetAmmoItem().m_dropPrefab.gameObject.name;

                GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(arrowType);

                if (!arrowType.Contains("arrow"))
                {
                    return;
                }

                float value = TebArrowsQoL.GetChanceToSaveArrowsValue();

                if (value != 100)
                {
                    System.Random random = new System.Random();
                    int randomNumber = random.Next(0, 101);

                    if (value < randomNumber)
                    {
                        __instance.m_spawnOnHit = itemPrefab;
                    }

                    return;
                }

                __instance.m_spawnOnHit = itemPrefab;
            }
        }
    }
}
