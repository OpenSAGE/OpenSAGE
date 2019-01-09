using System;
using System.Numerics;
using ImGuiNET;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Settings;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class ModelView : AssetView
    {
        private readonly RenderedView _renderedView;
        private readonly ModelInstance _modelInstance;

        public ModelView(DiagnosticViewContext context, Model model)
            : base(context)
        {
            _modelInstance = AddDisposable(model.CreateInstance(context.Game.GraphicsDevice));
            _modelInstance.Update(new GameTime());

            var enclosingBoundingBox = GetEnclosingBoundingBox(_modelInstance);

            var cameraController = new ArcballCameraController(
                enclosingBoundingBox.GetCenter(),
                Vector3.Distance(enclosingBoundingBox.Min, enclosingBoundingBox.Max));

            var scene3D = AddDisposable(new Scene3D(
                context.Game,
                () => new Veldrid.Viewport(0, 0, ImGui.GetContentRegionAvailWidth(), ImGui.GetContentRegionAvail().Y, 0, 1),
                cameraController,
                null,
                null,
                Array.Empty<Terrain.WaterArea>(),
                Array.Empty<Terrain.Road>(),
                Array.Empty<Terrain.Bridge>(),
                null,
                new GameObjectCollection(context.Game.ContentManager),
                new WaypointCollection(),
                new WaypointPathCollection(),
                WorldLighting.CreateDefault(),
                Array.Empty<Player>(),
                Array.Empty<Team>()));

            _renderedView = AddDisposable(new RenderedView(context, scene3D));

            void OnBuildingRenderList(object sender, BuildingRenderListEventArgs e)
            {
                _modelInstance.SetWorldMatrix(Matrix4x4.Identity);
                _modelInstance.BuildRenderList(e.RenderList, e.Camera, true, context.Game.CivilianPlayer);
            }

            _renderedView.RenderPipeline.BuildingRenderList += OnBuildingRenderList;
            AddDisposeAction(() => _renderedView.RenderPipeline.BuildingRenderList -= OnBuildingRenderList);
        }

        private static BoundingBox GetEnclosingBoundingBox(ModelInstance modelInstance)
        {
            var boundingBox = default(BoundingBox);

            var first = true;
            foreach (var subObject in modelInstance.Model.SubObjects)
            {
                var transformedBoundingBox = BoundingBox.Transform(subObject.RenderObject.BoundingBox,
                    modelInstance.ModelBoneInstances[subObject.Bone.Index].Matrix);

                if (first)
                {
                    boundingBox = transformedBoundingBox;
                    first = false;
                }
                else
                {
                    boundingBox = BoundingBox.CreateMerged(
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
