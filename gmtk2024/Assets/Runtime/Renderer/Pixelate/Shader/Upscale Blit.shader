Shader "Hidden/Pixelate/Upscale Blit"
{
    HLSLINCLUDE
    #pragma target 2.0
    #pragma editor_sync_compilation

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float2 BilinearUV(float2 uv, float4 texelSize)
    {
        // Box filter size in texel units
        float2 box_size = clamp(fwidth(uv) * texelSize.zw, 1e-5, 1);
        // Scale uv by texture size to get texel coordinates
        float2 tx = uv * texelSize.zw - 0.5 * box_size;
        // Compute offset for pixel-sized box filter
        float2 tx_offset = smoothstep(1 - box_size, 1, frac(tx));
        // Compute bilinear sample uv coordinates
        float2 sample_uv = (floor(tx) + 0.5 + tx_offset) * texelSize.xy;
        return sample_uv;
    }

    half4 Frag(Varyings IN) : SV_Target
    {
        float2 uv = BilinearUV(IN.texcoord, _BlitTexture_TexelSize);
        return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv);
    }
    ENDHLSL

    SubShader
    {
        Tags{ "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            ZWrite Off ZTest Always Blend Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}
