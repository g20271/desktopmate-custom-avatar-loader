namespace CustomAvatarLoader.Models;

using CustomAvatarLoader.Logging;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

public class ModelPackageLoader : IModelPackageHandler
{
    public ModelPackageLoader(ILogger logger)
    {
        Logger = logger;
    }

    protected virtual ILogger Logger { get; }

    public void Import(string path)
    {
        try
        {
            var guid = Guid.NewGuid();
            var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var extractPath = Path.Combine(assemblyDirectory, "temp", guid.ToString());

            if (!File.Exists(Path.Combine(extractPath, "config.json")))
            {
                throw new ModelPackageImportException("Package is missing config file (\"config.json\")");
            }

            var json = File.ReadAllText(Path.Combine(extractPath, "config.json"));

            var modelConfig = JsonSerializer.Deserialize<ModelPackage>(json);

            #region Package validation

            if (string.IsNullOrEmpty(modelConfig.Name))
            {
                throw new ModelPackageImportException("Model name is missing in config");
            }

            if (!IsValidString(modelConfig.Name))
            {
                throw new ModelPackageImportException("Model name contains invalid characters");
            }

            if (string.IsNullOrEmpty(modelConfig.ModelAuthor))
            {
                throw new ModelPackageImportException("Package author is missing in config");
            }

            if (string.IsNullOrEmpty(modelConfig.Version))
            {
                throw new ModelPackageImportException("Package version is missing in config");
            }

            if (!IsValidVersion(modelConfig.Version))
            {
                throw new ModelPackageImportException("Invalid package version format - must be in the format x.x.x (e.g. 1.0.2)");
            }

            if (!File.Exists(Path.Combine(extractPath, "model.vrm")))
            {
                throw new ModelPackageImportException("Package is missing model file (\"model.vrm\")");
            }

            #endregion

            Directory.CreateDirectory(Path.Combine(assemblyDirectory, "vrm", $"{modelConfig.Name}_{modelConfig.PackageAuthor}"));

            foreach (var file in Directory.GetFiles(extractPath))
            {
                File.Copy(file, Path.Combine(assemblyDirectory, "vrm", $"{modelConfig.Name}_{modelConfig.PackageAuthor}", Path.GetFileName(file)));
            }
        }
        catch (ModelPackageImportException ex)
        {
            Logger.Error($"Error importing model package: {ex.Message}");
            // Show error message box
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to import model package from {path}", ex);
        }
    }

    public IEnumerable<ModelPackage> ListModels()
    {
        var assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var vrmDirectory = Path.Combine(assemblyDirectory, "vrm");
        var models = new List<ModelPackage>();

        try
        {
            // Check if VRM directory exists
            if (!Directory.Exists(vrmDirectory))
            {
                Logger.Warn("\"vrm\" directory not found");
                return Enumerable.Empty<ModelPackage>();
            }

            // Get all subdirectories in the VRM folder
            var modelDirectories = Directory.GetDirectories(vrmDirectory);

            foreach (var modelDir in modelDirectories)
            {
                var configPath = Path.Combine(modelDir, "config.json");

                if (!File.Exists(configPath))
                {
                    Logger.Warn($"Config file not found in directory: {modelDir}");
                    continue;
                }

                try
                {
                    var configJson = File.ReadAllText(configPath);
                    var modelConfig = JsonSerializer.Deserialize<ModelPackage>(configJson);

                    if (modelConfig != null)
                    {
                        models.Add(modelConfig);
                    }
                    else
                    {
                        Logger.Warn($"Failed to deserialize config file: {configPath}");
                    }
                }
                catch (JsonException ex)
                {
                    Logger.Error($"Error parsing config file {configPath}: {ex.Message}");
                }
                catch (IOException ex)
                {
                    Logger.Error($"Error reading config file {configPath}: {ex.Message}");
                }
            }

            return models;
        }
        catch (Exception ex)
        {
            Logger.Error("Error listing model packages", ex);
            return Enumerable.Empty<ModelPackage>();
        }
    }

    protected void ExtractZipFile(
        string zipPath,
        string extractPath)
    {
        try
        {
            // Ensure the ZIP file exists
            if (!File.Exists(zipPath))
                throw new FileNotFoundException("ZIP file not found", zipPath);

            // Create the extraction directory if it doesn't exist
            Directory.CreateDirectory(extractPath);

            int filesExtracted = 0;

            // Open the ZIP archive
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                // Extract each entry
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                    // Ensure the destination path is within the extraction directory (security check)
                    if (!destinationPath.StartsWith(Path.GetFullPath(extractPath)))
                        throw new IOException("Attempted to extract file outside of destination directory");

                    // Create directory for the file if it doesn't exist
                    string? directoryPath = Path.GetDirectoryName(destinationPath);
                    if (directoryPath != null)
                        Directory.CreateDirectory(directoryPath);

                    // Extract the file
                    entry.ExtractToFile(destinationPath, true);
                    filesExtracted++;
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Error extracting ZIP file: {ex.Message}", ex);
        }
    }
    bool IsValidString(string input)
    {
        return Regex.IsMatch(input, @"^[a-zA-Z0-9_ .]{1,30}$");
    }

    bool IsValidVersion(string input)
    {
        return Regex.IsMatch(input, @"^[0-9]+\.[0-9]+\.[0-9]+$");
    }
}