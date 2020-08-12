#ifndef RADIUS_CURSOR_DECALS_FUNCTIONS_H

#define RADIUS_CURSOR_DECALS_FUNCTIONS_H

vec3 GetRadiusCursorDecalColor(vec3 worldPosition)
{
    vec3 result = vec3(0, 0, 0);

    for (int i = 0; i < _RadiusCursorDecalConstants.NumRadiusCursorDecals; i++)
    {
        // Can't do this because SPIRV-Cross doesn't support it yet:
        // RadiusCursorDecal decal = _RadiusCursorDecals[i];

        uint decalTextureIndex = _RadiusCursorDecals[i].DecalTextureIndex;

        vec2 decalBottomLeftPosition = _RadiusCursorDecals[i].BottomLeftCornerPosition;
        float decalDiameter = _RadiusCursorDecals[i].Diameter;

        // TODO: Opacity

        float decalU = (worldPosition.x - decalBottomLeftPosition.x) / decalDiameter;
        float decalV = (worldPosition.y - decalBottomLeftPosition.y) / decalDiameter;

        vec2 decalUV = vec2(decalU, 1 - decalV);

        vec4 decalColor = texture(
            sampler2DArray(RadiusCursorDecalTextures, RadiusCursorDecalSampler),
            vec3(decalUV, decalTextureIndex));

        result += decalColor.xyz * decalColor.a;
    }

    return result;
}

#endif