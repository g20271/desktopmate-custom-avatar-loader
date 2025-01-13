using MelonLoader;

namespace CustomAvatarLoader.Settings;

public class MelonLoaderSettings : ISettingsProvider
{
    private readonly string _defaultCategory;

    public MelonLoaderSettings(string defaultCategory)
    {
        _defaultCategory = defaultCategory;
    }

    public T? Get<T>(string setting)
    {
        if (string.IsNullOrEmpty(setting))
        {
            return default;
        }
        
        var settingSections = setting.Split(".");
        var category = settingSections.Length > 1 ? settingSections[0] : _defaultCategory;
        var key = settingSections.Length > 1 ? settingSections[1] : settingSections[0];
        
        return MelonPreferences.CreateCategory(category).CreateEntry<T>(key, default).Value;
    }

    public T? Set<T>(string setting, T value)
    {
        if (string.IsNullOrEmpty(setting))
        {
            return default;
        }
        
        var settingSections = setting.Split(".");
        var category = settingSections.Length > 1 ? settingSections[0] : _defaultCategory;
        var key = settingSections.Length > 1 ? settingSections[1] : settingSections[0];
        
        MelonPreferences.CreateCategory(category).CreateEntry<T>(key, default).Value = value;

        return value;
    }

    public void SaveSettings()
    {
        MelonPreferences.Save();
    }
}