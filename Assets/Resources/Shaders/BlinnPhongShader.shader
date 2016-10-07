Shader "Unlit/BlinnPhongShader"
{
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Light values. Support at most 256 lights
			uniform int _PointLightCount;
			uniform float3 _PointLightColors[256];
			uniform float3 _PointLightPositions[256];
			uniform float2 _PointLightAttenuations[256];

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
				float4 vertex : SV_POSITION;
				float3 worldVertex : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 color : COLOR;
			};

			float3 applyFog(float3 col, float dist) {
				// Calculate fog from distance to camera
				float fogFactor = saturate(exp(-dist * _fogDensity));
				col = lerp(_fogColor.rgb, col, fogFactor);
				return col;
			}

			fixed4 calculateLightColor(vertOut v, float3 normal) {
				// View ray
				float3 V = _WorldSpaceCameraPos - v.worldVertex;
				float dist = length(V);
				V = normalize(V);
				// Calculate ambient light
				float3 ambient = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * _Ka;
				// Sum diffuse and specular light for each source
				float3 sum = float3(0, 0, 0);
				for (int i = 0; i < _PointLightCount; ++i) {
					// Light ray
					float3 L = _PointLightPositions[i] - v.worldVertex;
					float lightDist = length(L);
					L = normalize(L);

					// Calculate attenuation factor from a Gaussian
					float fAtt = saturate(exp(-pow(_PointLightAttenuations[i].x * lightDist, 2)));

					float3 diffuse = fAtt * _PointLightColors[i].rgb
						* _Kd * v.color.rgb * saturate(dot(normalize(L), normal));

					// Approximation to reflected ray
					float3 H = normalize(V + L);
					float3 specular = fAtt * _PointLightColors[i].rgb
						* _Ks * pow(saturate(dot(H, normal)), _N);

					sum += diffuse + specular;
				}
				return fixed4(applyFog(ambient + sum, dist), v.color.a);
			}
			
			vertOut vert(vertIn v)
			{
				vertOut o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				// Convert position to world space
				o.worldVertex = mul(_Object2World, v.vertex);
				// Convert normals to local space. This is taking advantage of the transpose
				// nature of pre vs post multiplication
				o.worldNormal = normalize(mul(v.normal, _World2Object).xyz);
				o.color = _Color;
				return o;
			}
			
			fixed4 frag(vertOut v) : SV_Target
			{
				//return float4(_PointLightAttenuations[0].x, _PointLightAttenuations[0].x, _PointLightAttenuations[0].x, 1);
				// Interpolated normal
				float3 normal = v.worldNormal;

				return calculateLightColor(v, normal);
			}
			ENDCG
		}
	}
}
