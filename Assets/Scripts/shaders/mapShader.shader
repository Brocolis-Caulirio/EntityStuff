Shader "Unlit/mapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorE ("Outer color", Color) = (1,1,1,1)
        _ColorH ("Inner Color", Color) = (1,0,0,1)
        _Size("Map Radius", Range(0.,1.)) = 1.
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        zwrite off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 wpos : TEXCOORD1;
                float4 opos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float invLerp(float from, float to, float value) 
            {
                return (value - from) / (to - from);
            }
            float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
            {
                float rel = invLerp(origFrom, origTo, value);
                return lerp(targetFrom, targetTo, rel);
            }
            float2 remap(float origFrom, float origTo, float targetFrom, float targetTo, float2 value)
            {
                float rel1 = invLerp(origFrom, origTo, value.x);
                float rel2 = invLerp(origFrom, origTo, value.y);
                return float2(lerp(targetFrom, targetTo, rel1), lerp(targetFrom, targetTo, rel2));
            }

            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorE)
                UNITY_DEFINE_INSTANCED_PROP(float4, _ColorH)
            UNITY_INSTANCING_BUFFER_END(Props)

            //float4 _ColorE;
            //float4 _ColorH;
            float _Size;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.wpos = mul(unity_ObjectToWorld, v.vertex);
                o.opos = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
            
                UNITY_SETUP_INSTANCE_ID(i);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 cUv = abs(i.uv * 2 - 1);
                float s = 10 - (_Size * 10);
                float interpolator = (length(cUv));
                interpolator = 1 - max(0, remap(0, 1, -(1/s) * 1/s, 1, interpolator));

                col *= lerp(
                    UNITY_ACCESS_INSTANCED_PROP(Props, _ColorE),
                    UNITY_ACCESS_INSTANCED_PROP(Props, _ColorH),
                    interpolator);

                return col;
            }
            ENDCG
        }
    }
}
