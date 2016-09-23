Shader "Unlit/BlinnPhongShader"
{
	SubShader
	{
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

			// Lighting parameters
			uniform float _Ka;		// Ambient albedo
			uniform float _fAtt;	// Attenuation factor
			uniform float _Kd;		// Diffuse albedo
			uniform float _Ks;		// Specular albedo
			uniform float _N;		// Specular exponent

			struct vertIn {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
			};

			struct vertOut {
				float4 vertex : SV_POSITION;
				float3 worldVertex : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
				float4 color : COLOR;
			};
			
			vertOut vert(vertIn v)
			{
				vertOut o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				// Convert position to world space
				o.worldVertex = mul(_Object2World, v.vertex);
				// Convert normals to local space. This is taking advantage of the transpose
				// nature of pre vs post multiplication
				o.worldNormal = normalize(mul(v.normal, _World2Object).xyz);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (vertOut v) : SV_Target
			{
				// Interpolated normal
				float3 normal = v.worldNormal;

				// Calculate ambient light
				float3 ambient = v.color.rgb * UNITY_LIGHTMODEL_AMBIENT.rgb * _Ka;
				
				// Sum diffuse and specular light for each source
				float3 sum = float3(0, 0, 0);
				for (int i = 0; i < _PointLightCount; ++i) {
					// Light ray
					float3 L = normalize(_PointLightPositions[i] - v.worldVertex);
					float3 diffuse = _fAtt * _PointLightColors[i].rgb
									 * _Kd * v.color.rgb * saturate(dot(L, normal));

					// View ray
					float3 V = normalize(_WorldSpaceCameraPos - v.worldVertex);
					// Approximation to reflected ray
					float3 H = normalize(V + L);
					float3 specular = _fAtt * _PointLightColors[i].rgb
									   * _Ks * pow(saturate(dot(H, normal)), _N);

					sum += diffuse + specular;
				}
				fixed4 col = fixed4(ambient + sum, v.color.a);
				return col;
			}
			ENDCG
		}
	}
}
