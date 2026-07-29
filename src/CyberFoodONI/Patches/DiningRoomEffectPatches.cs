using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace CyberFoodONI.Patches;

[HarmonyPatch(typeof(EatChore.StatesInstance), nameof(EatChore.StatesInstance.OnEnterMessStation))]
internal static class DiningRoomEffectPatches
{
    private sealed class EffectSnapshot
    {
        internal Effects Effects;
        internal Dictionary<Effect, float> RemainingTime;
    }

    private static void Prefix(GameObject diner, out EffectSnapshot __state)
    {
        __state = null;
        if (diner == null || !diner.HasTag(GameTags.Minions.Models.Bionic))
            return;

        Effects effects = diner.GetComponent<Effects>();
        if (effects == null)
            return;

        var remainingTime = new Dictionary<Effect, float>();
        foreach (EffectInstance instance in effects.GetTimeLimitedEffects())
            remainingTime[instance.effect] = instance.timeRemaining;

        __state = new EffectSnapshot
        {
            Effects = effects,
            RemainingTime = remainingTime
        };
    }

    private static void Postfix(EffectSnapshot __state)
    {
        if (__state == null)
            return;

        var current = new List<EffectInstance>(__state.Effects.GetTimeLimitedEffects());
        foreach (EffectInstance instance in current)
        {
            if (__state.RemainingTime.TryGetValue(instance.effect, out float previousTime))
            {
                instance.timeRemaining = previousTime;
            }
            else
            {
                __state.Effects.Remove(instance.effect);
            }
        }
    }
}
