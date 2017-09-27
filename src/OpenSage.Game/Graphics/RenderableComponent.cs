using System.Linq;
using LLGfx;

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

        /// <summary>
        /// Gets or sets the materials to use to render this mesh. If the mesh being rendered consists
        /// only of a single submesh, then you can use the <see cref="Material"/> property instead. Otherwise,
        /// the number of materials set here must equal the number of submeshes.
        /// </summary>
        //public Material[] Materials { get; set; }

        /// <summary>
        /// Gets or sets the material to use to render this mesh. Setting this property has the same effect
        /// as setting <see cref="Materials"/> to an array with one value.
        /// </summary>
        //[ContentSerializerIgnore]
        //public Material Material
        //{
        //    get { return (Materials != null && Materials.Length > 0) ? Materials[0] : null; }
        //    set
        //    {
        //        Materials = new Material[1];
        //        Materials[0] = value;
        //    }
        //}

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

        protected internal abstract void Render(CommandEncoder commandEncoder);
    }
}
