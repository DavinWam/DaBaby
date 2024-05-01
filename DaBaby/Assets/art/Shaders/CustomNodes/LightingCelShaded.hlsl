#ifndef LightingCelShaded
#define LightingCelShaded

#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
 
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"



TEXTURE2D(_ShadowMapTexture);
SAMPLER(sampler_ShadowMapTexture);

float3 accumulatedDiffuse;

#ifndef SHADERGRAPH_PREVIEW
struct EdgeContstants
{
    float diffuse; 
    float specular;
    float specularOffset; 
    float distanceAttenuation; 
    float shadowAttenuation; 
    float rim; 
    float rimOffset; 

};
    struct SurfaceVariables
    {
    float3 normals;
    float3 view;
    float smoothness;
    float shininess;
    float rimThreshold;
    EdgeContstants ec;
    float3 baseColor;
    float3 shadowColor;
    float4 shadowCoord;
};
    
    float3 CalculateCelShading(Light l, SurfaceVariables s)
    {

        float shadowAttenuationSmooth = smoothstep(0,s.ec.shadowAttenuation,l.shadowAttenuation);
        float distanceAttenuationSmooth = smoothstep(0,s.ec.distanceAttenuation,l.distanceAttenuation);
        
        float attenuation = shadowAttenuationSmooth * distanceAttenuationSmooth;
        //diffuse
        float diffuse = saturate(dot(s.normals, l.direction));
        diffuse *= attenuation;


        //specular(blinn-phong reflection)
        float3 h = normalize(l.direction + s.view);
        float specular = saturate(dot(s.normals, h));
        specular = pow(specular, s.shininess);
    
        //prevent highlights from being in shadow
        specular *= diffuse * s.smoothness;
        
        //rim reflection
        float rim = 1 - dot(s.view, s.normals);
        rim *= pow(diffuse, s.rimThreshold);

        //smoothing values
        diffuse = smoothstep(0,s.ec.diffuse,diffuse);
        specular = s.smoothness * smoothstep((1-s.smoothness)*s.ec.specular+s.ec.specularOffset, 
            s.ec.specular+s.ec.specularOffset, specular);
        rim = s.smoothness * smoothstep(s.ec.rim -.5f * s.ec.rimOffset,s.ec.rim + .5f * s.ec.rimOffset, rim );
        
        float3 diffuseColor = lerp(s.shadowColor, s.baseColor, diffuse);

        accumulatedDiffuse += diffuse* l.color;

        //return l.color * diffuseColor + l.color * max(specular, rim * s.baseColor);
        return s.baseColor * l.color * diffuse + lerp(s.baseColor, l.color, max(specular, rim ));

    }
#endif

void LightingCelShaded_float(float Smooothness, float Shininess, float3 Position, float RimTrheshold,float3 Normal, float3 View,  float EdgeDiffuse, float EdgeSpecular,
    float EdgeSpecularOffset, float EdgeDistanceAttenuation, float EdgeShadowAttenuation, float EdgeRim, float EdgeRimOffset,float3 BaseColor, float3 ShadowColor, out float3 Color)
{
    #if SHADERGRAPH_PREVIEW
            Color = float3(1, 0, 1);
    #else
        //initialize s
        SurfaceVariables s;
        s.normals = normalize(Normal);
        s.view = normalize(View);
        s.smoothness = Smooothness;
        s.shininess = exp2(10 * Shininess + 1);
        s.rimThreshold = RimTrheshold;
        s.baseColor = BaseColor;
        s.shadowColor = ShadowColor;
        
        //initialize edge constants
        EdgeContstants edgeContstants;
        edgeContstants.diffuse = EdgeDiffuse;
        edgeContstants.specular = EdgeSpecular;
        edgeContstants.specularOffset = EdgeSpecularOffset;
        edgeContstants.distanceAttenuation = EdgeDistanceAttenuation;
        edgeContstants.shadowAttenuation = EdgeShadowAttenuation;
        edgeContstants.rim = EdgeRim;
        edgeContstants.rimOffset = EdgeRimOffset;
        s.ec = edgeContstants;
        
        //shadowing based on position
        #if SHADOWS_SCREEN
            float4 clipPos = TransformWorldToHClip(Position);
            s.shadowCoord = ComputeScreenPos(clipPos);
        #else
            s.shadowCoord = TransformWorldToShadowCoord(Position);
        #endif
       
       //mainlight
        Light light = GetMainLight(s.shadowCoord);
       Color = CalculateCelShading(light, s);

       //other scene lights
       int pixelLightCount = GetAdditionalLightsCount();
       for (int i = 0; i < pixelLightCount; i++){

        // the '1' doesn't do anything, the input is "supposedly " for baking, 
        // the '1' however is just me putting in a "default value"(in quotes because I'm not technically sure if it does something),
        // the function(and all the used ones not found in the file) can be found in RealtimeLights.hlsl, in unity's package folder
        light = GetAdditionalLight(i,Position,1);
        Color += CalculateCelShading(light, s);
       }
       accumulatedDiffuse = clamp(accumulatedDiffuse, 0.0, 1.0);
       //Color = accumulatedDiffuse;

       float3 diffuseColor = lerp(ShadowColor, Color, accumulatedDiffuse);
       Color = diffuseColor;

    #endif 
}

#endif