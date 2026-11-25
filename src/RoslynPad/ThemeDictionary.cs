using Avalonia.Media;
using RoslynPad.Themes;

namespace RoslynPad;

public class ThemeDictionary : ThemeDictionaryBase
{
    private static readonly IReadOnlySet<string> s_colors = typeof(ThemeDictionary).GetFields()
        .Where(t => t.IsStatic && t.Attributes.HasFlag(System.Reflection.FieldAttributes.Literal))
        .Select(t => (string)t.GetValue(null)!).ToHashSet();

    public ThemeDictionary(Theme theme) : base(theme)
    {
        Initialize(theme);
    }

    public const string TabBarBackground = "editorGroupHeader.tabsBackground";
    public const string TabBarBorder = "editorGroupHeader.tabsBorder";
    public const string ScrollBarSliderBackground = "scrollbarSlider.background";
    public const string ScrollBarSliderActiveBackground = "scrollbarSlider.activeBackground";
    public const string ScrollBarSliderHoverBackground = "scrollbarSlider.hoverBackground";
    public const string ScrollBarShadow = "scrollbar.shadow";
    public const string EditorBackground = "editor.background";
    public const string EditorForeground = "editor.foreground";
    public const string EditorSelectionBackground = "editor.selectionBackground";
    public const string Foreground = "foreground";
    public const string PanelBackground = "panel.background";
    public const string StatusBarBackground = "statusBar.background";
    public const string StatusBarForeground = "statusBar.foreground";
    public const string StatusBarItemErrorBackground = "statusBarItem.errorBackground";
    public const string StatusBarItemErrorForeground = "statusBarItem.errorForeground";
    public const string FocusBorder = "focusBorder";
    public const string ListActiveSelectionBackground = "list.activeSelectionBackground";
    public const string ListActiveSelectionForeground = "list.activeSelectionForeground";
    public const string ListInactiveSelectionBackground = "list.inactiveSelectionBackground";
    public const string TabActiveBackground = "tab.activeBackground";
    public const string TabUnfocusedActiveBackground = "tab.unfocusedActiveBackground";
    public const string TabInactiveBackground = "tab.inactiveBackground";
    public const string TabActiveForeground = "tab.activeForeground";
    public const string TabInactiveForeground = "tab.inactiveForeground";
    public const string TabHoverBackground = "tab.hoverBackground";
    public const string TabHoverForeground = "tab.hoverForeground";
    public const string TabActiveBorder = "tab.activeBorder";
    public const string TabActiveBorderTop = "tab.activeBorderTop";
    public const string TabHoverBorder = "tab.hoverBorder";
    public const string TabBorder = "tab.border";
    public const string InputBorder = "input.border";
    public const string TitleBarActiveBackground = "titleBar.activeBackground";
    public const string FoldingControlForeground = "editorGutter.foldingControlForeground";
    public const string PanelBorder = "panel.border";
    public const string ToolbarHoverBackground = "toolbar.hoverBackground";
    public const string ToolbarActiveBackground = "toolbar.activeBackground";
    public const string InputOptionActiveBackground = "inputOption.activeBackground";
    public const string InputOptionActiveBorder = "inputOption.activeBorder";
    public const string SideBarBackground = "sideBar.background";
    public const string ListHoverBackground = "list.hoverBackground";
    public const string DescriptionForeground = "descriptionForeground";
    public const string StatusBarItemHoverBackground = "statusBarItem.hoverBackground";
    public const string EditorGroupBorder = "editorGroup.border";
    public const string EditorGroupDropBackground = "editorGroup.dropBackground";
    public const string SashHoverBorder = "sash.hoverBorder";
    public const string IconForeground = "icon.foreground";
    public const string PanelTitleActiveForeground = "panelTitle.activeForeground";
    public const string PanelTitleInactiveForeground = "panelTitle.inactiveForeground";
    public const string PanelTitleActiveBorder = "panelTitle.activeBorder";
    public const string ErrorForeground = "errorForeground";
    public const string DisabledForeground = "disabledForeground";
    public const string InputBackground = "input.background";
    public const string InputForeground = "input.foreground";
    public const string InputPlaceholderForeground = "input.placeholderForeground";
    public const string ButtonBackground = "button.background";
    public const string ButtonForeground = "button.foreground";
    public const string ButtonHoverBackground = "button.hoverBackground";
    public const string ButtonBorder = "button.border";
    public const string ButtonSecondaryBackground = "button.secondaryBackground";
    public const string ButtonSecondaryForeground = "button.secondaryForeground";
    public const string ButtonSecondaryHoverBackground = "button.secondaryHoverBackground";
    public const string ButtonSecondaryBorder = "button.secondaryBorder";
    public const string MenuBackground = "menu.background";
    public const string MenuForeground = "menu.foreground";
    public const string MenuBorder = "menu.border";
    public const string MenuSelectionBackground = "menu.selectionBackground";
    public const string MenuSeparatorBackground = "menu.separatorBackground";
    public const string WidgetBorder = "widget.border";
    public const string WidgetShadow = "widget.shadow";
    public const string EditorWidgetBackground = "editorWidget.background";
    public const string ProgressBarBackground = "progressBar.background";
    public const string ListHoverForeground = "list.hoverForeground";
    public const string ListInactiveSelectionForeground = "list.inactiveSelectionForeground";
    public const string ListFocusOutline = "list.focusOutline";
    public const string ListWarningForeground = "list.warningForeground";
    public const string BadgeBackground = "badge.background";
    public const string BadgeForeground = "badge.foreground";
    public const string TitleBarBorder = "titleBar.border";

