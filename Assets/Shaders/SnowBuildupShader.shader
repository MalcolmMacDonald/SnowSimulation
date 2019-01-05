Shader "Unlit/SnowBuildupShader"
{
	Properties{
	_SnowMask("Snow Mask", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Zwrite On 
		LOD 100
		AlphaToMask On
		//BlendOp Max
		//Blend OneMinusSrcAlpha One 
		//Blend One One
		Blend OneMinusSrcAlpha SrcAlpha 

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

            struct v2f {
                // we'll output world space normal as one of regular ("texcoord") interpolators
                half3 worldNormal : TEXCOORD0;
				float2 uv : TEXCOORD1;
                float4 pos : SV_POSITION;
            };
			sampler2D _SnowMask;
			float4 _SnowMask_ST;

            // vertex shader: takes object space normal as input too
            v2f vert (float4 vertex : POSITION, float3 normal : NORMAL, float2 uv : TEXCOORD0)
            {
                v2f o;
				o.uv = TRANSFORM_TEX(uv, _SnowMask);
                o.pos = UnityObjectToClipPos(vertex);
				o.worldNormal = UnityObjectToWorldNormal(normal);
                return o;
            }

			fixed4 frag (v2f i) : SV_Target 
			{
				fixed4 col = fixed4(i.worldNormal.rgb,1);
				float maskValue = 1 - tex2D(_SnowMask, i.uv).r;
				col.a = pow(maskValue,2);
				col.rgb = 1;
				return col;
			}
			ENDCG
		}
	}
}
