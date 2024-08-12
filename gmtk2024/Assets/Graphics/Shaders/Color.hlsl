#include "Assets/Graphics/ShaderLibrary/Color.hlsl"
#include "Assets/Graphics/ShaderLibrary/Core.hlsl"

void RGB_to_OKLCH_float(float4 rgba, out float3 oklch)
{
    oklch = srgb2oklch(rgba.rgb);
}

void OKLCH_to_RGB_float(float3 oklch, out float4 rgb)
{
    rgb = float4(oklch2srgb(oklch), 1.0);
}

void Palette_float(
    float4 col,
    float3 base,
    float brightness,
    float darkness,
    float tint,
    float saturation,
    float2 uv,
    float steps_hue,
    float steps_sat,
    float steps_val,
    out float4 rgba)
{
    float3 lch = srgb2oklch(col.rgb);
    float3 base_lch = srgb2oklch(base);

    float bri = clamp01(pow(lch.x, 2.0 - brightness) + (darkness - 1.0));

    float bd = (base_lch.x - bri);
    float hue = base_lch.z + (bd * tint);
    float sat = base_lch.y + (bd * saturation);

    float3 result = float3(
        clamp01(posterize(bri, steps_val)),
        clamp01(posterize(sat, steps_sat)),
        clamp01(posterize(hue, steps_hue))
    );

    rgba =  float4(oklch2srgb(result), col.a);
}

void Palette_HSV_float(
    float4 col,
    float3 base,
    float brightness,
    float darkness,
    float tint,
    float saturation,
    float2 uv,
    float steps_hue,
    float steps_sat,
    float steps_val,
    out float4 rgba)
{
    float3 hsv = rgb2hsv(col.rgb);
    float3 base_hsv = rgb2hsv(base);

    float bri = clamp01(pow(hsv.b, 2.0 - brightness) + (darkness - 1.0));

    float bd = (base_hsv.b - bri);
    float hue = base_hsv.r + (bd * tint);
    float sat = base_hsv.g + (bd * saturation);

    float3 result = float3(
        clamp01(posterize(hue, steps_hue)),
        clamp01(posterize(sat, steps_sat)),
        clamp01(posterize(bri, steps_val))
    );

    rgba =  float4(hsv2rgb(result), 1.0);
}
