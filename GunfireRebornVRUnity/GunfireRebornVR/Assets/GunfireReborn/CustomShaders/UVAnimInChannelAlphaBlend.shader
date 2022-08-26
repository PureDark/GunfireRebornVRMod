// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "PureDark/UVAnimInChannel_AlphaBlend" {
Properties {
    _MainTexture("Main Texture", 2D) = "white" { }
    _MainTexTilingAndOffset("Main Tex Tiling And Offset", Vector) = (1,1,0,0)
    _MainColor("MainColor (RGB)", Color) = (1,1,1,1)
    _UVXSpeed("UV X Speed", Float) = 0
    _UVYSpeed("UV Y Speed", Float) = 0
    _MaskTex("Mask Tex", 2D) = "white" { }
    _MaskTexTilingAndOffset("Mask Tex Tiling And Offset", Vector) = (1,1,0,0)
    _MaskTexUVXSpeed("MaskTex UV X Speed", Float) = 0
    _MaskTexUVYSpeed("MaskTex UV Y Speed", Float) = 0
    _InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 maskuv : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };


            sampler2D _MainTexture;
            fixed4 _MainColor;
            float4 _MainTexture_ST;
            float4 _MainTexTilingAndOffset;
            sampler2D   _MaskTex;
            float4      _MaskTex_ST;
            float4 _MaskTexTilingAndOffset;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                #ifdef SOFTPARTICLES_ON
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
                #endif
                o.color = v.color * _MainColor;
                _MainTexture_ST = _MainTexTilingAndOffset;
                o.uv = TRANSFORM_TEX(v.uv, _MainTexture);
                _MaskTex_ST = _MaskTexTilingAndOffset;
                o.maskuv = TRANSFORM_TEX(v.uv, _MaskTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;
            float _UVXSpeed;
            float _UVYSpeed;
            float _MaskTexUVXSpeed;
            float _MaskTexUVYSpeed;

            fixed4 TwoColorBlend(fixed4 c1, fixed4 c2)
            {
                fixed4 c12;
                c12.a = c1.a + c2.a - c1.a * c2.a;
                c12.rgb = (c1.rgb * c1.a * (1 - c2.a) + c2.rgb * c2.a) / c12.a;
                return c12;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv_offset = float2(0, 0);
                uv_offset.x = _Time.y * _UVXSpeed;
                uv_offset.y = _Time.y * _UVYSpeed;
                float2 mask_uv_offset = float2(0, 0);
                mask_uv_offset.x = _Time.y * _MaskTexUVXSpeed;
                mask_uv_offset.y = _Time.y * _MaskTexUVYSpeed;

                #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
                #endif

                fixed4 col = 2.0f * i.color * tex2D(_MainTexture, (i.uv + uv_offset) % 1);
                col.a = saturate(col.a); // alpha should not have double-brightness applied to it, but we can't fix that legacy behavior without breaking everyone's effects, so instead clamp the output to get sensible HDR behavior (case 967476)

                fixed4 maskcol = i.color * tex2D(_MaskTex, (i.maskuv + mask_uv_offset) % 1);

                UNITY_APPLY_FOG(i.fogCoord, col);
                col.a *= min((maskcol.x+ maskcol.y + maskcol.z)/3, maskcol.a);
                return col;
            }
            ENDCG
        }
    }
}
}
