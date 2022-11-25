using System;
using System.Collections.Generic;
using System.Reflection;
using ImGuiNET;
using OpenSage.Diagnostics.AssetViews;

namespace OpenSage.Diagnostics
{
    internal sealed class PreviewView : DiagnosticView
    {
        private static readonly Dictionary<Type, ConstructorInfo> AssetViewConstructors;

        static PreviewView()
        {
            AssetViewConstructors = new Dictionary<Type, ConstructorInfo>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                var assetViewAttribute = type.GetCustomAttribute<AssetViewAttribute>();
                if (assetViewAttribute != null)
                {
                    var constructorParameterTypes = new[]
                    {
                        typeof(DiagnosticViewContext),
                        assetViewAttribute.ForType
                    };
                    AssetViewConstructors.Add(assetViewAttribute.ForType, type.GetConstructor(constructorParameterTypes));
                }
            }
        }

        private object _selectedObject;
        private AssetView _currentAssetView;

        public PreviewView(DiagnosticViewContext context)
            : base(context)
        {

        }

        public override string DisplayName { get; } = "Preview";

        protected override void DrawOverride(ref bool isGameViewFocused)
        {
            if (Context.SelectedObject != _selectedObject)
            {
                RemoveAndDispose(ref _currentAssetView);

                var assetViewConstructor = AssetViewConstructors.GetValueOrDefault(Context.SelectedObject.GetType());

                // If there's no AssetView for this type, search with the base type.
                if (assetViewConstructor == null) {
                    var baseType = Context.SelectedObject.GetType().BaseType;
                    while (baseType != null && assetViewConstructor == null) {
                        assetViewConstructor = AssetViewConstructors.GetValueOrDefault(baseType);
                        baseType = baseType.BaseType;
                    }
                }

                if (assetViewConstructor != null)
                {
                    AssetView createAssetView() => (AssetView) assetViewConstructor.Invoke(new object[] { Context, Context.SelectedObject });

                    _currentAssetView = AddDisposable(createAssetView());
                }

                _selectedObject = Context.SelectedObject;
            }

            if (_currentAssetView != null)
            {
                _currentAssetView.Draw();
            }
            else
            {
                ImGui.Text("No previewable object selected");
            }
        }
    }
}
