Shader "Unlit/BlinnPhongTexNormalShader"
{
	SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }
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
			uniform float _PointLightAttenuations[5];

			// Lighting parameters
			uniform float _Ka;		// Ambient albedo
			uniform float _fAtt;	// Attenuation factor
			uniform float _Kd;		// Diffuse albedo
			uniform float _Ks;		// Specular albedo
			uniform float _N;		// Specular exponent

			// Fog parameters
			uniform float4 _fogColor;
			uniform float _fogDensity;

			// Texture parameters
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
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldVertex : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				float3 worldTangent : TANGENT;
				float3 worldBinormal : TEXCOORD3;
				LIGHTING_COORDS(4, 5)
				float4 color : COLOR;
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
				o.worldTangent = normalize(mul(_Object2World, float4(v.tangent, 0)).xyz);
				o.worldBinormal = cross(o.worldNormal, o.worldTangent);
				TRANSFER_VERTEX_TO_FRAGMENT(o);

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = float4(0, 0, 0, 0);
				return o;
			}

			fixed4 frag(vertOut v) : SV_Target
			{
				// Encoded normal; shift from 0-1 to +/- 1
				float3 normal = 2 * (tex2D(_NormalMap, v.uv).xyz) - 1;
				// Unity has backwards normals?
				normal.g *= -1;
				v.color = tex2D(_MainTex, v.uv);
				// Create the matrix to transform from the surface basis to
				// world basis
				normal = mul(normal, float3x3(v.worldTangent,
									 v.worldBinormal,
									 v.worldNormal));
				
				return calculateLightColor(v, normal);
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}