Shader "Hidden/Battle/FogOfWar"
{
	HLSLINCLUDE

	#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

	TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
	TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
	TEXTURE3D_SAMPLER3D(_Vision, sampler_Vision);
	TEXTURE2D_SAMPLER2D(_Blur, sampler_Blur);

	float3 _WorldSize;
	float4x4 unity_ViewToWorldMatrix;
	float4x4 unity_InverseProjectionMatrix;

	struct VertexInput 
	{
		float4 vertex : POSITION;
	};

	struct VertexOutput 
	{
		float4 pos : SV_POSITION;
		float2 screenPos : TEXCOORD0;
	};

	float3 GetWorldFromViewPosition(VertexOutput i) 
	{
		// get view space position
		float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.screenPos).r;
		float4 result = mul(unity_InverseProjectionMatrix, float4(2.0 * i.screenPos - 1.0, z, 1.0));
		float3 viewPos = result.xyz / result.w;

		// get ws position
		float3 worldPos = mul(unity_ViewToWorldMatrix, float4(viewPos, 1.0));
		return worldPos;
	}

	VertexOutput Vertex(VertexInput i) 
	{
		VertexOutput o;
		o.pos = float4(i.vertex.xy, 0.0, 1.0);

		// get clip space coordinates for sampling camera tex
		o.screenPos = TransformTriangleVertexToUV(i.vertex.xy);
		#if UNITY_UV_STARTS_AT_TOP
		o.screenPos = o.screenPos * float2(1.0, -1.0) + float2(0.0, 1.0);
		#endif

		return o;
	}

	float4 Frag(VertexOutput i) : SV_Target
	{	
		// world position & uv
		float3 worldPos = GetWorldFromViewPosition(i);
		float3 vPixelSize = float3(0.5f / _WorldSize.x, 0.5f / _WorldSize.y, 0.5f / _WorldSize.z);
		float3 uvVision = float3(worldPos.x / _WorldSize.x + vPixelSize.x, worldPos.y / _WorldSize.y + vPixelSize.y, worldPos.z / _WorldSize.z + vPixelSize.z);

		// sample textures
		float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.screenPos);
		float4 blur = SAMPLE_TEXTURE2D(_Blur, sampler_Blur, i.screenPos);
		float4 vision = SAMPLE_TEXTURE3D(_Vision, sampler_Vision, uvVision);

		// blur amount
		color = lerp(blur, color, vision.g);
		
		// grayscale
		float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750)) * 0.5f * vision.r;

		// compose
		color.rgb = lerp(luminance.xxx, color.rgb, vision.g);
		return color;
	}

	ENDHLSL

	SubShader
	{
		Cull Off 
		ZWrite Off 
		ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vertex
			#pragma fragment Frag
			ENDHLSL
		}
	}
}