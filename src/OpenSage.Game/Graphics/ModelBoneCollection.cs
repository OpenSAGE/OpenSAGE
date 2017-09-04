using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenSage.Graphics
{
    public sealed class ModelBoneCollection : ReadOnlyCollection<ModelBone>
    {
        public ModelBoneCollection(IList<ModelBone> list)
            : base(list)
        {
        }
    }
}