    private void Initialize(Theme theme)
    {
        if (theme.Colors is null)
        {
            return;
        }

        foreach (var id in s_colors)
        {
            if (theme.TryGetColor(id) is { } color)
            {
                SetThemeColor(id, color);
            }
        }

        MapDockResources(theme.Type);
        MapScrollBarDefaults(theme.Type);
        MapSemanticDefaults(theme.Type);
        MapFluentAccent(theme.Type);
    }

    /// <summary>
    /// VS Code's coded scrollbar defaults (colorRegistry.ts) for themes that don't set
    /// <c>scrollbarSlider.*</c> — the bundled 2026 themes do, the classic ones don't.
    /// </summary>
    private void MapScrollBarDefaults(ThemeType type)
    {
        SetDefaultThemeColor(ScrollBarSliderBackground, type == ThemeType.Dark ? "#79797966" : "#64646466");
        SetDefaultThemeColor(ScrollBarSliderHoverBackground, "#646464B3");
        SetDefaultThemeColor(ScrollBarSliderActiveBackground, type == ThemeType.Dark ? "#BFBFBF66" : "#00000099");
    }

    private void SetDefaultThemeColor(string name, string color)
    {
        if (!TryGetValue(name, out _))
        {
            SetThemeColor(name, color);
        }
    }

    /// <summary>Copies an already mapped theme color into a missing semantic key.</summary>
    private void SetDefaultThemeColorFrom(string name, params string[] sourceIds)
    {
        if (TryGetValue(name, out _))
        {
            return;
        }

        foreach (var id in sourceIds)
        {
            if (TryGetValue(id, out var value) && value is SolidColorBrush brush)
            {
                this[name] = brush;
                this[name + "Color"] = brush.Color;
                return;
            }
        }
    }

    /// <summary>
    /// Semantic colors for the app's controls. The bundled 2026 themes provide most of these;
    /// the classic VS Code themes predate them, so coded defaults keep both light and dark
    /// readable when a custom theme omits a key.
    /// </summary>
    private void MapSemanticDefaults(ThemeType type)
    {
        var isLight = type == ThemeType.Light;

        SetDefaultThemeColor(ErrorForeground, isLight ? "#AD0707" : "#F48771");
        SetDefaultThemeColor(DisabledForeground, isLight ? "#BBBBBB" : "#555555");
        SetDefaultThemeColor(DescriptionForeground, isLight ? "#606060" : "#8C8C8C");
        SetDefaultThemeColor(InputPlaceholderForeground, isLight ? "#999999" : "#555555");
        SetDefaultThemeColor(ButtonBackground, isLight ? "#0069CC" : "#297AA0");
        SetDefaultThemeColor(ButtonForeground, "#FFFFFF");
        SetDefaultThemeColor(ButtonHoverBackground, isLight ? "#0063C1" : "#2B7DA3");
        SetDefaultThemeColor(ButtonBorder, isLight ? "#0069CC" : "#297AA0");
        SetDefaultThemeColor(ButtonSecondaryBackground, isLight ? "#EAEAEA" : "#333536");
        SetDefaultThemeColor(ButtonSecondaryForeground, isLight ? "#202020" : "#BFBFBF");
        SetDefaultThemeColor(ButtonSecondaryHoverBackground, isLight ? "#F2F3F4" : "#3A3B3C");
        SetDefaultThemeColor(ButtonSecondaryBorder, isLight ? "#EAEAEA" : "#333536");
        SetDefaultThemeColor(MenuBackground, isLight ? "#FAFAFD" : "#202122");
        SetDefaultThemeColor(MenuForeground, isLight ? "#202020" : "#BFBFBF");
        SetDefaultThemeColor(MenuBorder, isLight ? "#E4E5E6" : "#2A2B2C");
        SetDefaultThemeColor(MenuSeparatorBackground, isLight ? "#EEEEF1" : "#2A2B2C");
        SetDefaultThemeColor(WidgetBorder, isLight ? "#E2E2E5" : "#2A2B2C");
        SetDefaultThemeColor(WidgetShadow, isLight ? "#33000000" : "#99000000");
        SetDefaultThemeColor(ProgressBarBackground, "#878889");
        SetDefaultThemeColor(StatusBarItemErrorBackground, "#C72E0F");
        SetDefaultThemeColor(StatusBarItemErrorForeground, "#FFFFFF");
        SetDefaultThemeColor(ListWarningForeground, isLight ? "#667309" : "#E5BA7D");
        SetDefaultThemeColor(BadgeForeground, "#FFFFFF");

        SetDefaultThemeColorFrom(InputBackground, EditorBackground, SideBarBackground);
        SetDefaultThemeColorFrom(InputForeground, Foreground);
        SetDefaultThemeColorFrom(InputBorder, InputBorder, WidgetBorder, EditorGroupBorder, PanelBorder);
        SetDefaultThemeColorFrom(MenuSelectionBackground, ListInactiveSelectionBackground, ListActiveSelectionBackground);
        SetDefaultThemeColor(MenuSelectionBackground, isLight ? "#00000014" : "#FFFFFF14");
        SetDefaultThemeColorFrom(EditorWidgetBackground, EditorWidgetBackground, EditorBackground, SideBarBackground);
        SetDefaultThemeColorFrom(ListHoverForeground, ListHoverForeground, Foreground);
        SetDefaultThemeColorFrom(ListInactiveSelectionForeground, ListInactiveSelectionForeground, Foreground);
        SetDefaultThemeColorFrom(ListFocusOutline, ListFocusOutline, FocusBorder);
        SetDefaultThemeColorFrom(PanelTitleActiveBorder, PanelTitleActiveBorder, TabActiveBorderTop, FocusBorder);
        SetDefaultThemeColorFrom(BadgeBackground, BadgeBackground, ButtonBackground);
        SetDefaultThemeColorFrom(TitleBarBorder, TitleBarBorder, PanelBorder);
    }

