using System.IO;
using HarmonyLib;
using KMod;

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
