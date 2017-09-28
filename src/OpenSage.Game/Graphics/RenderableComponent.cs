using System.Collections.Generic;
using System.Linq;
using LLGfx;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    /// <summary>
    /// Base class for renderable components.
    /// </summary>
    public abstract class RenderableComponent : EntityComponent
    {
        /// <summary>
        /// Gets or sets whether this renderable component casts shadows on other objects in the scene.
        /// </summary>
        public bool CastsShadows { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this renderable component receives shadows cast from other objects in the scene.
        /// </summary>
        public bool ReceivesShadows { get; set; } = true;

        internal virtual bool IsAlwaysVisible => false;

        /// <inheritdoc />
        //public override BoundingBox BoundingBox
        //{
        //    get
        //    {
        //        // TODO: Don't create this every time.

        //        var localBoundingBox = LocalBoundingBox;
        //        var localToWorldMatrix = Transform.LocalToWorldMatrix;

        //        // TODO: There's probably a better solution than this.
        //        BoundingBox transformedBoundingBox;
        //        transformedBoundingBox.Min = Vector3.Transform(localBoundingBox.Min, localToWorldMatrix);
        //        transformedBoundingBox.Max = Vector3.Transform(localBoundingBox.Max, localToWorldMatrix);

        //        var boundingSphere = BoundingSphere.CreateFromBoundingBox(transformedBoundingBox);
        //        return BoundingBox.CreateFromSphere(boundingSphere);
        //    }
        //}

        //internal abstract BoundingBox LocalBoundingBox { get; }

        //[ContentSerializerIgnore]
        //internal abstract MeshBase MeshBase { get; }

        internal abstract void BuildRenderList(RenderList renderList);
    }
}
