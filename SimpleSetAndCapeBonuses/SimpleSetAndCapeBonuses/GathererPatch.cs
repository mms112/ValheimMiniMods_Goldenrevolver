using System;
using HarmonyLib;
using UnityEngine;

namespace SimpleSetAndCapeBonuses
{
    [HarmonyPatch]
    internal class GathererPatch
    {
        static bool injectBonus = false;

        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
        public static void Prefix(Pickable __instance, Humanoid character)
        {
            if (__instance.m_picked)
            {
                return;
            }

            if (!character.m_seman.HaveStatusEffect(PatchObjectDB.ragsSetBonusHash))
            {
                return;
            }

            if (IsForage(__instance))
            {
                if (SimpleSetAndCapeBonusesPlugin.ForagerSetBonusExtraChance.Value >= 1f || UnityEngine.Random.value < SimpleSetAndCapeBonusesPlugin.ForagerSetBonusExtraChance.Value)
                {
                    injectBonus = true;
                    DamageText.instance.ShowText(DamageText.TextType.Bonus, __instance.transform.position + Vector3.up * __instance.m_spawnOffset, $"+1", player: true);
                    __instance.m_bonusEffect.Create(__instance.transform.position, Quaternion.identity);
                }
            }
        }

        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
        public static void Postfix(Pickable __instance, ref int __state)
        {
            injectBonus = false;
        }

        [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.InvokeRPC), new Type[] { typeof(string), typeof(object[]) })]
        [HarmonyPrefix]
        static void InjectBonusAmount(string method, params object[] parameters)
        {
            if (injectBonus && method == "RPC_Pick")
                parameters[0] = (int)parameters[0] + 1;
        }

        private static readonly string[] allowedPickables = new string[]
        {
                "Pickable_Branch",
                "Pickable_Stone",
                "Pickable_Flint",

                "Pickable_Mushroom",
                // intentional lower casing
                "Pickable_Mushroom_yellow",
                // intentional inclusion of unobtainable item for my other mod 'Immersively Obtainable Blue Mushrooms'
                "Pickable_Mushroom_blue",

                "Pickable_Dandelion",
                "Pickable_Thistle",

                "RaspberryBush",
                "BlueberryBush",
                "CloudberryBush",

                //"Pickable_Barley_Wild",
                //"Pickable_Flax_Wild",

                //"Pickable_Tar",
                //"Pickable_TarBig"
        };

        private static bool IsForage(Pickable pickup)
        {
            foreach (var item in allowedPickables)
            {
                // missing bracket is intentional in case its a clone of a clone
                if (pickup.name.StartsWith(item + "(Clone"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}