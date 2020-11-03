using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Tools.AptEditor.Apt.Editor;
using OpenSage.Tools.AptEditor.Util;
using Action = System.Action;

namespace OpenSage.Tools.AptEditor.UI.Widgets
{
    internal class FrameItemList : IWidget
    {
        public const string Name = "Frame Properties";
        private FrameItemUtilities _utilities;
        private InstructionEditor _currentFrameAction;
        public FrameItemList()
        {
            _utilities = new FrameItemUtilities();
        }

        public void Draw(AptSceneManager manager)
        {
            if(_utilities.Reset(manager))
            {
                // if this is a different frame (imply _utilities.Active == true)
                ImGui.SetNextWindowSize(new Vector2(0, 0));
            }
            else if(!_utilities.Active)
            {
                _currentFrameAction = null;
                return;
            }

            if(ImGui.Begin(Name))
            {
                // frame label
                ImGui.Text("Frame labels");
                ImGui.Button("New Frame label");
                foreach (var label in _utilities.FrameLabels)
                {
                    ImGui.Text($"{label.FrameId}");
                    ImGui.SameLine();
                    ImGui.TextColored(new Vector4(1, 1, 0, 1), label.Name);
                    ImGui.SameLine();
                    ImGui.Text($"{label.Flags}");
                    ImGui.SameLine(ImGui.GetWindowWidth() - 100);
                    ImGui.Button("Remove");
                    ImGui.NewLine();
                    // TODO: is frame label globally visible?
                }
                ImGui.Separator();

                // background color
                if(!_utilities.BackgroundColors.Any())
                {
                    ImGui.Button("Set background color");
                }
                foreach(var color in _utilities.BackgroundColors)
                {
                    ImGui.Text("Background Color");
                    ImGui.SameLine();
                    ImGui.ColorButton("Background color", color.Color.ToColorRgbaF().ToVector4());
                    ImGui.SameLine(ImGui.GetWindowWidth() - 100);
                    ImGui.Button("Remove");
                }
                ImGui.Separator();

                // actions
                if(!_utilities.FrameActions.Any())
                {
                    ImGui.Button("Add frame Action");
                }
                foreach(var item in _utilities.FrameActions)
                {
                    if(ImGui.Button("Frame Action"))
                    {
                        _currentFrameAction = new InstructionEditor(item.Instructions);
                    }
                }
                ImGui.Separator();

                // initActions
                ImGui.Button("Add InitAction");
                var indexColor = new Vector4(0, 1, 1, 1);
                foreach(var item in _utilities.InitActions)
                {
                    ImGui.TextColored(indexColor, $"{item.Sprite}");
                    ImGui.SameLine(35, 5);
                    if(ImGui.Button("Sprite InitAction"))
                    {
                        _currentFrameAction = new InstructionEditor(item.Instructions);
                    }
                }
                ImGui.Separator();

                // placeobjects
                ImGui.Text("Place commands");
                ImGui.Button("New");
                ImGui.Indent(10);
                foreach(var (depth, placeObject) in _utilities.PlaceObjects)
                {
                    ImGui.Separator();
                    ImGui.Text($"Depth: {depth}");
                    ImGui.SameLine(ImGui.GetWindowWidth() - 100);
                    ImGui.Button("Remove");

                    if(placeObject.IsRemoveObject)
                    {
                        ImGui.Text("Remove character in the current depth.");
                        continue;
                    }

                    ImGui.TextColored(indexColor, "Character");
                    ProcessPlaceCharacter(manager, placeObject);
                    ImGui.Spacing();
                    ProcessTransform(placeObject);
                    ImGui.Spacing();
                    ProcessColorTransform(placeObject);
                    ImGui.Spacing();
                    ProcessRatio(placeObject);
                    ImGui.Spacing();
                    ProcessName(placeObject);
                    ImGui.Spacing();
                    ProcessClipEvents(placeObject);

                }
                ImGui.Unindent();
            }
            ImGui.End();

            // Draw Frame's Action / InitAction
            if(_currentFrameAction != null)
            {
                _currentFrameAction.Draw(manager);
            }
        }

        private static void ProcessPlaceCharacter(AptSceneManager manager, LogicalPlaceObject placeObject)
        {
            // Initialize: non null
            // Edit: -> replace if non null
            if(!placeObject.ModifyingExisting)
            {
                ImGui.Button("Initialize");
                ImGui.SameLine();
                ImGui.Button(ToStringOrDefault(placeObject.Character, "Invalid"));
                if(!placeObject.Character.HasValue)
                {
                    manager.SubmitError("A character ID must be set");
                }
                ImGui.NewLine();
            }
            else
            {
                if(!placeObject.Character.HasValue)
                {
                    ImGui.Button("Edit");
                }
                else{
                    ImGui.Button("Replace with");
                }
                ImGui.SameLine();
                ImGui.Button(ToStringOrDefault(placeObject.Character, "Existing"));
                ImGui.NewLine();
            }
        }

        private static void ProcessTransform(LogicalPlaceObject placeObject)
        {
            if(!placeObject.Transform.HasValue)
            {
                ImGui.Button("Add Transformation");
                return;
            }

            ImGui.Text("Transformation");
            ImGui.SameLine(ImGui.GetWindowWidth() - 100);
            ImGui.Button("Remove");

            var (rotation, skew, scale) = FrameItemUtilities.GetRotationSkewAndScale(placeObject.Transform.Value, 0.0001f);

            var rotationInDegrees = rotation * (180 / MathF.PI);
            ImGui.InputFloat("Rotation", ref rotationInDegrees, 1, 10, "%f deg");

            var skewInDegrees = skew * (180 / MathF.PI);
            ImGui.InputFloat("Skew", ref skewInDegrees, 1, 10, "%f deg");

            ImGui.InputFloat2("Scale", ref scale);

            var translation = placeObject.Transform.Value.Translation;
            ImGui.InputFloat2("Translation", ref translation);
        }

        private static void ProcessColorTransform(LogicalPlaceObject placeObject)
        {
            if(!placeObject.ColorTransform.HasValue)
            {
                ImGui.Button("Add Color Transform");
                return;
            }
            ImGui.Text("Color Transform");
            ImGui.SameLine();
            ImGui.ColorButton("Color Transform", placeObject.ColorTransform.Value.ToColorRgbaF().ToVector4());
            ImGui.SameLine(ImGui.GetWindowWidth() - 100);
            ImGui.Button("Remove");
        }

        private static void ProcessRatio(LogicalPlaceObject placeObject)
        {
            ImGui.Text($"Ratio: {ToStringOrDefault(placeObject.Ratio, "None")}");
        }

        private static void ProcessName(LogicalPlaceObject placeObject)
        {
            if(placeObject.Name == null)
            {
                ImGui.Button("Add Name");
                return;
            }

            var name = placeObject.Name;
            ImGui.InputText("Name", ref name, (uint)(name.Length + 10));
            ImGui.SameLine(ImGui.GetWindowWidth() - 100);
            ImGui.Button("Remove");
        }

        private static void ProcessClipEvents(LogicalPlaceObject placeObject)
        {
            if(placeObject.ClipEvents == null)
            {
                ImGui.Button("Add Ciip Events");
                return;
            }

            ImGui.Text($"Clip Event Count: {placeObject.ClipEvents.Count}");
            return;
        }

        private static string ToStringOrDefault<T>(T? item, string defaultValue) where T : struct
        {
            if(item.HasValue)
            {
                return item.ToString();
            }
            return defaultValue;
        }
    }
}