using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class DefaultAssetView : AssetView
    {
        private readonly BaseAsset _asset;
        private readonly PropertyInfo[] _propertyInfos;

        public DefaultAssetView(DiagnosticViewContext context, BaseAsset asset)
            : base(context)
        {
            _asset = asset;
            _propertyInfos = asset.GetType().GetProperties();
        }

        public override void Draw()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, 2));
            ImGui.Columns(2);
            ImGui.Separator();

            DrawObject(_asset);

            ImGui.Columns(1);
            ImGui.Separator();
            ImGui.PopStyleVar();
            ImGui.End();
        }

        private static void DrawObject(object instance)
        {
            if (typeof(IEnumerable).IsAssignableFrom(instance.GetType()))
            {
                var i = 0;
                foreach (var item in (IEnumerable) instance)
                {
                    DrawRow(i.ToString(), item);
                    i++;
                }
            }
            else
            {
                const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                var type = instance.GetType();
                var memberInfos =
                    type.GetFields(bindingFlags).Cast<MemberInfo>()
                    .Concat(type.GetProperties(bindingFlags))
                    .OrderBy(x => x.Name)
                    .ToArray();

                foreach (var memberInfo in memberInfos)
                {
                    object value;
                    switch (memberInfo)
                    {
                        case FieldInfo f:
                            value = f.GetValue(instance);
                            break;

                        case PropertyInfo p:
                            value = p.GetValue(instance);
                            break;

                        default:
                            throw new InvalidOperationException();
                    }
                    DrawRow(memberInfo.Name, value);
                }
            }
        }

        private static void DrawRow(string name, object propertyValue)
        {
            var displayValue = propertyValue?.ToString() ?? "[null]";

            var hasChildNodes = false;
            if (propertyValue != null)
            {
                var typeCode = Type.GetTypeCode(propertyValue.GetType());
                if (typeCode == TypeCode.Object)
                {
                    hasChildNodes = true;
                }
            }

            ImGui.AlignTextToFramePadding();

            var nodeOpen = false;
            if (hasChildNodes)
            {
                nodeOpen = ImGui.TreeNode(name);
            }
            else
            {
                ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.Bullet);
            }

            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(displayValue);
            ImGui.NextColumn();

            if (nodeOpen)
            {
                DrawObject(propertyValue);
                ImGui.TreePop();
            }
        }
    }
}
