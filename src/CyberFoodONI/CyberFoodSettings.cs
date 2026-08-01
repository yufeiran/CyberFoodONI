using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace CyberFoodONI;

[JsonObject(MemberSerialization.OptIn)]
internal sealed class CyberFoodSettingsData
{
    [JsonProperty]
    public bool EnableOriginalFoodEffects { get; set; }

    [JsonProperty]
    public bool EnableDiningRoomEffects { get; set; }
}

internal static class CyberFoodSettings
{
    private const string ConfigFileName = "CyberFoodONI.json";

    private static CyberFoodSettingsData current = new CyberFoodSettingsData();
    private static string configPath;

    internal static bool EnableOriginalFoodEffects => current.EnableOriginalFoodEffects;

    internal static bool EnableDiningRoomEffects => current.EnableDiningRoomEffects;

    internal static void Initialize(KMod.Mod mod)
    {
        configPath = Path.Combine(KMod.Manager.GetDirectory(), "config", ConfigFileName);
        Load();
        InstallManagementButton(mod);
    }

    private static void Load()
    {
        try
        {
            if (File.Exists(configPath))
            {
                current = JsonConvert.DeserializeObject<CyberFoodSettingsData>(
                              File.ReadAllText(configPath))
                          ?? new CyberFoodSettingsData();
            }
            else
            {
                current = new CyberFoodSettingsData();
                Save();
            }

            Debug.Log(
                $"[{ModInfo.StaticId}] Settings loaded: " +
                $"food effects={EnableOriginalFoodEffects}, " +
                $"dining room effects={EnableDiningRoomEffects}");
        }
        catch (Exception exception)
        {
            current = new CyberFoodSettingsData();
            Debug.LogWarning(
                $"[{ModInfo.StaticId}] Could not load settings from '{configPath}'. " +
                $"Using defaults. {exception}");
        }
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));
            File.WriteAllText(
                configPath,
                JsonConvert.SerializeObject(current, Formatting.Indented));
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[{ModInfo.StaticId}] Could not save settings to '{configPath}'. {exception}");
        }
    }

    internal static void InstallManagementButton(KMod.Mod mod)
    {
        if (mod == null ||
            !string.Equals(mod.staticID, ModInfo.StaticId, StringComparison.Ordinal))
            return;

        try
        {
            SetPrivateProperty(
                mod,
                nameof(KMod.Mod.manage_tooltip),
                (LocString)STRINGS.UI.SETTINGS.MANAGE_TOOLTIP.text);
            SetPrivateProperty(
                mod,
                nameof(KMod.Mod.on_managed),
                new System.Action(ShowDialog));
            Debug.Log($"[{ModInfo.StaticId}] Mods-screen settings button installed");
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"[{ModInfo.StaticId}] Could not install the Mods-screen settings button. {exception}");
        }
    }

    private static void SetPrivateProperty(KMod.Mod mod, string name, object value)
    {
        PropertyInfo property = typeof(KMod.Mod).GetProperty(
            name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MethodInfo setter = property?.GetSetMethod(nonPublic: true);
        if (setter == null)
            throw new MissingMemberException(typeof(KMod.Mod).FullName, name);

        setter.Invoke(mod, new[] { value });
    }

    private static void ShowDialog()
    {
        string enabled = STRINGS.UI.SETTINGS.ENABLED.text;
        string disabled = STRINGS.UI.SETTINGS.DISABLED.text;
        string foodState = EnableOriginalFoodEffects ? enabled : disabled;
        string roomState = EnableDiningRoomEffects ? enabled : disabled;

        var dialog = (ConfirmDialogScreen)KScreenManager.Instance.StartScreen(
            ScreenPrefabs.Instance.ConfirmDialogScreen.gameObject,
            Global.Instance.globalCanvas);
        dialog.PopupConfirmDialog(
            text: string.Format(
                STRINGS.UI.SETTINGS.DESCRIPTION.text,
                foodState,
                roomState),
            on_confirm: ToggleOriginalFoodEffects,
            on_cancel: () => { },
            configurable_text: string.Format(
                STRINGS.UI.SETTINGS.DINING_ROOM_BUTTON.text,
                roomState),
            on_configurable_clicked: ToggleDiningRoomEffects,
            title_text: STRINGS.UI.SETTINGS.TITLE.text,
            confirm_text: string.Format(
                STRINGS.UI.SETTINGS.FOOD_EFFECTS_BUTTON.text,
                foodState),
            cancel_text: STRINGS.UI.SETTINGS.DONE.text);

        EnlargeDialog(dialog);
    }

    private static void EnlargeDialog(ConfirmDialogScreen dialog)
    {
        LocText title = AccessTools.Field(typeof(ConfirmDialogScreen), "titleText")
            ?.GetValue(dialog) as LocText;
        LocText message = AccessTools.Field(typeof(ConfirmDialogScreen), "popupMessage")
            ?.GetValue(dialog) as LocText;

        SetFontSize(title, 24f);
        SetFontSize(message, 22f);

        RectTransform panel = FindDialogPanel(dialog, title, message);
        if (panel != null)
        {
            ContentSizeFitter fitter = panel.GetComponent<ContentSizeFitter>();
            if (fitter != null)
                fitter.enabled = false;

            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 740f);
            panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 680f);

            LayoutElement panelLayout = panel.GetComponent<LayoutElement>() ??
                                        panel.gameObject.AddComponent<LayoutElement>();
            panelLayout.minWidth = 740f;
            panelLayout.preferredWidth = 740f;
            panelLayout.minHeight = 680f;
            panelLayout.preferredHeight = 680f;

            Debug.Log(
                $"[{ModInfo.StaticId}] Enlarged settings panel '{panel.name}' to 740x680");
        }

        if (message != null)
        {
            message.lineSpacing = 5f;
            LayoutElement layout = message.GetComponent<LayoutElement>() ??
                                   message.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = 630f;
            layout.preferredWidth = 630f;
            layout.minHeight = 370f;
            layout.preferredHeight = 370f;
        }

        EnlargeButton(dialog, "confirmButton");
        EnlargeButton(dialog, "cancelButton");
        EnlargeButton(dialog, "configurableButton");
    }

    private static RectTransform FindDialogPanel(
        ConfirmDialogScreen dialog,
        LocText title,
        LocText message)
    {
        if (title == null || message == null)
            return null;

        Transform current = message.transform.parent;
        while (current != null && current != dialog.transform)
        {
            if (title.transform.IsChildOf(current))
                return current as RectTransform;

            current = current.parent;
        }

        return null;
    }

    private static void EnlargeButton(ConfirmDialogScreen dialog, string fieldName)
    {
        GameObject button = AccessTools.Field(typeof(ConfirmDialogScreen), fieldName)
            ?.GetValue(dialog) as GameObject;
        if (button == null)
            return;

        SetFontSize(button.GetComponentInChildren<LocText>(includeInactive: true), 21f);
        LayoutElement layout = button.GetComponent<LayoutElement>() ??
                               button.AddComponent<LayoutElement>();
        layout.minHeight = 52f;
        layout.preferredHeight = 52f;
    }

    private static void SetFontSize(LocText text, float size)
    {
        if (text == null)
            return;

        text.enableAutoSizing = false;
        text.fontSize = size;
    }

    private static void ToggleOriginalFoodEffects()
    {
        current.EnableOriginalFoodEffects = !current.EnableOriginalFoodEffects;
        Save();
        ShowDialog();
    }

    private static void ToggleDiningRoomEffects()
    {
        current.EnableDiningRoomEffects = !current.EnableDiningRoomEffects;
        Save();
        ShowDialog();
    }
}
