namespace CustomAvatarLoader.Models;

using System.Collections.Generic;

public interface IModelPackageHandler
{
    void Import(string path);

    IEnumerable<ModelPackage> ListModels();
}