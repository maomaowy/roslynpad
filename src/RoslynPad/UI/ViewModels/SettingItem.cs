using System.Reflection;
using NuGet.Versioning;
using RoslynPad.Localization;

namespace RoslynPad.UI;

/// <summary>
/// Represents a single editable setting.
/// </summary>
public class SettingItem(PropertyInfo property, IApplicationSettingsValues settings, string displayName, string? description, Type propertyType) : NotificationObject
{
    private const string ValidationErrorId = "Validation";
    private object? _value = property.GetValue(settings);
    private IReadOnlyList<SettingOption>? _enumOptions;

    public PropertyInfo Property { get; } = property;
    public IApplicationSettingsValues Settings { get; } = settings;
    public string DisplayName { get; } = displayName;
    public string? Description { get; } = description;
    public Type PropertyType { get; } = propertyType;

    internal Type NonNullablePropertyType => Nullable.GetUnderlyingType(PropertyType) ?? PropertyType;

    public bool IsBoolean => PropertyType == typeof(bool);
    public bool IsNumeric => PropertyType == typeof(int) || PropertyType == typeof(double) || PropertyType == typeof(double?);
    public bool IsString => PropertyType == typeof(string);
    public bool IsEnum => NonNullablePropertyType.IsEnum;
    public bool IsStringArray => PropertyType == typeof(string[]);

    public object? Value
    {
        get => _value;
        set => SetValue(value, nameof(Value));
    }

    public bool BoolValue
    {
        get => Value is true;
        set => Value = value;
    }

    public string? StringValue
    {
        get => Value as string;
        set => SetValue(value, nameof(StringValue));
    }

    public double NumericValue
    {
        get => Value switch
        {
            int i => i,
            double d => d,
            _ => 0
        };
        set
        {
            if (PropertyType == typeof(int))
            {
                SetValue((int)value, nameof(NumericValue));
            }
            else if (PropertyType == typeof(double) || PropertyType == typeof(double?))
            {
                SetValue(value, nameof(NumericValue));
            }
        }
    }

    public string? StringArrayValue
    {
        get => Value is string[] arr ? string.Join(Environment.NewLine, arr) : null;
        set => SetValue(value?.Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [], nameof(StringArrayValue));
    }

    private bool IsRequiredString => Property.Name is nameof(IApplicationSettingsValues.EditorFontFamily);

    public IReadOnlyList<SettingOption>? EnumOptions => IsEnum ? _enumOptions ??= CreateEnumOptions() : null;

    public SettingOption? SelectedEnumOption
    {
        get => EnumOptions?.FirstOrDefault(option => Equals(option.Value, Value));
        set
        {
            if (value is null)
            {
                return;
            }

            Value = value.Value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedEnumValue));
        }
    }

    public object? SelectedEnumValue
    {
        get => SelectedEnumOption;
        set => SelectedEnumOption = value as SettingOption;
    }

    private List<SettingOption> CreateEnumOptions()
    {
        var options = new List<SettingOption>();

        if (PropertyType != NonNullablePropertyType)
        {
            options.Add(new SettingOption(null,
                LocalizationManager.Get($"Enum_{NonNullablePropertyType.Name}_NotSet", LocalizationManager.Get("Enum_ThemeType_NotSet"))));
        }

        foreach (var value in Enum.GetValues(NonNullablePropertyType))
        {
            var name = Enum.GetName(NonNullablePropertyType, value);
            options.Add(new SettingOption(value, LocalizationManager.Get($"Enum_{NonNullablePropertyType.Name}_{name}", name ?? string.Empty)));
        }

        return options;
    }

    public bool MatchesFilter(string[]? filterTerms)
    {
        if (filterTerms is null || filterTerms.Length == 0)
        {
            return true;
        }

        // All terms must match (AND logic)
        foreach (var term in filterTerms)
        {
            if (!DisplayName.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                !(Description?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                return false;
            }
        }

        return true;
    }

    private void SetValue(object? value, string sourcePropertyName)
    {
        ClearErrors(nameof(StringValue));
        ClearErrors(nameof(NumericValue));
        ClearErrors(nameof(StringArrayValue));

        value = NormalizeValue(value);
        var validationError = ValidateValue(value);
        if (validationError is not null)
        {
            SetError(sourcePropertyName, ValidationErrorId, validationError);
            return;
        }

        if (SetProperty(ref _value, value, nameof(Value)))
        {
            Property.SetValue(Settings, value);
            _value = Property.GetValue(Settings);
            OnPropertyChanged(nameof(BoolValue));
            OnPropertyChanged(nameof(StringValue));
            OnPropertyChanged(nameof(NumericValue));
            OnPropertyChanged(nameof(StringArrayValue));
            OnPropertyChanged(nameof(SelectedEnumOption));
            OnPropertyChanged(nameof(SelectedEnumValue));
        }
    }

    private object? NormalizeValue(object? value)
    {
        if (PropertyType == typeof(string))
        {
            var stringValue = value as string;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return IsRequiredString ? string.Empty : null;
            }

            stringValue = stringValue.Trim();
            if (Property.Name == nameof(IApplicationSettingsValues.DefaultPlatformName))
            {
                var lastSpaceIndex = stringValue.LastIndexOf(' ');
                if (lastSpaceIndex >= 0 && NuGetVersion.TryParse(stringValue[(lastSpaceIndex + 1)..], out var version))
                {
                    return version.ToNormalizedString();
                }
            }

            return stringValue;
        }

        return value;
    }

    private string? ValidateValue(object? value)
    {
        if (Property.Name == nameof(IApplicationSettingsValues.EditorFontFamily) && value is not string { Length: > 0 })
        {
            return LocalizationManager.Get("Validation_EditorFontFamily_Required");
        }

        switch (Property.Name)
        {
            case nameof(IApplicationSettingsValues.DocumentPath) when value is string documentPath && !Directory.Exists(documentPath):
                return LocalizationManager.Get("Validation_DocumentPath_Invalid");

            case nameof(IApplicationSettingsValues.SdkLocation) when value is string sdkLocation && !Directory.Exists(sdkLocation):
                return LocalizationManager.Get("Validation_SdkLocation_Invalid");

            case nameof(IApplicationSettingsValues.CustomThemePath) when value is string themePath && !File.Exists(themePath):
                return LocalizationManager.Get("Validation_CustomThemePath_Invalid");

            case nameof(IApplicationSettingsValues.DefaultPlatformName) when value is string platformName && !NuGetVersion.TryParse(platformName, out _):
                return LocalizationManager.Get("Validation_DefaultPlatformName_Invalid");

            case nameof(IApplicationSettingsValues.EditorFontSize) or nameof(IApplicationSettingsValues.OutputFontSize) when value is double fontSize && (fontSize < 8 || fontSize > 72):
                return LocalizationManager.Get("Validation_FontSize_Invalid");

            case nameof(IApplicationSettingsValues.WindowFontSize) when value is double windowFontSize && windowFontSize <= 0:
                return LocalizationManager.Get("Validation_WindowFontSize_Invalid");

            case nameof(IApplicationSettingsValues.LiveModeDelayMs) when value is int liveModeDelayMs && liveModeDelayMs < 0:
                return LocalizationManager.Get("Validation_LiveModeDelay_Invalid");

            default:
                return null;
        }
    }
}

public sealed class SettingOption(object? value, string displayName)
{
    public object? Value { get; } = value;

    public string DisplayName { get; } = displayName;

    public override string ToString() => DisplayName;

    public override bool Equals(object? obj) => obj is SettingOption other && Equals(Value, other.Value);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}
