Shader "Custom/ChannelFilterShader"
{
Properties
{
    _MainTex ("Texture", 2D) = "white" {}
    _RedMask ("Red Mask Switch", Range(0, 1)) = 1
    _GreenMask ("Green Mask Switch", Range(0, 1)) = 1
    _BlueMask ("Blue Mask Switch", Range(0, 1)) = 1
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

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _FilterColor;
            float _RedMask;
            float _GreenMask;
            float _BlueMask;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

fixed4 frag (v2f i) : SV_Target {
    fixed4 col = tex2D(_MainTex, i.uv);

    // 分别提取 R, G, B 三个通道的灰度图效果
    float r = col.r * _RedMask;
    float g = col.g * _GreenMask;
    float b = col.b * _BlueMask;

    // 将它们重新组合成一个颜色
    // 如果红蓝都开启，r=1, g=0, b=1，就会看到紫色调的画面
    return fixed4(r, g, b, 1);
}
            ENDCG
        }
    }
}