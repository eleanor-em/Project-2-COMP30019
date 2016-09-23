Shader "Unlit/BlinnPhongTexNormalShader"
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

			uniform sampler2D _MainTex;
			uniform sampler2D _NormalMap;
			float4 _MainTex_ST;

			struct vertIn {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
				float3 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct vertOut {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldVertex : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float3 worldTangent : TANGENT;
				float3 worldBinormal : TEXCOORD3;
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
				o.worldTangent = normalize(mul(_Object2World, float4(v.tangent, 0)).xyz);
				o.worldBinormal = cross(o.worldNormal, o.worldTangent);

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = float4(0, 0, 0, 0);
				return o;
			}

			fixed4 frag(vertOut v) : SV_Target
			{
				// Encoded normal; shift from 0-1 to +/- 1
				float3 encodedNormal = 2 * (tex2D(_NormalMap, v.uv).xyz) - 1;
				encodedNormal.r *= -1;
				encodedNormal.g *= -1;
				v.color = tex2D(_MainTex, v.uv);
				// Create the matrix to transform from the surface basis to
				// world basis
				float3x3 surfaceToWorld = float3x3(v.worldTangent,
												   v.worldBinormal,
												   v.worldNormal);
				float3 normal = mul(encodedNormal, surfaceToWorld);

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
