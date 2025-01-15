namespace CustomAvatarLoader.Models;

using System.Reflection;
using System.Text.Json.Serialization;

public class ModelPackage
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("package-author")]
    public string PackageAuthor { get; set; }

    [JsonPropertyName("model-author")]
    public string ModelAuthor { get; set; }

    [JsonPropertyName("model-url")]
    public string ModelUrl { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    public string PackageBasePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "vrm", $"{Name}_{PackageAuthor}");

    public string ConfigPath => Path.Combine(PackageBasePath, "config.json");

    public string ModelPath => Path.Combine(PackageBasePath, "model.vrm");

    public string BannerPath => Path.Combine(PackageBasePath, "banner.png");
}