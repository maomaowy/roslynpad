using System.Collections;
using System.Globalization;
using System.Resources;
using Avalonia;
using Avalonia.Controls;
using RoslynPad.UI;

namespace RoslynPad.Localization;

/// <summary>
/// Application-wide localization: resolves strings from the embedded resx resources and keeps
/// a <c>L_*</c> resource dictionary in the application resources for XAML
/// (<c>{DynamicResource L_Key}</c>). Switching the language rebuilds that dictionary, so open
/// windows and dialogs update in place without a restart.
/// </summary>
public static class LocalizationManager
{
    private const string ResourceBaseName = "RoslynPad.Resources.Strings";
    private const string ResourceKeyPrefix = "L_";

    private static readonly ResourceManager s_resourceManager = new(ResourceBaseName, typeof(LocalizationManager).Assembly);
    private static readonly CultureInfo s_systemCulture = CultureInfo.CurrentUICulture;
    private static readonly Dictionary<NativeMenuItem, string> s_nativeMenuKeys = [];

    private static string[]? s_keys;
    private static ResourceDictionary? s_dictionary;

    public static AppLanguage Language { get; private set; } = AppLanguage.System;

    public static CultureInfo CurrentCulture { get; private set; } = s_systemCulture;

    /// <summary>Raised on the UI thread after the language (and the resource dictionary) changed.</summary>
    public static event Action? LanguageChanged;

    public static void SetLanguage(AppLanguage language)
    {
        if (language == Language && s_dictionary is not null)
        {
            return;
        }

        Language = language;
        CurrentCulture = ResolveCulture(language);
        CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;

        InstallResources();
        LanguageChanged?.Invoke();
    }

    /// <summary>Returns the localized string, or <paramref name="key"/> when it is missing.</summary>
    public static string Get(string key) => s_resourceManager.GetString(key, CurrentCulture) ?? key;

    public static string Get(string key, string fallback) => s_resourceManager.GetString(key, CurrentCulture) ?? fallback;

    public static string Format(string key, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), args);

    /// <summary>
    /// Fills the headers of a native menu from resource keys. Menu items declare their key as the
    /// initial <see cref="NativeMenuItem.Header"/> (e.g. <c>L_Menu_File</c>); the key is remembered
    /// so later language changes can re-apply it.
    /// </summary>
    public static void Localize(NativeMenu? menu)
    {
        if (menu is null)
        {
            return;
        }

        foreach (var item in menu.Items)
        {
            LocalizeItem(item);
        }
    }

    private static void LocalizeItem(NativeMenuItemBase item)
    {
        if (item is not NativeMenuItem menuItem)
        {
            return;
        }

        if (menuItem.Header is { } header && header.StartsWith(ResourceKeyPrefix, StringComparison.Ordinal))
        {
            s_nativeMenuKeys[menuItem] = header;
        }

        if (s_nativeMenuKeys.TryGetValue(menuItem, out var key))
        {
            menuItem.Header = Get(key);
        }

        Localize(menuItem.Menu);
    }

    private static CultureInfo ResolveCulture(AppLanguage language) => language switch
    {
        AppLanguage.English => CultureInfo.GetCultureInfo("en"),
        AppLanguage.ChineseSimplified => CultureInfo.GetCultureInfo("zh-CN"),
        _ => s_systemCulture.TwoLetterISOLanguageName == "zh"
            ? CultureInfo.GetCultureInfo("zh-CN")
            : s_systemCulture,
    };

    private static void InstallResources()
    {
        if (Application.Current is not { } application)
        {
            return;
        }

        var dictionary = new ResourceDictionary();
        foreach (var key in GetKeys())
        {
            dictionary[ResourceKeyPrefix + key] = Get(key);
        }

        if (s_dictionary is not null)
        {
            application.Resources.MergedDictionaries.Remove(s_dictionary);
        }

        s_dictionary = dictionary;
        application.Resources.MergedDictionaries.Add(dictionary);
    }

    private static string[] GetKeys()
    {
        if (s_keys is not null)
        {
            return s_keys;
        }

        var keys = new List<string>();

        // The ResourceManager caches and owns this set; disposing it here would close the
        // cached instance that later GetString calls reuse.
        var resourceSet = s_resourceManager.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: true);
        if (resourceSet is not null)
        {
            foreach (DictionaryEntry entry in resourceSet)
            {
                if (entry.Key is string key)
                {
                    keys.Add(key);
                }
            }
        }

        s_keys = [.. keys];
        return s_keys;
    }
}