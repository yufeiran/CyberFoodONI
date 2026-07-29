using HarmonyLib;
using Klei.AI;

namespace CyberFoodONI.Patches;

[HarmonyPatch(typeof(ModifierSet), nameof(ModifierSet.Initialize))]
internal static class SensoryDiningEffectPatch
{
    private static void Postfix(ModifierSet __instance)
    {
        if (__instance.effects.Exists(ModInfo.SensoryDiningEffectId))
            return;

        string name =
            STRINGS.DUPLICANTS.MODIFIERS.CYBERFOODONI_SENSORYDINING.NAME.text;
        string tooltip =
            STRINGS.DUPLICANTS.MODIFIERS.CYBERFOODONI_SENSORYDINING.TOOLTIP.text;

        var effect = new Effect(
            ModInfo.SensoryDiningEffectId,
            name,
            tooltip,
            ModInfo.EffectDurationSeconds,
            show_in_ui: true,
            trigger_floating_text: true,
            is_bad: false);

        effect.Add(new AttributeModifier(
            Db.Get().Attributes.QualityOfLife.Id,
            ModInfo.MoraleBonus,
            name));

        __instance.effects.Add(effect);
    }
}
