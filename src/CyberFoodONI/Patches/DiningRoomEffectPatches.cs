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
        internal HashSet<Effect> RoomEffects;
        internal bool EnableRoomEffects;
    }

    private static void Prefix(
        GameObject messStation,
        GameObject diner,
        out EffectSnapshot __state)
    {
        __state = null;
        if (diner == null ||
            !diner.HasTag(GameTags.Minions.Models.Bionic))
            return;

        Effects effects = diner.GetComponent<Effects>();
        if (effects == null)
            return;

        Room room = Game.Instance?.roomProber?.GetRoomOfGameObject(messStation);
        string[] roomEffectIds = room?.roomType?.effects;
        if (roomEffectIds == null || roomEffectIds.Length == 0)
            return;

        var roomEffects = new HashSet<Effect>();
        foreach (string effectId in roomEffectIds)
            roomEffects.Add(Db.Get().effects.Get(effectId));

        var remainingTime = new Dictionary<Effect, float>();
        foreach (EffectInstance instance in effects.GetTimeLimitedEffects())
            remainingTime[instance.effect] = instance.timeRemaining;

        __state = new EffectSnapshot
        {
            Effects = effects,
            RemainingTime = remainingTime,
            RoomEffects = roomEffects,
            EnableRoomEffects = CyberFoodSettings.EnableDiningRoomEffects
        };
    }

    private static void Postfix(EffectSnapshot __state)
    {
        if (__state == null)
            return;

        var current = new List<EffectInstance>(__state.Effects.GetTimeLimitedEffects());
        foreach (EffectInstance instance in current)
        {
            if (!__state.RoomEffects.Contains(instance.effect))
                continue;

            if (__state.EnableRoomEffects)
            {
                instance.timeRemaining = ModInfo.EffectDurationSeconds;
            }
            else if (__state.RemainingTime.TryGetValue(instance.effect, out float previousTime))
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

[HarmonyPatch(typeof(Garnish), nameof(Garnish.Activate))]
internal static class BionicGarnishEffectPatch
{
    private static bool Prefix(GameObject diner, ref EffectInstance __result)
    {
        if (diner == null ||
            !diner.HasTag(GameTags.Minions.Models.Bionic) ||
            CyberFoodSettings.EnableOriginalFoodEffects)
        {
            return true;
        }

        __result = null;
        return false;
    }

    private static void Postfix(GameObject diner, EffectInstance __result)
    {
        if (__result != null &&
            diner != null &&
            diner.HasTag(GameTags.Minions.Models.Bionic) &&
            CyberFoodSettings.EnableOriginalFoodEffects)
        {
            __result.timeRemaining = ModInfo.EffectDurationSeconds;
        }
    }
}
