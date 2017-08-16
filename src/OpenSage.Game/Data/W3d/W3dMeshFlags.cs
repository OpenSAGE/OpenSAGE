using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dMeshFlags : uint
    {
        None = 0x00000000, // plain ole normal mesh

        CollisionBox = 0x00000001, // (obsolete as of 4.1) mesh is a collision box (should be 8 verts, should be hidden, etc)
        Skin = 0x00000002, // (obsolete as of 4.1) skin mesh 
        Shadow = 0x00000004, // (obsolete as of 4.1) intended to be projected as a shadow
        Aligned = 0x00000008, // (obsolete as of 4.1) always aligns with camera

        CollisionTypeMask = 0x00000FF0, // mask for the collision type bits
        CollisionTypeShift = 4, // shifting to get to the collision type bits
        CollisionTypePhysical = 0x00000010, // physical collisions
        CollisionTypeProjectile = 0x00000020, // projectiles (rays) collide with this
        CollisionTypeVis = 0x00000040, // vis rays collide with this mesh
        CollisionTypeCamera = 0x00000080, // camera rays/boxes collide with this mesh
        CollisionTypeVehicle = 0x00000100, // vehicles collide with this mesh (and with physical collision meshes)

        Hidden = 0x00001000, // this mesh is hidden by default
        TwoSided = 0x00002000, // render both sides of this mesh
        ObsoleteLightMapped = 0x00004000, // obsolete lightmapped mesh

        // NOTE: retained for backwards compatibility - use W3D_MESH_FLAG_PRELIT_* instead.
        CastShadow = 0x00008000, // this mesh casts shadows

        GeometryTypeMask = 0x00FF0000, // (introduced with 4.1)
        GeometryTypeNormal = 0x00000000, // (4.1+) normal mesh geometry
        GeometryTypeCameraAligned = 0x00010000, // (4.1+) camera aligned mesh
        GeometryTypeSkin = 0x00020000, // (4.1+) skin mesh
        ObsoleteGeometryTypeShadow = 0x00030000, // (4.1+) shadow mesh OBSOLETE!
        GeometryTypeAAbox = 0x00040000, // (4.1+) aabox OBSOLETE!
        GeometryTypeOBBox = 0x00050000, // (4.1+) obbox OBSOLETE!
        GeometryTypeCameraOriented = 0x00060000, // (4.1+) camera oriented mesh (points _towards_ camera)

        PrelitMask = 0x0F000000, // (4.2+) 
        PrelitUnlit = 0x01000000, // mesh contains an unlit material chunk wrapper
        PrelitVertex = 0x02000000, // mesh contains a precalculated vertex-lit material chunk wrapper 

        PrelitLightMapMultiPass = 0x04000000, // mesh contains a precalculated multi-pass lightmapped material chunk wrapper

        PrelitLightMapMultiTexture = 0x08000000, // mesh contains a precalculated multi-texture lightmapped material chunk wrapper

        Shatterable = 0x10000000, // this mesh is shatterable.
        NPatchable = 0x20000000 // it is ok to NPatch this mesh
    }
}