    /// <summary>
    /// Maps the Fluent theme's accent resources to the VS Code theme accent so controls that
    /// keep their Fluent templates (menus, check boxes, sliders, progress bars, popups) follow
    /// the active theme instead of the fixed Fluent blue.
    /// </summary>
    private void MapFluentAccent(ThemeType type)
    {
        var accent = GetThemeColor(FocusBorder,
            GetThemeColor(ButtonBackground, type == ThemeType.Light ? Color.Parse("#0069CC") : Color.Parse("#3994BC")));

        // The focus border is translucent in several themes; the Fluent accent itself must stay opaque.
        accent = Color.FromRgb(accent.R, accent.G, accent.B);

        this["SystemAccentColor"] = accent;
        this["SystemAccentColorLight1"] = Blend(accent, Colors.White, 0.1);
        this["SystemAccentColorLight2"] = Blend(accent, Colors.White, 0.2);
        this["SystemAccentColorLight3"] = Blend(accent, Colors.White, 0.3);
        this["SystemAccentColorDark1"] = Blend(accent, Colors.Black, 0.1);
        this["SystemAccentColorDark2"] = Blend(accent, Colors.Black, 0.2);
        this["SystemAccentColorDark3"] = Blend(accent, Colors.Black, 0.3);

        var accentBrush = new SolidColorBrush(accent);
        this["SystemControlHighlightAccentBrush"] = accentBrush;
        this["SystemControlForegroundAccentBrush"] = accentBrush;
        this["SystemControlBackgroundAccentBrush"] = accentBrush;
        this["SystemControlHighlightAltAccentBrush"] = accentBrush;
        this["SystemControlHighlightListAccentLowBrush"] = new SolidColorBrush(accent, 0.3);
        this["SystemControlHighlightListAccentMediumBrush"] = new SolidColorBrush(accent, 0.6);
        this["SystemControlHighlightListAccentHighBrush"] = new SolidColorBrush(accent, 0.8);
    }

    private Color GetThemeColor(string name, Color fallback) =>
        TryGetValue(name + "Color", out var value) && value is Color color ? color : fallback;

    private static Color Blend(Color color, Color target, double amount) => Color.FromArgb(
        color.A,
        (byte)(color.R + (target.R - color.R) * amount),
        (byte)(color.G + (target.G - color.G) * amount),
        (byte)(color.B + (target.B - color.B) * amount));

