using LL.Graphics3D;
using OpenSage.Graphics.Animation;

namespace OpenSage.Graphics
{
    public sealed class Model : GraphicsObject
    {
        public ModelBone[] Bones { get; }
        public ModelMesh[] Meshes { get; }
        public Animation.Animation[] Animations { get; }

        internal Model(
            ModelBone[] bones,
            ModelMesh[] meshes,
            Animation.Animation[] animations)
        {
            Bones = bones;

            foreach (var mesh in meshes)
            {
                AddDisposable(mesh);
            }
            Meshes = meshes;

            Animations = animations;
        }

        public Entity CreateEntity()
        {
            var result = new Entity();

            var boneTransforms = new TransformComponent[Bones.Length];
            for (var i = 0; i < Bones.Length; i++)
            {
                var bone = Bones[i];

                var parentTransform = bone.Parent != null
                    ? boneTransforms[bone.Parent.Index]
                    : result.Transform;

                var boneEntity = new Entity();
                boneEntity.Name = bone.Name + " Animation Offset Parent";
                boneEntity.Transform.LocalPosition = bone.Translation;
                boneEntity.Transform.LocalRotation = bone.Rotation;

                parentTransform.Children.Add(boneEntity.Transform);

                var animatedBoneEntity = new Entity();
                animatedBoneEntity.Name = bone.Name;
                boneEntity.AddChild(animatedBoneEntity);

                boneTransforms[i] = animatedBoneEntity.Transform;
            }

            result.Components.Add(new ModelComponent
            {
                Bones = boneTransforms
            });

            foreach (var mesh in Meshes)
            {
                var boneEntity = boneTransforms[mesh.ParentBone.Index].Entity;

                if (mesh.Skinned)
                {
                    // Add skinned mesh component to root model entity,
                    // not bone entity.
                    result.Components.Add(new SkinnedMeshComponent
                    {
                        Mesh = mesh
                    });
                }
                else
                {
                    boneEntity.Components.Add(new MeshComponent
                    {
                        Mesh = mesh
                    });
                }
            }

            foreach (var animation in Animations)
            {
                result.Components.Add(new AnimationComponent
                {
                    Animation = animation
                });
            }

            return result;
        }
    }
}
