Shader "IndieChest/OneClickLightningShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Lightning Color", Color) = (0,0,0,0)
		_LineSize("Line Size", Range(0.001, 1.0)) = 0.001
		_Mitigation("Mitigation", Float) = 0.0
		_Speed ("Speed", Float) = 0.0
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" }
		LOD 100
			ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
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
			float4 _MainTex_ST;
			float4 _Color;
			float _LineSize;
			float _Mitigation;
			float _Speed;
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				float noise = tex2D(_MainTex, i.uv + _Time.x * _Speed).a * _Mitigation;
				fixed4 col = (step(0.5 - _LineSize, i.uv.y + noise) * step(i.uv.y + noise, 0.5 + _LineSize)) * _Color;
				return col;
			}
		ENDCG
	}
	}
}
