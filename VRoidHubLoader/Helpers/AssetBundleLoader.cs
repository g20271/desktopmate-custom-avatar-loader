namespace CustomAvatarLoader.Helpers;

using Il2CppUniGLTF;
using Il2CppUniVRM10;
using UnityEngine;
using ILogger = Logging.ILogger;

public class AssetBundleLoader
{
    ILogger _logger;

    public AssetBundleLoader(ILogger logger)
    {
        _logger = logger;
    }

    public GameObject? LoadAssetBundleIntoScene(string path)
    {
        _logger.Debug($"Loading Asset Bundle Into Scene: \"{path}\"");

        var bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            _logger.Error("Failed to load asset bundle!");
            return null;
        }

        var prefabPath = "Assets/CustomAvatarExporter/temp/TempAvatar.prefab";
        var model = bundle.LoadAsset<GameObject>(prefabPath);
        if (model == null)
        {
            _logger.Error($"Failed to find prefab in bundle at path \"{prefabPath}\"");
            return null;
        }

        var instance = Object.Instantiate(model, Vector3.zero, Quaternion.identity);
        instance.transform.position = Vector3.zero;
        instance.transform.rotation = Quaternion.identity;

        return instance;
    }
}