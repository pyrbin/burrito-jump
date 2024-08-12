using System.Linq;
using UnityEditor;
using UnityEngine;

namespace pyr.Shared.Editor.Extensions;

public static class AssetDatabaseExtensions
{
    public static bool IsObjectPartOfSpecificAsset(
        Object targetObject,
        Object asset
    )
    {
        if (asset == null || targetObject == null)
        {
            Debug.LogError("targetObject or asset object is null.");
            return false;
        }

        Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(
            AssetDatabase.GetAssetPath(asset)
        );

        return subAssets.Any(subAsset => subAsset == targetObject);
    }
}
