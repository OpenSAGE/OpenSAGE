using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using OpenSage.Data;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Apt
{
    internal class AptSceneInstance
    {
        public Game Game { get; }
        public ColorRgbaF DisplayBackgroundColor { get; private set; }

        public AptFile? AptFile { get; private set; }
        public int MillisecondsPerFrame => (int) (AptFile?.Movie.MillisecondsPerFrame ?? 30);
        
        public AptWindow? CurrentWindow { get; private set; }
        public WrappedDisplayItem? CurrentItem { get; private set; }
        public Character? CurrentCharacter { get; private set; }
        public bool IsCurrentCharacterImported => CurrentCharacter?.Container != AptFile;

        public int NumberOfFrames { get; private set; }
        public int? CurrentFrameWrapped => NumberOfFrames == 0 ? new int?() : CurrentFrame % NumberOfFrames;
        public int CurrentFrame { get; private set; }

        public Vector2 CurrentOffset
        {
            get => CurrentWindow?.WindowTransform.GeometryTranslation ?? Vector2.Zero;
            set => SetTransform((ref ItemTransform t) => t.GeometryTranslation = value);
        }
        public float CurrentScale
        {
            get => CurrentWindow?.WindowTransform.GeometryTransform.M11 ?? 1;
            set => SetTransform((ref ItemTransform t) => t.GeometryRotationScale = new Matrix2x2(value, 0, 0, value));
        }


        public AptSceneInstance(Game game)
        {
            ResetAll();
            Game = game;
        }

        public AptSceneInstance(
            string rootPath, 
            Dictionary<string, string>? searchPaths = null,
            Action<AptSceneInstance>? attachAdditionalRenderers = null,
            Action<AptSceneInstance>? detachAdditionalRenderers = null
            )
        {
            var installation = new GameInstallation(new AptEditorDefinition(), rootPath);
            Game = new Game(installation, null, new Configuration { LoadShellMap = false });
            var scene = new AptSceneInstance(Game);
            ResetAll();

            // add all paths
            if (searchPaths != null)
                foreach (var kvp in searchPaths)
                {
                    var orig = kvp.Key;
                    var mapped = kvp.Value;
                    var mapping = FileUtilities.GetFilesMappingByDirectory(orig, mapped, out var _, out var __);
                    Game.ContentManager.FileSystem.LoadFiles(mapping, isPhysicalFile: true, loadArtOnly: false);
                }

            // TODO multi-thread & file?

            if (attachAdditionalRenderers != null)
                attachAdditionalRenderers(this);
            try
            {
                Game.ShowMainMenu();
                Game.Run();
            }
            finally
            {
                
                if (detachAdditionalRenderers != null)
                    detachAdditionalRenderers(this);
            }
        }

        // Apt Loading Operations

        public void ResetAll()
        {
            if (CurrentWindow != null)
            {
                var list = CurrentWindow.Root.Content;
                while (list.Items.Any())
                    list.RemoveItem(list.Items.Keys.First());
            }
            NumberOfFrames = 0;
            CurrentFrame = 0;
            CurrentOffset = Vector2.Zero;
            CurrentScale = 1;
            AptFile = null;
            CurrentWindow = null;
            CurrentItem = null;
            CurrentCharacter = null;
        }

        public void SetApt(AptFile aptFile)
        {
            ResetAll();
            AptFile = aptFile;
        }

        // Character Loading Operations

        public void ChangeDisplayBackgroundColor(ColorRgbaF newColor)
        {
            if (DisplayBackgroundColor == newColor)
            {
                return;
            }

            DisplayBackgroundColor = newColor;
            ResetAptWindow();
        }

        public void ResetAptWindow()
        {
            // unload current resources

            CurrentWindow = null;
            CurrentItem = null;
            // CurrentActions = null;

            var windows = Game.Scene2D.AptWindowManager;
            while (windows.OpenWindowCount > 0)
            {
                windows.PopWindow();
            }

            // load new resources

            var multiplied = DisplayBackgroundColor.ToVector4() * 255;
            var color = new ColorRgba((byte) multiplied.X, (byte) multiplied.Y, (byte) multiplied.Z, (byte) multiplied.W);
            windows.PushWindow(AptEditorBackgroundSource.CreateBackgroundAptWindow(Game, color));

            if (AptFile != null)
            {
                CurrentWindow = new AptWindow(Game, Game.ContentManager, AptFile);
                
                var root = CurrentWindow.Root;
                // hack: an AptWindow will always be updated by the game when it's loaded
                // because AptWindow.Root.IsNewFrame() will return true if the private member
                // _lastUpdate.TotalTime is zero.
                // By manually setting it to a non-zero value, and by stopping the sripte,
                // it won't be updated by the game anymore
                var field = root.GetType().GetField("_lastUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
                field!.SetValue(root, new TimeInterval(1, 0));
                CurrentWindow.Root.Stop();

                CurrentWindow.Context.Avm.Pause();

                // set display list
                var list = CurrentWindow.Root.Content;
                while (list.Items.Any())
                    list.RemoveItem(list.Items.Keys.First());

                if (CurrentCharacter != null)
                {
                    CurrentItem = new WrappedDisplayItem(CurrentCharacter, CurrentWindow.Context, CurrentWindow.Root);
                    list.AddItem(default, CurrentItem);
                }

                windows.PushWindow(CurrentWindow);
                if (CurrentItem is not null)
                    CurrentItem.PlayToFrameNoActions(0);
            }
        }

        public void SetCharacter(Character character)
        {
            if (AptFile is null)
            {
                throw new InvalidOperationException();
            }

            CurrentCharacter = character;
            NumberOfFrames = (CurrentCharacter as Playable)?.Frames.Count ?? 0;
            CurrentScale = 1f;
            CurrentOffset = new Vector2(200, 200);
            ResetAptWindow();
            if (NumberOfFrames > 0)
            {
                PlayToFrame(0);
            }
        }

        // Frame Operations

        public void PlayToFrame(int frame)
        {
            if (CurrentItem is null)
            {
                throw new InvalidOperationException();
            }

            CurrentFrame = frame;
            CurrentItem.PlayToFrameNoActions(CurrentFrame);
        }

        public void NextFrame()
        {
            if (CurrentItem is null)
            {
                throw new InvalidOperationException();
            }

            ++CurrentFrame;
            CurrentItem.UpdateNextFrameNoActions();
        }

        private delegate void TransformAction(ref ItemTransform t);
        private void SetTransform(TransformAction action)
        {
            if (CurrentWindow == null)
            {
                return;
            }

            action(ref CurrentWindow.WindowTransform);
        }

        // VM

        public string ExecuteOnce()
        {
            var _context = CurrentItem.Context;
            var last_executed_func = "AptContext Not Loaded";
            var cur_ctx = _context == null ? null : _context.Avm.CurrentContext();
            if (cur_ctx != null && (!cur_ctx.IsOutermost()))
            {
                var lef = _context.Avm.ExecuteOnce(true);
                last_executed_func = lef == null ? "null" : lef.ToString(cur_ctx);
            }
            else if (_context != null)
            {
                _context.Avm.PushContext(_context.Avm.DequeueContext());
                last_executed_func = "New Actions Pushed";
            }
            return last_executed_func;
        }
    }
}
