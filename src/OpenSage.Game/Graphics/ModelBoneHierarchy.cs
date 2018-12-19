namespace OpenSage.Graphics
{
    public sealed class ModelBoneHierarchy
    {
        public ModelBone[] Bones { get; }

        internal ModelBoneHierarchy(ModelBone[] bones)
        {
            Bones = bones;
        }
    }
}
