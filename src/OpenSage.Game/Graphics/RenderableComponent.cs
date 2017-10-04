using System;
using System.Numerics;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;

namespace OpenSage.Graphics
{
    /// <summary>
    /// Base class for renderable components.
    /// </summary>
    public abstract class RenderableComponent : EntityComponent
    {
        private BoundingBox? _cachedBoundingBox;

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
        public override BoundingBox BoundingBox
        {
            get
            {
                if (_cachedBoundingBox == null)
                {
                    var localBoundingBox = LocalBoundingBox;
                    var localToWorldMatrix = Transform.LocalToWorldMatrix;

                    // TODO: There's probably a better solution than this, to generate a tighter bounding box.
                    BoundingBox transformedBoundingBox;
                    transformedBoundingBox.Min = Vector3.Transform(localBoundingBox.Min, localToWorldMatrix);
                    transformedBoundingBox.Max = Vector3.Transform(localBoundingBox.Max, localToWorldMatrix);

                    var boundingSphere = BoundingSphere.CreateFromBoundingBox(transformedBoundingBox);
                    _cachedBoundingBox = BoundingBox.CreateFromSphere(boundingSphere);
                }
                return _cachedBoundingBox.Value;
            }
        }

        internal abstract BoundingBox LocalBoundingBox { get; }

        internal virtual ModelMesh MeshBase => null;

        protected override void Start()
        {
            base.Start();

            Transform.TransformChanged += OnTransformChanged;
        }

        private void OnTransformChanged(object sender, EventArgs e)
        {
            _cachedBoundingBox = null;
        }

        protected override void Destroy()
        {
            Transform.TransformChanged -= OnTransformChanged;

            base.Destroy();
        }

        internal abstract void BuildRenderList(RenderList renderList);
    }
}
