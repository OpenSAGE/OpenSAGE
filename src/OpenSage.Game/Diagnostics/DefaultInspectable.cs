using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using OpenSage.Content;
using OpenSage.Diagnostics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics
{
    internal sealed class DefaultInspectable : IInspectable
    {
        private delegate bool InspectableDrawerDrawDelegate(ref object propertyValue, DiagnosticViewContext context);

        private sealed class InspectableDrawer
        {
            public readonly Type Type;
            public readonly InspectableDrawerDrawDelegate Draw;
            public readonly bool HasChildNodes;

            public InspectableDrawer(Type type, InspectableDrawerDrawDelegate draw, bool hasChildNodes = false)
            {
                Type = type;
                Draw = draw;
                HasChildNodes = hasChildNodes;
            }
        }

        private static readonly List<InspectableDrawer> Drawers;

        static DefaultInspectable()
        {
            Drawers = new List<InspectableDrawer>();

            Drawers.Add(new InspectableDrawer(typeof(string), (ref object v, DiagnosticViewContext context) =>
            {
                var s = (string) v;
                if (ImGui.InputText("", ref s, 1000))
                {
                    v = s;
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(bool), (ref object v, DiagnosticViewContext context) =>
            {
                var b = (bool) v;
                if (ImGui.Checkbox("", ref b))
                {
                    v = b;
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(int), (ref object v, DiagnosticViewContext context) =>
            {
                var i = (int) v;
                if (ImGui.DragInt("", ref i))
                {
                    v = i;
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(float), (ref object v, DiagnosticViewContext context) =>
            {
                var f = (float) v;
                if (ImGui.DragFloat("", ref f))
                {
                    v = f;
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(Vector3), (ref object v, DiagnosticViewContext context) =>
            {
                var c = (Vector3) v;
                if (ImGui.DragFloat3("", ref c))
                {
                    v = c;
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(Percentage), (ref object v, DiagnosticViewContext context) =>
            {
                var f = (float) (Percentage) v;
                if (ImGui.DragFloat("", ref f))
                {
                    v = new Percentage(f);
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(ColorRgb), (ref object v, DiagnosticViewContext context) =>
            {
                var c = ((ColorRgb) v).ToVector3();
                if (ImGui.ColorEdit3("", ref c))
                {
                    v = new ColorRgb(
                        (byte) (c.X * 255.0f),
                        (byte) (c.Y * 255.0f),
                        (byte) (c.Z * 255.0f));
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(ColorRgba), (ref object v, DiagnosticViewContext context) =>
            {
                var c = ((ColorRgba) v).ToVector4();
                if (ImGui.ColorEdit4("", ref c))
                {
                    v = new ColorRgba(
                        (byte) (c.X * 255.0f),
                        (byte) (c.Y * 255.0f),
                        (byte) (c.Z * 255.0f),
                        (byte) (c.W * 255.0f));
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(ColorRgbF), (ref object v, DiagnosticViewContext context) =>
            {
                var c = ((ColorRgbF) v).ToVector3();
                if (ImGui.ColorEdit3("", ref c))
                {
                    v = new ColorRgbF(c.X, c.Y, c.Z);
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(ColorRgbaF), (ref object v, DiagnosticViewContext context) =>
            {
                var c = ((ColorRgbaF) v).ToVector4();
                if (ImGui.ColorEdit4("", ref c))
                {
                    v = new ColorRgbaF(c.X, c.Y, c.Z, c.W);
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(Enum), (ref object v, DiagnosticViewContext context) =>
            {
                var e = (Enum) v;
                if (ImGuiUtility.ComboEnum(v.GetType(), "", ref e))
                {
                    v = e;
                    return true;
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(ILazyAssetReference), (ref object v, DiagnosticViewContext context) =>
            {
                var asset = (ILazyAssetReference) v;
                if (asset.Value != null)
                {
                    if (ImGui.Button(asset.Value.FullName))
                    {
                        context.SelectedObject = asset.Value;
                    }
                }
                else
                {
                    ImGui.Text("<null>");
                }
                return false;
            }));

            Drawers.Add(new InspectableDrawer(typeof(BaseAsset), (ref object v, DiagnosticViewContext context) =>
            {
                var asset = (BaseAsset) v;
                if (ImGui.Button(asset.FullName))
                {
                    context.SelectedObject = v;
                }
                return false;
            }));

            // Order matters here - this must be last.
            Drawers.Add(new InspectableDrawer(typeof(object), (ref object v, DiagnosticViewContext context) =>
            {
                return false;
            }, hasChildNodes: true));
        }

        private readonly object _value;
        private readonly DiagnosticViewContext _context;

        public string Name => _value.GetType().Name;

        public DefaultInspectable(object value, DiagnosticViewContext context)
        {
            _value = value;
            _context = context;
        }

        void IInspectable.DrawInspector()
        {
            ImGuiUtility.BeginPropertyList();

            DrawObject(_value);

            ImGuiUtility.EndPropertyList();
        }

        private void DrawObject(object instance)
        {
            if (typeof(IEnumerable).IsAssignableFrom(instance.GetType()))
            {
                var i = 0;
                foreach (var item in (IEnumerable) instance)
                {
                    ImGui.PushID(i);

                    DrawRow($"Element {i} ({item.GetType().Name})", item, null);
                    i++;

                    ImGui.PopID();
                }
            }
            else
            {
                const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                var type = instance.GetType();
                var memberInfos =
                    type.GetFields(bindingFlags)
                    .Cast<MemberInfo>()
                    .Concat(type.GetProperties(bindingFlags))
                    .OrderBy(x => x.Name)
                    .ToArray();

                var i = 0;
                foreach (var memberInfo in memberInfos)
                {
                    var value = memberInfo switch
                    {
                        FieldInfo f => f.GetValue(instance),
                        PropertyInfo p => p.GetValue(instance),
                        _ => throw new InvalidOperationException(),
                    };
                    void SetValue(object newValue)
                    {
                        switch (memberInfo)
                        {
                            case FieldInfo f:
                                f.SetValue(instance, newValue);
                                break;

                            case PropertyInfo p:
                                p.SetValue(instance, newValue);
                                break;

                            default:
                                throw new InvalidOperationException();
                        }
                    }

                    ImGui.PushID(i);

                    DrawRow(memberInfo.Name, value, SetValue);
                    i++;

                    ImGui.PopID();
                }
            }
        }

        private void DrawRow(string name, object propertyValue, Action<object> setPropertyValue)
        {
            InspectableDrawer drawer = null;
            var hasChildNodes = false;
            if (propertyValue != null)
            {
                var propertyType = propertyValue.GetType();
                drawer = Drawers.First(x => x.Type.IsAssignableFrom(propertyType));
                hasChildNodes = drawer.HasChildNodes;
            }

            ImGui.AlignTextToFramePadding();

            var nodeOpen = false;
            if (hasChildNodes)
            {
                nodeOpen = ImGui.TreeNode(name);
            }
            else
            {
                ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
            }

            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();

            if (drawer != null)
            {
                if (drawer.Draw(ref propertyValue, _context))
                {
                    setPropertyValue?.Invoke(propertyValue);
                }
            }
            else
            {
                ImGui.Text("<null>");
            }

            ImGui.NextColumn();

            if (nodeOpen)
            {
                DrawObject(propertyValue);
                ImGui.TreePop();
            }
        }
    }
}
