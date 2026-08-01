using System.IO;
using System.Reflection;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace CyberFoodONI;

public sealed class Mod : UserMod2
{
    private static string translationPath;
    private static bool translationLoaded;

    public override void OnLoad(Harmony harmony)
    {
        Localization.RegisterForTranslation(typeof(STRINGS));
        translationPath = Path.Combine(path, "translations", "zh.po");
        base.OnLoad(harmony);
        LoadChineseTranslationWhenNeeded();
        CyberFoodSettings.Initialize(mod);

        Debug.Log($"[{ModInfo.StaticId}] Loaded");
    }

    internal static void LoadChineseTranslationWhenNeeded()
    {
        if (translationLoaded)
            return;

        Localization.Locale locale = Localization.GetLocale();
        if (locale == null || locale.Lang != Localization.Language.Chinese)
            return;

        if (string.IsNullOrEmpty(translationPath) || !File.Exists(translationPath))
        {
            Debug.LogWarning($"[{ModInfo.StaticId}] Missing translation file: {translationPath}");
            return;
        }

        Localization.OverloadStrings(
            Localization.LoadStringsFile(translationPath, isTemplate: false));
        translationLoaded = true;
        Debug.Log($"[{ModInfo.StaticId}] Chinese translation loaded");
    }
}

[HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
internal static class LocalizationInitializePatch
{
    private static void Postfix()
    {
        Mod.LoadChineseTranslationWhenNeeded();
    }
}

[HarmonyPatch(typeof(KMod.Manager), nameof(KMod.Manager.Subscribe))]
internal static class ModSubscriptionPatch
{
    private static void Postfix(KMod.Mod mod)
    {
        CyberFoodSettings.InstallManagementButton(mod);
    }
}

[HarmonyPatch]
internal static class ModsScreenButtonLabelPatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(ModsScreen), "BuildDisplay");
    }

    private static void Postfix(ModsScreen __instance)
    {
        Transform entryParent = AccessTools.Field(typeof(ModsScreen), "entryParent")
            ?.GetValue(__instance) as Transform;
        if (entryParent == null)
            return;

        foreach (Transform child in entryParent)
        {
            if (child == null || child.name != "Cyber Food")
                continue;

            HierarchyReferences references = child.GetComponent<HierarchyReferences>();
            KButton button = references?.GetReference<KButton>("ManageButton");
            LocText label = button?.GetComponentInChildren<LocText>();
            if (label != null)
                label.text = STRINGS.UI.SETTINGS.OPTIONS_BUTTON.text;

            break;
        }
    }
}
