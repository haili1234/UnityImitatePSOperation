Shader "Custom/Tailor"
{
	//裁剪
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
				float x1;
				float y1;
				float x2;
				float y2;
				float Scalex;
				float Scaley;
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
			i.uv.x/=Scalex;
			i.uv.y/=Scaley;
			if(x2>x1&&y2<y1){
			i.uv.x=i.uv.x+x1;
			i.uv.y=i.uv.y+y2;}
			else if(x2>x1&&y2>y1){
			i.uv.x=i.uv.x+x1;
			i.uv.y=i.uv.y+y1;
			}
			else if(y2>y1&&x2<x1){
			i.uv.x=i.uv.x+x2;
			i.uv.y=i.uv.y+y1;
			}
			else{
			i.uv.x=i.uv.x+x2;
			i.uv.y=i.uv.y+y2;
			}
			fixed4 renderTex = tex2D(_MainTex,i.uv);
			return fixed4(renderTex);
			}
			ENDCG
		}
	}
			Fallback Off
}
