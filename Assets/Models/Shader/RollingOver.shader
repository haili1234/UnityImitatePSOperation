Shader "Custom/RollingOver"
{
	//翻转处理
	Properties
	{
		_MainTex ("_MainTex", 2D) = "white" {}
	}
		SubShader
		{
			Pass
			{
				ZTest Always Cull Off ZWrite Off
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				sampler2D _MainTex;
				int _Horizon;
				int _Vertical;
			struct v2a
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};
			v2f vert (v2a v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
			if(_Horizon==1){
				i.uv.x=1-i.uv.x;
			}
			if(_Vertical==1){
				i.uv.y=1-i.uv.y;
			}
			fixed4 renderTex = tex2D(_MainTex,i.uv);
			return fixed4(renderTex);
			}
			ENDCG
		}
	}
			Fallback Off
}
