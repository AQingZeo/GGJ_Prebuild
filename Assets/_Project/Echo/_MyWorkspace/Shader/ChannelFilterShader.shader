Shader "Custom/SimpleMultiMask"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _RedWeight ("Red Weight", Range(0,1)) = 0
        _BlueWeight ("Blue Weight", Range(0,1)) = 0
        _YellowWeight ("Yellow Weight", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float _RedWeight, _BlueWeight, _YellowWeight;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                fixed3 redColor = fixed3(1, 0, 0);
                fixed3 blueColor = fixed3(0, 0, 1);
                fixed3 yellowColor = fixed3(1, 1, 0);


                fixed3 finalRGB = col.rgb;
                finalRGB = lerp(finalRGB, redColor, _RedWeight);
                finalRGB = lerp(finalRGB, blueColor, _BlueWeight);
                finalRGB = lerp(finalRGB, yellowColor, _YellowWeight);

                return fixed4(finalRGB, col.a);
            }
            ENDCG
        }
    }
}