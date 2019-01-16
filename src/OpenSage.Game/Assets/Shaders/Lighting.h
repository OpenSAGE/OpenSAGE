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

struct GlobalLightingConstantsPSType
{
    Light[NUM_LIGHTS] Lights;
};

#define MAKE_GLOBAL_LIGHTING_CONSTANTS_RESOURCES_VS(resourceSet) \
    layout(set = resourceSet, binding = 0) uniform GlobalLightingConstantsVS \
    { \
        GlobalLightingConstantsVSType _GlobalLightingConstantsVS; \
    };

#define MAKE_GLOBAL_LIGHTING_CONSTANTS_RESOURCES_PS(resourceSet) \
    layout(set = resourceSet, binding = 1) uniform GlobalLightingConstantsPS \
    { \
        GlobalLightingConstantsPSType _GlobalLightingConstantsPS; \
    };

vec3 CalculateViewVector(vec3 cameraPosition, vec3 worldPosition)
{
    return normalize(cameraPosition - worldPosition);
}

void DoLighting(
    GlobalLightingConstantsPSType lightingConstantsPS,
    vec3 worldPosition,
    vec3 worldNormal,
    vec3 materialAmbient,
    vec3 materialDiffuse,
    vec3 materialSpecular,
    float materialShininess,
    vec3 cameraPosition,
    bool specularEnabled,
    vec3 shadowVisibility,
    out vec3 diffuseColor,
    out vec3 specularColor)
{
    diffuseColor = vec3(0, 0, 0);
    specularColor = vec3(0, 0, 0);

    for (int i = 0; i < NUM_LIGHTS; i++)
    {
        Light light = lightingConstantsPS.Lights[i];

        vec3 ambient = light.Ambient * materialAmbient;

        vec3 diffuse =
            saturate(dot(worldNormal, -light.Direction)) *
            materialDiffuse;

        if (specularEnabled)
        {
            vec3 v = CalculateViewVector(cameraPosition, worldPosition);
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