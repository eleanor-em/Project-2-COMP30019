// Adapted from http://stackoverflow.com/questions/35422692/how-to-make-an-object-to-glow-in-unity3d
Shader "Custom/GlowShader" {
	Properties {
		_Color ("Color Tint", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_RimColor ("Rim Colour", Color) = (1, 1, 1, 1)
		_RimPower ("Rim Power", Range(1.0, 6.0)) = 3.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf BlinnPhong

		struct Input {
			float4 color : Color;
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 viewDir;
		};

		fixed4 _Color;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));

			// Inverse scale based on distance to edge from view direction
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			// Create a glow around the edge
			o.Emission = _RimColor.rgb * pow(rim, 7.0 - _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
