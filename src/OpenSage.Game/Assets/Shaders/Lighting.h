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

        diffuseColor += ambient + (diffuse * light.Color);
    }

    diffuseColor = saturate(diffuseColor);
}