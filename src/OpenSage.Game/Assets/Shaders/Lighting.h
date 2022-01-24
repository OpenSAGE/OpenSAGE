#ifndef LIGHTING_H

#define LIGHTING_H

struct GlobalLightingConstantsVSType
{
    mat4 CloudShadowMatrix;
};

struct Light
{
    vec3 Ambient;
    float _Padding1;
    vec3 Color;
    float _Padding2;
    vec3 Direction;
    float _Padding3;
};

#define NUM_LIGHTS 3

struct LightingConfiguration
{
    Light[NUM_LIGHTS] Lights;
};

struct GlobalLightingConstantsPSType
{
    LightingConfiguration Terrain;
    LightingConfiguration Object;
};

layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 0) uniform GlobalLightingConstantsVS
{
    GlobalLightingConstantsVSType _GlobalLightingConstantsVS;
};

layout(set = PASS_CONSTANTS_RESOURCE_SET, binding = 1) uniform GlobalLightingConstantsPS
{
    GlobalLightingConstantsPSType _GlobalLightingConstantsPS;
};

vec3 CalculateViewVector(vec3 worldPosition)
{
    return normalize(_GlobalConstants.CameraPosition - worldPosition);
}

void DoLighting(
    LightingConfiguration lightingConfiguration,
    vec3 worldPosition,
    vec3 worldNormal,
    vec3 materialAmbient,
    vec3 materialDiffuse,
    vec3 materialSpecular,
    float materialShininess,
    bool specularEnabled,
    vec3 shadowVisibility,
    out vec3 diffuseColor,
    out vec3 specularColor)
{
    diffuseColor = vec3(0, 0, 0);
    specularColor = vec3(0, 0, 0);

    for (int i = 0; i < NUM_LIGHTS; i++)
    {
        Light light = lightingConfiguration.Lights[i];

        vec3 ambient = light.Ambient * materialAmbient;

        vec3 diffuse =
            saturate(dot(worldNormal, -light.Direction)) *
            materialDiffuse;

        if (specularEnabled)
        {
            vec3 v = CalculateViewVector(worldPosition);
            vec3 h = normalize(v - light.Direction);
            specularColor +=
                saturate(dot(worldNormal, h)) *
                materialShininess *
                materialSpecular *
                light.Color;
        }

        vec3 diffuseContribution = diffuse * light.Color;

        if (i == 0)
        {
            diffuseContribution *= shadowVisibility;
        }

        diffuseColor += ambient + diffuseContribution;
    }

    diffuseColor = saturate(diffuseColor);
}

#endif