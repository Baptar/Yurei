#define UNIVERSAL_LIGHTING_INCLUDED 1
#define SHADERGRAPH_PREVIEW 0
#include "shaderMarchePas/Core.hlsl"
#include "shaderMarchePas/Lighting.hlsl"

void MainLight_float(float3 _WorldPos, out float3 _Direction, out float3 _Color, out float _DistanceAtten, out float _ShadowAtten)
{
	
    
#if SHADERGRAPH_PREVIEW
	_Direction = float3(0.5, 0.5, 0);
	_Color = 1;
	_DistanceAtten = 1;
	_ShadowAtten = 1;

#else
	Light mainLight = GetMainLight();
	_Direction = mainLight.direction;
    _Color = mainLight.color;
    _DistanceAtten = mainLight.distanceAttenuation;

    float4 shadowCoord = TransformWorldToShadowCoord(_WorldPos);
	ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
	half shadowStrength = GetMainLightShadowStrength();
    _ShadowAtten = SampleShadowmap(shadowCoord, TEXTURE2D_ARGS(_MainLightShadowmapTexture,
sampler_MainLightSadowmapTexture), shadowSamplingData, shadowStrength, fmase);


#endif
}


/*
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
/* IN(5): SpecColor(4), Smoothness(1), WPos(3), WNormal(3), WView(3) */
/* OUT(2): Diffuse(3), Specular(3) NdotL(1) for toon ramp: point,clamp) */
/*void CalculateLights_half(half4 SpecColor, half Smoothness, half3 WPos, half3 WNormal, half3 WView,
                          out half3 Diffuse, out half3 Specular, out half NdotL)
{
    Diffuse = 0;
    Specular = 0;
    NdotL = 0;
#ifndef SHADERGRAPH_PREVIEW
    Smoothness = exp2(10 * Smoothness + 1); // WNormal = normalize(WNormal);    WView = SafeNormalize(WView);        
                                                        
    Light light = GetMainLight(); // Main Pixel Light
    half3 attenCol = light.color * light.distanceAttenuation * light.shadowAttenuation;
    Diffuse = LightingLambert(attenCol, light.direction, WNormal); /* LAMBERT */
   /* Specular = LightingSpecular(attenCol, light.direction, WNormal, WView, SpecColor, Smoothness); /*Blinn-Phong*/
    /*NdotL = (saturate(dot(light.direction, WNormal)) + 1) / 2; // NdotL [-1..1] normalized [0..1] for toon ramp
#endif
}

#endif

*/