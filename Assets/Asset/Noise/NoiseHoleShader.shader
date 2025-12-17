Shader "Custom/NoiseHoleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _HoleRadius ("Hole Radius", Range(0, 1)) = 0.2
        _Softness ("Edge Softness", Range(0, 1)) = 0.1
        _PlayerPos ("Player Position", Vector) = (0.5, 0.5, 0, 0)
        _NoiseColor ("Noise Color", Color) = (1,0,0,1) // 기본값 빨강
    }
    SubShader
    {
        Tags { "Queue"="Overlay+1" "IgnoreProjector"="True" "RenderType"="Transparent" }
        ZWrite Off
        ZTest Always // [핵심] 벽 뚫고 그리기
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _PlayerPos;
            float _HoleRadius;
            float _Softness;
            fixed4 _NoiseColor;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; 
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                screenUV.x *= aspect;
                float2 playerPosAdjusted = _PlayerPos.xy;
                playerPosAdjusted.x *= aspect;
                
                float dist = distance(screenUV, playerPosAdjusted);
                float mask = smoothstep(_HoleRadius, _HoleRadius + _Softness, dist);

                // 색상 * 노이즈패턴(col.a) * 구멍(mask) * 전체투명도(i.color.a)
                fixed4 finalColor = _NoiseColor;
                finalColor.a *= col.a * mask * i.color.a;

                return finalColor;
            }
            ENDCG
        }
    }
}