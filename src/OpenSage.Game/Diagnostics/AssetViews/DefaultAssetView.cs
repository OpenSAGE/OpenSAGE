﻿using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using OpenSage.Audio;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class DefaultAssetView : AssetView
    {
        private readonly BaseAsset _asset;
        private SoundView _currentSoundView;

        public DefaultAssetView(DiagnosticViewContext context, BaseAsset asset)
            : base(context)
        {
            _asset = asset;
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

        private void DrawObject(object instance)
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

        private void DrawRow(string name, object propertyValue)
        {
            var hasChildNodes = false;
            if (propertyValue != null)
            {
                var propertyType = propertyValue.GetType();
                var typeCode = Type.GetTypeCode(propertyType);
                if (typeCode == TypeCode.Object && propertyType != typeof(Percentage))
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
                ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
            }

            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();

            if (propertyValue is AudioFile audioFile && audioFile.Entry != null)
            {
                if (ImGui.Button("Play audio"))
                {
                    ImGui.OpenPopup("Audio player");
                    _currentSoundView = AddDisposable(new SoundView(Context, audioFile));
                }
                var isAudioPlayerOpen = _currentSoundView != null;
                if (ImGui.BeginPopupModal("Audio player", ref isAudioPlayerOpen))
                {
                    _currentSoundView.Draw();
                    ImGui.EndPopup();
                }
                if (!isAudioPlayerOpen && _currentSoundView != null)
                {
                    RemoveAndDispose(ref _currentSoundView);
                }
            }
            else
            {
                var displayValue = propertyValue?.ToString() ?? "[null]";
                ImGui.Text(displayValue);
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
