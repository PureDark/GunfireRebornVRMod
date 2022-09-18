Shader "PureDark/StereoRender"
{
    Properties
    {
        _LeftFirstTex("Texture", 2D) = "white" {}
        _RightFirstTex("Texture", 2D) = "white" {}
        _LeftLastTex("Texture", 2D) = "white" {}
        _RightLastTex("Texture", 2D) = "white" {}
        _AlphaMultiplier("AlphaMultiplier", float) = 1
    }

        SubShader
    {
        Tags { }
        Pass
        {
            CGPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag

                sampler2D _LeftFirstTex;
                sampler2D _RightFirstTex;
                sampler2D _LeftLastTex;
                sampler2D _RightLastTex;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                v2f Vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                half4 _LeftFirstTex_ST;
                half4 _RightFirstTex_ST;
                half4 _LeftLastTex_ST;
                half4 _RightLastTex_ST;
                float _AlphaMultiplier;

                float4 Frag(v2f i) : SV_Target
                {
                    fixed4 o;
                    fixed4 lastCol;
                    if (unity_StereoEyeIndex == 0) {
                        o = tex2D(_LeftFirstTex, i.uv);
                        lastCol = tex2D(_LeftLastTex, i.uv);
                        //o = fixed4(1, 0, 0, 1);
                    }
                    else {
                        o = tex2D(_RightFirstTex, i.uv);
                        lastCol = tex2D(_RightLastTex, i.uv);
                        //o = fixed4(0, 0, 1, 1);
                    }
                    lastCol.a = min(lastCol.a * _AlphaMultiplier, 1);
                    o.rgb = o.rgb * (1 - lastCol.a) + lastCol.rgb * lastCol.a;
                    return o;
                }
            ENDCG
        }
    }
}