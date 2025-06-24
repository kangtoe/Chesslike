Shader "Sprites/Inline"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0

        [PerRendererData] _Line("Line", Float) = 0
        [PerRendererData] _LineColor("Line Color", Color) = (1,1,1,1)
        [PerRendererData] _LineSize("Line Size", int) = 0
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
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            float _Line;
            fixed4 _LineColor;
            int _LineSize;
            float4 _MainTex_TexelSize;

            float SampleAlphaSafe(float2 uv)
            {
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return 0.0;
                return tex2D(_MainTex, uv).a;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                if (_Line > 0 && c.a > 0.0)
                {
                    float hasTransparentNeighbor = 0.0;

                    [unroll(16)]
                    for (int i = 1; i <= _LineSize; i++)
                    {
                        float2 uvUp = IN.texcoord + float2(0, i * _MainTex_TexelSize.y);
                        float2 uvDown = IN.texcoord - float2(0, i * _MainTex_TexelSize.y);
                        float2 uvRight = IN.texcoord + float2(i * _MainTex_TexelSize.x, 0);
                        float2 uvLeft = IN.texcoord - float2(i * _MainTex_TexelSize.x, 0);

                        hasTransparentNeighbor = max(hasTransparentNeighbor, 1.0 - SampleAlphaSafe(uvUp));
                        hasTransparentNeighbor = max(hasTransparentNeighbor, 1.0 - SampleAlphaSafe(uvDown));
                        hasTransparentNeighbor = max(hasTransparentNeighbor, 1.0 - SampleAlphaSafe(uvRight));
                        hasTransparentNeighbor = max(hasTransparentNeighbor, 1.0 - SampleAlphaSafe(uvLeft));
                    }

                    if (hasTransparentNeighbor > 0.0)
                    {
                        return _LineColor;
                    }
                }

                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
