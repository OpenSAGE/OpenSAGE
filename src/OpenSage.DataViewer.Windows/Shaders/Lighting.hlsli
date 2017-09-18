struct Light
{
    float3 Ambient;
    float3 Color;
    float3 Direction;
};

#define NUM_LIGHTS 3

struct LightingConstants
{
    float3 CameraPosition;
    Light Lights[NUM_LIGHTS];
};

ConstantBuffer<LightingConstants> LightingCB : register(LIGHTING_CB_REGISTER);

struct LightingParameters
{
    float3 WorldPosition;
    float3 WorldNormal;
    float3 MaterialAmbient;
    float3 MaterialDiffuse;
    float3 MaterialSpecular;
    float MaterialShininess;
};

float3 CalculateViewVector(float3 worldPosition)
{
    return normalize(LightingCB.CameraPosition - worldPosition);
}

void DoLighting(
    LightingParameters params,
    out float3 diffuseColor,
    out float3 specularColor)
{
    diffuseColor = float3(0, 0, 0);
    specularColor = float3(0, 0, 0);

    for (int i = 0; i < NUM_LIGHTS; i++)
    {
        Light light = LightingCB.Lights[i];

        float3 ambient = light.Ambient * params.MaterialAmbient;

        float3 diffuse = saturate(dot(params.WorldNormal, -light.Direction)) * params.MaterialDiffuse;

        #if defined(SPECULAR_ENABLED)
        float3 v = CalculateViewVector(params.WorldPosition);
        float3 h = normalize(v - light.Direction);
        specularColor += saturate(dot(params.WorldNormal, h)) 
            * params.MaterialShininess 
            * params.MaterialSpecular 
            * light.Color;
        #endif

        diffuseColor += ambient + (diffuse * light.Color);
    }

    diffuseColor = saturate(diffuseColor);
}