using System.Numerics;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics.AssetViews
{
    [AssetView(typeof(Model))]
    internal sealed class ModelView : AssetView
    {
        private readonly RenderedView _renderedView;
        private readonly ModelInstance _modelInstance;

        public ModelView(DiagnosticViewContext context, Model model)
            : base(context)
        {
            _modelInstance = AddDisposable(model.CreateInstance(context.Game.AssetStore.LoadContext));
            _modelInstance.Update(TimeInterval.Zero);

            var enclosingBoundingBox = GetEnclosingBoundingBox(_modelInstance);

            _renderedView = AddDisposable(new RenderedView(
                context,
                enclosingBoundingBox.GetCenter(),
                Vector3.Distance(enclosingBoundingBox.Min, enclosingBoundingBox.Max)));

            void OnBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                _modelInstance.SetWorldMatrix(Matrix4x4.Identity);
                _modelInstance.BuildRenderList(
                    e.RenderList,
                    e.Camera,
                    true,
                    new MeshShaderResources.RenderItemConstantsPS
                    {
                        HouseColor = context.Game.CivilianPlayer.Color.ToVector3(),
                        Opacity = 1.0f
                    });
            }

            _renderedView.RenderPipeline.BuildingRenderList += OnBuildingRenderList;
            AddDisposeAction(() => _renderedView.RenderPipeline.BuildingRenderList -= OnBuildingRenderList);
        }

        private static AxisAlignedBoundingBox GetEnclosingBoundingBox(ModelInstance modelInstance)
        {
            var boundingBox = default(AxisAlignedBoundingBox);

            var first = true;
            foreach (var subObject in modelInstance.Model.SubObjects)
            {
                var transformedBoundingBox = AxisAlignedBoundingBox.Transform(subObject.RenderObject.BoundingBox,
                    modelInstance.ModelBoneInstances[subObject.Bone.Index].Matrix);

                if (first)
                {
                    boundingBox = transformedBoundingBox;
                    first = false;
                }
                else
                {
                    boundingBox = AxisAlignedBoundingBox.CreateMerged(
                        boundingBox,
                        transformedBoundingBox);
                }
            }

            return boundingBox;
        }

        public override void Draw()
        {
            _renderedView.Draw();
        }
    }
}
