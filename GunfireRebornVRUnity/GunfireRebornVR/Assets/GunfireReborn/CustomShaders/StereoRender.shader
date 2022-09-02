Shader "PureDark/StereoRender"
{
    Properties
    {
        _LeftFarTex("Texture", 2D) = "white" {}
        _RightFarTex("Texture", 2D) = "white" {}
        _LeftNearTex("Texture", 2D) = "white" {}
        _RightNearTex("Texture", 2D) = "white" {}
        _LeftUITex("Texture", 2D) = "white" {}
        _RightUITex("Texture", 2D) = "white" {}
    }

        SubShader
    {
        Tags { }
        Pass
        {
            CGPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag

                sampler2D _LeftFarTex;
                sampler2D _RightFarTex;
                sampler2D _LeftNearTex;
                sampler2D _RightNearTex;
                sampler2D _LeftUITex;
                sampler2D _RightUITex;

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

                half4 _LeftFarTex_ST;
                half4 _RightFarTex_ST;
                half4 _LeftNearTex_ST;
                half4 _RightNearTex_ST;
                half4 _LeftUITex_ST;
                half4 _RightUITex_ST;

                float4 Frag(v2f i) : SV_Target
                {
                    float4 o = float4(0, 0, 0, 1);
                    if (unity_StereoEyeIndex == 0) {
                        o = tex2D(_LeftFarTex, i.uv);
                        fixed4 nearCol = tex2D(_LeftNearTex, i.uv);
                        o.rgb = o.rgb * (1 - nearCol.a) + nearCol.rgb * nearCol.a;
                        fixed4 UICol = tex2D(_LeftUITex, i.uv);
                        UICol.a = min(UICol.a * 2, 1);
                        o.rgb = o.rgb * (1 - UICol.a) + UICol.rgb * UICol.a;
                    }
                    else {
                        o = tex2D(_RightFarTex, i.uv);
                        fixed4 nearCol = tex2D(_RightNearTex, i.uv);
                        o.rgb = o.rgb * (1 - nearCol.a) + nearCol.rgb * nearCol.a;
                        fixed4 UICol = tex2D(_RightUITex, i.uv);
                        UICol.a = min(UICol.a * 2, 1);
                        o.rgb = o.rgb * (1 - UICol.a) + UICol.rgb * UICol.a;
                    }

                    return o;
                }
            ENDCG
        }
    }
}