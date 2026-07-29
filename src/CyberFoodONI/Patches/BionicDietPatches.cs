using System.Collections.Generic;
using HarmonyLib;

namespace CyberFoodONI.Patches;

[HarmonyPatch(
    typeof(ConsumerManager),
    nameof(ConsumerManager.BionicDuplicantDietaryRestrictions),
    MethodType.Getter)]
internal static class BionicDietaryRestrictionsPatch
{
    private static void Postfix(List<Tag> __result)
    {
        __result.RemoveAll(IsOrdinaryFood);
    }

    private static bool IsOrdinaryFood(Tag tag)
    {
        UnityEngine.GameObject prefab = Assets.GetPrefab(tag);
        return prefab != null && prefab.HasTag(GameTags.Edible);
    }
}
