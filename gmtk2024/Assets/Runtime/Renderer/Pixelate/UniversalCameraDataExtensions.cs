using System.Reflection;
using UnityEngine.Rendering.Universal;

namespace gmtk2024.Runtime.Renderer.Pixelate;

public static class UniversalCameraDataExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetPixelRect(this UniversalCameraData cameraData, Rect pixelRect)
    {
        const string k_PixelRectField = "pixelRect";

        var cameraDataType = typeof(UniversalCameraData);

        var pixelRectField = cameraDataType.GetField(
            k_PixelRectField,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (pixelRectField != null)
            pixelRectField.SetValue(cameraData, pixelRect);
    }
}
