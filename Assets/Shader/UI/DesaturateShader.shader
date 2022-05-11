Shader "Unlit/DesaturateShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("Alpha", 2D) = "white" {}
        [MaterialToggle] _Desaturate ("Desaturate", float) = 0
        [MaterialToggle] _BlackOut ("Black Out", float) = 0
        _BlackOutAmount ("Black Out Amount", Range(0, 1)) = 0.75

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Alpha;
            float4 _MainTex_ST;
            float _Desaturate;
            float _BlackOut;
            float _BlackOutAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float4 alpha = tex2D(_Alpha, i.uv);
                
                float ill = col.x * 0.2126f + col.y * 0.7152f + col.z * 0.0722f;

                float black = 1.0f - _BlackOutAmount * _BlackOut;

                return ((float4(ill,ill,ill,col.w * alpha.w) * _Desaturate) + col * (1.0f - _Desaturate)) * black;
            }
            ENDCG
        }
    }
}
