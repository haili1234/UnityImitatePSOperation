Shader "Custom/Rotate"
{
	//旋转
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
				int _Clockwise;
				int _AnuiClockwise;
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
			if(_Clockwise==1){
				i.uv=mul(float2x2(0,-1,1,0),i.uv);
				i.uv.x=1+i.uv.x;
			}
			if( _AnuiClockwise==1){
				i.uv=mul(float2x2(0,1,-1,0),i.uv);
				i.uv.y=1+i.uv.y;//这点很重要，因为对rt采样时，要确保xy都大于0，而texture就不用
			}
			fixed4 renderTex = tex2D(_MainTex,i.uv);
			return fixed4(renderTex);
			}
			ENDCG
		}
	}
			Fallback Off
}
