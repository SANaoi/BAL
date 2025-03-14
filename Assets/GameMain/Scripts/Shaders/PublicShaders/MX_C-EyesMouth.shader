Shader "Custom/C-EyeMouthShader"
{
    Properties
    {
        _MainTex("Face Texture", 2D) = "white" {}
        _MouthTex("Mouth Texture", 2D) = "white" {}
        _MouthBlend("Mouth Blend Factor", Range(0, 1)) = 0.5
        _offsetX("X offset", float) = 0
        _offsetY("Y offset", float) = 0
    }
        SubShader
        {
            Tags
            {
                "RenderType" = "Opaque"
                "RenderPipeline" = "UniversalPipeline"
                "UniversalMaterialType" = "Lit"
                "IgnoreProjector" = "True"
            }
            LOD 200

            Pass
            {
                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                sampler2D _MouthTex;
                float4 _MouthTex_ST;
                float _MouthBlend;
                float _offsetX;
                float _offsetY;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 faceColor = tex2D(_MainTex, i.uv);
                    float2 mouthUV = i.uv + float2(_offsetX, _offsetY);
                    fixed4 mouthColor = tex2D(_MouthTex, mouthUV);
                    if (i.uv.x < 0.25 && i.uv.y < 0.25)
                    {
                        if (mouthColor.a > 0.0)
                        {
                            faceColor.a = 1.0;
                            faceColor = lerp(faceColor, mouthColor, mouthColor.a * _MouthBlend);
                        }
                        else
                        {
                            faceColor.a = 0.0;
                        }
                    }
                    return faceColor;
                }
                ENDCG
            }
        }
            FallBack "Transparent/VertexLit"
}