    /// <summary>
    /// Maps VS Code theme colors onto the semantic brush keys consumed by Dock's control themes,
    /// overriding the DockFluentTheme accent defaults.
    /// </summary>
    private void MapDockResources(ThemeType type)
    {
        // VS Code sashes are invisible until interacted with; tab strips are part of
        // the framed cards, so they stay transparent too
        this["DockSplitterIdleBrush"] = Brushes.Transparent;
        this["DockTabBackgroundBrush"] = Brushes.Transparent;
        this["DockDocumentTabStripBackgroundBrush"] = Brushes.Transparent;

        MapDockBrush("DockSurfaceWorkbenchBrush", TabBarBackground, SideBarBackground);
        MapDockBrush("DockSurfaceSidebarBrush", SideBarBackground);
        MapDockBrush("DockSurfaceEditorBrush", EditorBackground);
        MapDockBrush("DockSurfacePanelBrush", PanelBackground, EditorBackground);
        MapDockBrush("DockSurfaceHeaderBrush", TabUnfocusedActiveBackground, TabActiveBackground);
        MapDockBrush("DockSurfaceHeaderActiveBrush", ListInactiveSelectionBackground);

        MapDockBrush("DockBorderSubtleBrush", EditorGroupBorder, PanelBorder);
        MapDockBrush("DockBorderStrongBrush", EditorGroupBorder, PanelBorder);
        MapDockBrush("DockSeparatorBrush", TabBarBorder, EditorGroupBorder);
        MapDockBrush("DockDocumentContentBorderBrush", EditorGroupBorder, PanelBorder);

        MapDockBrush("DockSplitterHoverBrush", SashHoverBorder, FocusBorder);
        MapDockBrush("DockSplitterDragBrush", SashHoverBorder, FocusBorder);
        MapDockBrush("DockApplicationAccentBrushIndicator", SashHoverBorder, FocusBorder);

        MapDockBrush("DockTabHoverBackgroundBrush", ToolbarHoverBackground);
        MapDockBrush("DockTabActiveBackgroundBrush", TabActiveBackground);
        MapDockBrush("DockTabActiveIndicatorBrush", TabActiveBorderTop, FocusBorder);
        MapDockBrush("DockTabForegroundBrush", TabInactiveForeground);
        MapDockBrush("DockTabSelectedForegroundBrush", TabActiveForeground);
        MapDockBrush("DockTabActiveForegroundBrush", TabActiveForeground);
        MapDockBrush("DockDocumentTabSelectedForegroundBrush", TabActiveForeground);
        MapDockBrush("DockDocumentTabPointerOverForegroundBrush", TabHoverForeground, TabActiveForeground);
        MapDockBrush("DockDocumentTabCloseSelectedForegroundBrush", TabActiveForeground);
        MapDockBrush("DockDocumentTabClosePointerOverForegroundBrush", TabActiveForeground);
        MapDockBrush("DockTabCloseHoverBackgroundBrush", ToolbarHoverBackground);

        MapDockBrush("DockTargetIndicatorBrush", EditorGroupDropBackground, ListActiveSelectionBackground);

        MapDockBrush("DockChromeButtonForegroundBrush", IconForeground, Foreground);
        MapDockBrush("DockChromeButtonHoverBackgroundBrush", ToolbarHoverBackground);
        MapDockBrush("DockChromeButtonPressedBackgroundBrush", ToolbarActiveBackground, ToolbarHoverBackground);
        MapDockBrush("DockChromeButtonDangerHoverBrush", ToolbarHoverBackground);

        if (TryGetValue(Foreground + "Color", out var foregroundValue) && foregroundValue is Color foreground)
        {
            var isLight = type == ThemeType.Light;
            this["DockTabPillBrush"] = new SolidColorBrush(foreground, isLight ? 0.04 : 0.05);
            this["DockTabPillHoverBrush"] = new SolidColorBrush(foreground, isLight ? 0.06 : 0.08);
            this["DockTabPillSelectedBrush"] = new SolidColorBrush(foreground, isLight ? 0.10 : 0.18);
        }

        // Inactive tab text must stay readable; VS Code tab.inactiveForeground is the semantic
        // source, with disabled-foreground or 70% foreground fallbacks for classic themes.
        MapDockBrush("DockTabPillForegroundBrush", TabInactiveForeground, DisabledForeground);
        if (!TryGetValue("DockTabPillForegroundBrush", out _) &&
            TryGetValue(Foreground + "Color", out var pillForeground) && pillForeground is Color pillColor)
        {
            this["DockTabPillForegroundBrush"] = new SolidColorBrush(pillColor, 0.7);
        }

        MapDockBrush("DockThemeForegroundBrush", Foreground);
        MapDockBrush("DockThemeBackgroundBrush", SideBarBackground);
        MapDockBrush("DockThemeBorderLowBrush", EditorGroupBorder, PanelBorder);
        MapDockBrush("DockThemeControlBackgroundBrush", EditorBackground);
        MapDockBrush("DockThemeAccentBrush", FocusBorder);
    }

    private void MapDockBrush(string dockKey, params string[] colorIds)
    {
        foreach (var id in colorIds)
        {
            if (TryGetValue(id, out var brush))
            {
                this[dockKey] = brush;
                return;
            }
        }
    }
}
