using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace CyberFoodONI.Patches;

internal static class BionicWorker
{
    internal static bool IsBionic(WorkerBase worker)
    {
        return worker != null && worker.gameObject.HasTag(GameTags.Minions.Models.Bionic);
    }
}

internal static class SuppressOrdinaryFoodEffectsPatch
{
    internal static bool Prefix(WorkerBase worker)
    {
        return !BionicWorker.IsBionic(worker) ||
               CyberFoodSettings.EnableOriginalFoodEffects;
    }
}

internal static class CompleteSensoryDiningPatch
{
    internal sealed class ConsumptionState
    {
        internal bool IsBionic;
        internal float CaloriesConsumed;
        internal List<string> SuppressedEffects;
        internal HashSet<Effect> OriginalFoodEffects;
    }

    internal static void Prefix(
        Edible __instance,
        WorkerBase worker,
        out ConsumptionState __state)
    {
        __state = new ConsumptionState
        {
            IsBionic = BionicWorker.IsBionic(worker),
            CaloriesConsumed = __instance.caloriesConsumed
        };

        if (!__state.IsBionic)
            return;

        if (CyberFoodSettings.EnableOriginalFoodEffects)
        {
            __state.OriginalFoodEffects = CollectOriginalFoodEffects(__instance, worker);
        }
        else if (__instance.FoodInfo.Effects.Count > 0)
        {
            __state.SuppressedEffects = new List<string>(__instance.FoodInfo.Effects);
            __instance.FoodInfo.Effects.Clear();
        }
    }

    internal static void Postfix(
        Edible __instance,
        WorkerBase worker,
        ConsumptionState __state)
    {
        if (__state.SuppressedEffects != null)
            __instance.FoodInfo.Effects.AddRange(__state.SuppressedEffects);

        if (!__state.IsBionic)
            return;

        if (__state.OriginalFoodEffects != null)
            SetEffectDurations(effects: worker.GetComponent<Effects>(), __state.OriginalFoodEffects);

        AmountInstance calories = Db.Get().Amounts.Calories.Lookup(worker.gameObject);
        if (calories != null)
            calories.value = calories.GetMin();

        if (__state.CaloriesConsumed < ModInfo.MinimumSuccessfulTastingCalories)
            return;

        Effects effects = worker.GetComponent<Effects>();
        effects?.Add(ModInfo.SensoryDiningEffectId, should_save: true);
    }

    private static HashSet<Effect> CollectOriginalFoodEffects(
        Edible edible,
        WorkerBase worker)
    {
        var result = new HashSet<Effect>();

        foreach (string effectId in edible.FoodInfo.Effects)
            result.Add(Db.Get().effects.Get(effectId));

        int expectation = Mathf.RoundToInt(
            worker.GetAttributes().Add(Db.Get().Attributes.FoodExpectation).GetTotalValue());
        string qualityEffectId = Edible.GetEffectForFoodQuality(
            edible.FoodInfo.Quality + expectation);
        result.Add(Db.Get().effects.Get(qualityEffectId));

        foreach (SpiceInstance spice in edible.Spices)
        {
            if (spice.StatBonus != null)
                result.Add(spice.StatBonus);
        }

        if (edible.gameObject.HasTag(GameTags.Rehydrated))
            result.Add(FoodRehydratorConfig.RehydrationEffect);

        return result;
    }

    private static void SetEffectDurations(
        Effects effects,
        HashSet<Effect> targetEffects)
    {
        if (effects == null)
            return;

        foreach (EffectInstance instance in effects.GetTimeLimitedEffects())
        {
            if (targetEffects.Contains(instance.effect))
                instance.timeRemaining = ModInfo.EffectDurationSeconds;
        }
    }
}

[HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
internal static class InstallEdiblePatchesAfterDbInitialization
{
    private static bool installed;

    private static void Postfix()
    {
        if (installed)
            return;

        var harmony = new Harmony($"{ModInfo.StaticId}.Edible");

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Edible), "AddOnConsumeEffects"),
            prefix: new HarmonyMethod(
                typeof(SuppressOrdinaryFoodEffectsPatch),
                nameof(SuppressOrdinaryFoodEffectsPatch.Prefix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Edible), "StopConsuming"),
            prefix: new HarmonyMethod(
                typeof(CompleteSensoryDiningPatch),
                nameof(CompleteSensoryDiningPatch.Prefix)),
            postfix: new HarmonyMethod(
                typeof(CompleteSensoryDiningPatch),
                nameof(CompleteSensoryDiningPatch.Postfix)));

        installed = true;
        Debug.Log($"[{ModInfo.StaticId}] Late Edible patches installed");
    }
}
