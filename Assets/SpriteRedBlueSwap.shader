Shader "Custom/SpriteRedBlueSwap"
{
    Properties
    {
        [PerRendererData] _MainTex(
            "Sprite Texture",
            2D) = "white" {}

        _Color(
            "Tint",
            Color) = (1, 1, 1, 1)

        [Range(0, 1)] _SwapThreshold(
            "Swap Threshold",
            Range(0, 1)) = 0.35

        [MaterialToggle] PixelSnap(
            "Pixel Snap",
            Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            #pragma vertex SpriteVert
            #pragma fragment Fragment
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            fixed _SwapThreshold;

            fixed4 Fragment(v2f input) : SV_Target
            {
                fixed4 color =
                    SampleSpriteTexture(input.texcoord)
                    * input.color;

                fixed redDifference =
                    color.r - max(color.g, color.b);

                fixed blueDifference =
                    color.b - max(color.r, color.g);

                fixed redMask =
                    step(_SwapThreshold, redDifference);

                fixed blueMask =
                    step(_SwapThreshold, blueDifference);

                fixed swapMask =
                    saturate(redMask + blueMask);

                fixed3 swappedColor =
                    color.bgr;

                color.rgb = lerp(
                    color.rgb,
                    swappedColor,
                    swapMask);

                color.rgb *= color.a;

                return color;
            }

            ENDCG
        }
    }
}