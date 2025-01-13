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
        if (string.IsNullOrEmpty(setting)) return default;
        
        String[] settingSections = setting.Split(".");
        String category = settingSections.Length > 1 ? settingSections[0] : _defaultCategory;
        String key = settingSections.Length > 1 ? settingSections[1] : settingSections[0];
        
        return MelonPreferences.CreateCategory(category).CreateEntry<T>(key, default).Value;
    }

    public void Set<T>(string setting, T value)
    {
        if (string.IsNullOrEmpty(setting)) return;
        
        String[] settingSections = setting.Split(".");
        String category = settingSections.Length > 1 ? settingSections[0] : _defaultCategory;
        String key = settingSections.Length > 1 ? settingSections[1] : settingSections[0];
        
        MelonPreferences.CreateCategory(category).CreateEntry<T>(key, default).Value = value;
    }

    public void SaveSettings()
    {
        MelonPreferences.Save();
    }
}