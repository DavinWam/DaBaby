#ifndef LightingCelShaded
#define LightingCelShaded

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl"

#ifndef SHADERGRAPH_PREVIEW

    struct SurfaceVariables
    {
    float3 normals;
    float3 view;
    float smoothness;
    float shininess;
    float rimThreshold;
};
    
    float3 CalculateCelShading(Light l, SurfaceVariables s)
    {
        float diffuse = saturate(dot(s.normals, l.direction));
    
        float3 h = normalize(l.direction + s.view);
        float specular = saturate(dot(s.normals, h));
        specular = pow(specular, s.shininess);
        //prevent highlights from being in shadow
        specular *= diffuse * s.smoothness;
        
        float rim = 1 - dot(s.view, s.normals);
        rim *= pow(diffuse, s.rimThreshold);
        return l.color * (diffuse + max(specular, rim));
}
#endif

void LightingCelShaded_float(float Smooothness, float Shininess,float RimTrheshold, float3 Normal,float3 View, out float3 Color)
{
    #if defined(SHADERGRAPH_PREVIEW)
            Color = float3(1, 0, 1);
    #else
        SurfaceVariables s;
        s.normals = normalize(Normal);
        s.view = normalize(View);
        s.smoothness = Smooothness;
        s.shininess = exp2(10 * Shininess + 1);
        s.rimThreshold = RimTrheshold;
        Light light = GetMainLight();
    
        //Color = float3(.5f, .5f, .5f);
        Color = CalculateCelShading(light, s);
    #endif 
}
#endif