using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using OpenSage.Diagnostics.Util;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics
{
    internal sealed class DefaultInspectable : IInspectable
    {
        private readonly object _value;

        public string Name => _value.GetType().Name;

        public DefaultInspectable(object value)
        {
            _value = value;
        }

        void IInspectable.DrawInspector()
        {
            DrawObject(_value);
        }

        private void DrawObject(object instance)
        {
            if (typeof(IEnumerable).IsAssignableFrom(instance.GetType()))
            {
                var i = 0;
                foreach (var item in (IEnumerable) instance)
                {
                    DrawRow($"Element {i} ({item.GetType().Name})", item, null);
                    i++;
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
                    DrawRow(memberInfo.Name, value, SetValue);
                }
            }
        }

        private void DrawRow(string name, object propertyValue, Action<object> setPropertyValue)
        {
            ImGui.AlignTextToFramePadding();

            switch (propertyValue)
            {
                case string s:
                    if (ImGui.InputText(name, ref s, 1000))
                    {
                        setPropertyValue?.Invoke(s);
                    }
                    break;

                case bool b:
                    if (ImGui.Checkbox(name, ref b))
                    {
                        setPropertyValue?.Invoke(b);
                    }
                    break;

                case int i:
                    if (ImGui.DragInt(name, ref i))
                    {
                        setPropertyValue?.Invoke(i);
                    }
                    break;

                case float f:
                    if (ImGui.DragFloat(name, ref f))
                    {
                        setPropertyValue?.Invoke(f);
                    }
                    break;

                case Percentage p:
                    var percentage = (float) p;
                    if (ImGui.DragFloat(name, ref percentage))
                    {
                        setPropertyValue?.Invoke(new Percentage(percentage));
                    }
                    break;

                case ColorRgb rgb:
                    var colorRgb = rgb.ToVector3();
                    if (ImGui.ColorEdit3(name, ref colorRgb))
                    {
                        setPropertyValue?.Invoke(new ColorRgb(
                            (byte) (colorRgb.X * 255.0f),
                            (byte) (colorRgb.Y * 255.0f),
                            (byte) (colorRgb.Z * 255.0f)));
                    }
                    break;

                case ColorRgba rgba:
                    var colorRgba = rgba.ToVector4();
                    if (ImGui.ColorEdit4(name, ref colorRgba))
                    {
                        setPropertyValue?.Invoke(new ColorRgba(
                            (byte) (colorRgba.X * 255.0f),
                            (byte) (colorRgba.Y * 255.0f),
                            (byte) (colorRgba.Z * 255.0f),
                            (byte) (colorRgba.W * 255.0f)));
                    }
                    break;

                case ColorRgbaF rgbaf:
                    var colorRgbaF = rgbaf.ToVector4();
                    if (ImGui.ColorEdit4(name, ref colorRgbaF))
                    {
                        setPropertyValue?.Invoke(new ColorRgbaF(
                            colorRgbaF.X,
                            colorRgbaF.Y,
                            colorRgbaF.Z,
                            colorRgbaF.W));
                    }
                    break;

                case Vector3 v3:
                    if (ImGui.DragFloat3(name, ref v3))
                    {
                        setPropertyValue?.Invoke(v3);
                    }
                    break;

                case Enum e:
                    if (ImGuiUtility.ComboEnum(propertyValue.GetType(), name, ref e))
                    {
                        setPropertyValue?.Invoke(e);
                    }
                    break;

                default:
                    var hasChildNodes = propertyValue != null &&
                        Type.GetTypeCode(propertyValue.GetType()) == TypeCode.Object;
                    if (hasChildNodes)
                    {
                        if (ImGui.TreeNode(name))
                        {
                            DrawObject(propertyValue);
                            ImGui.TreePop();
                        }
                    }
                    else
                    {
                        ImGui.LabelText(name, propertyValue?.ToString() ?? "<null>");
                    }
                    break;
            }
        }
    }
}
