using HarmonyLib;
using Klei.AI;

namespace CyberFoodONI.Patches;

internal static class BionicDiningNeed
{
    internal static bool IsBionic(UnityEngine.GameObject gameObject)
    {
        return gameObject != null &&
               gameObject.HasTag(GameTags.Minions.Models.Bionic);
    }

    internal static bool IsBionic(CalorieMonitor.Instance instance)
    {
        KPrefabID prefabId = instance.GetComponent<KPrefabID>();
        return prefabId != null && prefabId.HasTag(GameTags.Minions.Models.Bionic);
    }

    internal static bool HasSensoryDiningEffect(CalorieMonitor.Instance instance)
    {
        Effects effects = instance.GetComponent<Effects>();
        return effects != null && effects.HasEffect(ModInfo.SensoryDiningEffectId);
    }
}

[HarmonyPatch(typeof(EatChore), nameof(EatChore.Begin))]
internal static class BionicEatChoreBeginPatch
{
    private static void Prefix(Chore.Precondition.Context context)
    {
        UnityEngine.GameObject eater = context.consumerState.gameObject;
        if (!BionicDiningNeed.IsBionic(eater))
            return;

        AmountInstance calories = Db.Get().Amounts.Calories.Lookup(eater);
        if (calories != null)
            calories.value = calories.GetMin();
    }
}

[HarmonyPatch(typeof(CalorieMonitor.Instance), nameof(CalorieMonitor.Instance.IsHungry))]
internal static class BionicIsHungryPatch
{
    private static bool Prefix(CalorieMonitor.Instance __instance, ref bool __result)
    {
        if (!BionicDiningNeed.IsBionic(__instance))
            return true;

        __result = !BionicDiningNeed.HasSensoryDiningEffect(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(CalorieMonitor.Instance), nameof(CalorieMonitor.Instance.IsSatisfied))]
internal static class BionicIsSatisfiedPatch
{
    private static bool Prefix(CalorieMonitor.Instance __instance, ref bool __result)
    {
        if (!BionicDiningNeed.IsBionic(__instance))
            return true;

        __result = BionicDiningNeed.HasSensoryDiningEffect(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(CalorieMonitor.Instance), nameof(CalorieMonitor.Instance.IsStarving))]
internal static class BionicIsStarvingPatch
{
    private static bool Prefix(CalorieMonitor.Instance __instance, ref bool __result)
    {
        if (!BionicDiningNeed.IsBionic(__instance))
            return true;

        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(CalorieMonitor.Instance), nameof(CalorieMonitor.Instance.IsDepleted))]
internal static class BionicIsDepletedPatch
{
    private static bool Prefix(CalorieMonitor.Instance __instance, ref bool __result)
    {
        if (!BionicDiningNeed.IsBionic(__instance))
            return true;

        __result = false;
        return false;
    }
}
