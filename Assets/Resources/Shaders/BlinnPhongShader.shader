Shader "Unlit/BlinnPhongShader"
{
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase
			#pragma target 3.0

			// Light values. Support at most 5 lights
			uniform int _PointLightCount;
			uniform float3 _PointLightColors[5];
			uniform float3 _PointLightPositions[5];
			uniform float2 _PointLightAttenuations[5];

			// Lighting parameters
			uniform float _Ka;		// Ambient albedo
			uniform float _Kd;		// Diffuse albedo
			uniform float _Ks;		// Specular albedo
			uniform float _N;		// Specular exponent

			// Color parameter
			uniform float4 _Color;

			// Fog parameters
			uniform float4 _fogColor;
			uniform float _fogDensity;

			struct vertIn {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};

			struct vertOut {
				float4 pos : SV_POSITION;
				float3 worldVertex : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 color : COLOR;
				LIGHTING_COORDS(2,3)
			};

			fixed4 calculateLightColor(vertOut v, float3 normal) {
				// Sum diffuse and specular light for each source
				float3 sum = float3(0, 0, 0);
				for (int i = 0; i < _PointLightCount; ++i) {
					// This is spaghetti. The first part is calculating an attenuation factor
					// using a Gaussian distribution; then the second part is the diffuse and
					// specular terms. Sadly Windows Store has an incredibly strict limit
					// on how many registers the GPU can access, so I have to mash it all together
					// to reduce the number of local variables.
					sum += saturate(exp(-pow(_PointLightAttenuations[i].x *
						length(_PointLightPositions[i] - v.worldVertex), 2)))
						* (LIGHT_ATTENUATION(v) * _PointLightColors[i].rgb
							* _Kd * v.color.rgb * saturate(dot(normalize(_PointLightPositions[i] - v.worldVertex), normal))
							+ _PointLightColors[i].rgb
							* _Ks * pow(saturate(dot(
								normalize(_WorldSpaceCameraPos - v.worldVertex + _PointLightPositions[i] - v.worldVertex),
								normal)), _N));
				}
				// Calculate ambient light and fog here
				return fixed4(lerp(_fogColor.rgb, v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * _Ka + sum,
								   saturate(exp(-length(_WorldSpaceCameraPos - v.worldVertex) * _fogDensity))),
					LIGHT_ATTENUATION(v) * v.color.a);
			}
			
			vertOut vert(vertIn v)
			{
				vertOut o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				// Convert position to world space
				o.worldVertex = mul(_Object2World, v.vertex);
				// Convert normals to local space. This is taking advantage of the transpose
				// nature of pre vs post multiplication
				o.worldNormal = normalize(mul(v.normal, _World2Object).xyz);
				o.color = _Color;
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}
			
			fixed4 frag(vertOut v) : SV_Target
			{
				return calculateLightColor(v, v.worldNormal);
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}
