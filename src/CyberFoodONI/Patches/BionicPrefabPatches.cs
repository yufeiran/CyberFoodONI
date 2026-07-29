using System;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace CyberFoodONI.Patches;

internal static class ArrayHelper
{
    internal static T[] Append<T>(T[] source, params T[] additions)
    {
        var result = new T[source.Length + additions.Length];
        Array.Copy(source, result, source.Length);
        Array.Copy(additions, 0, result, source.Length, additions.Length);
        return result;
    }
}

[HarmonyPatch(typeof(BionicMinionConfig), MethodType.Constructor)]
internal static class BionicMinionConfigConstructorPatch
{
    private static void Postfix(BionicMinionConfig __instance)
    {
        __instance.RATIONAL_AI_STATE_MACHINES = ArrayHelper.Append(
            __instance.RATIONAL_AI_STATE_MACHINES,
            new Func<RationalAi.Instance, StateMachine.Instance>(
                smi => new RationMonitor.Instance(smi.master)),
            new Func<RationalAi.Instance, StateMachine.Instance>(
                smi => new CalorieMonitor.Instance(smi.master)));
    }
}

[HarmonyPatch(typeof(BionicMinionConfig), nameof(BionicMinionConfig.GetAmounts))]
internal static class BionicMinionConfigGetAmountsPatch
{
    private static void Postfix(ref string[] __result)
    {
        string caloriesId = Db.Get().Amounts.Calories.Id;
        if (Array.IndexOf(__result, caloriesId) < 0)
            __result = ArrayHelper.Append(__result, caloriesId);
    }
}

[HarmonyPatch(typeof(BionicMinionConfig), nameof(BionicMinionConfig.GetTraits))]
internal static class BionicMinionConfigGetTraitsPatch
{
    private static void Postfix(ref AttributeModifier[] __result)
    {
        __result = ArrayHelper.Append(
            __result,
            new AttributeModifier(
                Db.Get().Amounts.Calories.maxAttribute.Id,
                ModInfo.TastingCalories,
                STRINGS.DUPLICANTS.MODIFIERS.CYBERFOODONI_SENSORYDINING.NAME.text));
    }
}

[HarmonyPatch(typeof(BionicMinionConfig), nameof(BionicMinionConfig.CreatePrefab))]
internal static class BionicMinionConfigCreatePrefabPatch
{
    private static void Postfix(GameObject __result)
    {
        __result.AddOrGet<SensoryDiningController>();
    }
}
