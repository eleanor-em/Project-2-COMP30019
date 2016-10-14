Shader "Unlit/BlinnPhongTexShader"
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

			// Light values. Support at most 8 lights
			uniform int _PointLightCount;
			uniform float3 _PointLightColors[8];
			uniform float3 _PointLightPositions[8];
			uniform float _PointLightAttenuations[8];

			// Lighting parameters
			uniform float _Ka;		// Ambient albedo
			uniform float _fAtt;	// Attenuation factor
			uniform float _Kd;		// Diffuse albedo
			uniform float _Ks;		// Specular albedo
			uniform float _N;		// Specular exponent

			// Fog parameters
			uniform float4 _fogColor;
			uniform float _fogDensity;

			uniform sampler2D _MainTex;
			float4 _MainTex_ST;

			struct vertIn {
				float4 vertex : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct vertOut {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldVertex : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				LIGHTING_COORDS(3, 4)
				float4 color : COLOR;
			};

			float3 applyFog(float3 col, float dist) {
				// Calculate fog from distance to camera
				float fogFactor = saturate(exp(-dist * _fogDensity));
				col = lerp(_fogColor.rgb, col, fogFactor);
				return col;
			}

			fixed4 calculateLightColor(vertOut v, float3 normal) {
				// Shadows
				fixed shadow = LIGHT_ATTENUATION(v);
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

					float3 diffuse = shadow * fAtt * _PointLightColors[i].rgb
						* _Kd * v.color.rgb * saturate(dot(L, normal));

					// Approximation to reflected ray
					float3 H = normalize(V + L);
					float3 specular = fAtt * _PointLightColors[i].rgb
						* _Ks * pow(saturate(dot(H, normal)), _N);

					sum += diffuse + specular;
				}
				return fixed4(applyFog(ambient + sum, dist), shadow * v.color.a);
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
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = float4(0, 0, 0, 0);
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}

			fixed4 frag(vertOut v) : SV_Target
			{
				v.color = tex2D(_MainTex, v.uv);
				return calculateLightColor(v, v.worldNormal);
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}