using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using OpenSage.Data;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.Gui.Apt;
using OpenSage.Mathematics;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.UI
{
    internal class AptLoadFailure : Exception
    {
        public string? File { get; }
        public AptLoadFailure(string? file) : base(file is null ? "Apt Load failed" : $"Cannot load {file}")
        {
            File = file;
        }
    }

    internal class AptSceneManager
    {
        // deserted
        public Character? CurrentCharacter { get; private set; }
        public LogicalInstructions? CurrentActions { get; set; }
        public bool IsCurrentCharacterImported => CurrentCharacter?.Container != EditManager?.AptFile;

        public string CurrentTitle { get; set; }
        public int NumberOfFrames { get; private set; }
        public int? CurrentFrameWrapped => NumberOfFrames == 0 ? new int?() : CurrentFrame % NumberOfFrames;
        public int CurrentFrame { get; private set; }

        public ColorRgbaF DisplayBackgroundColor { get; private set; }

        // edit
        public AptEditManager? EditManager { get; private set; }
        public string? CurrentAptPath { get; private set; }

        public int MillisecondsPerFrame => (int) (EditManager?.AptFile.Movie.MillisecondsPerFrame ?? 30);

        // runtime
        public Game Game { get; }
        public AptWindow? CurrentWindow { get; private set; }
        public WrappedDisplayItem CurrentItem { get; private set; }

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


        public AptSceneManager(Game game)
        {
            UnloadApt();
            Game = game;
        }

        // Apt Loading Operations

        public void UnloadApt()
        {
            CurrentAptPath = null;
            NumberOfFrames = 0;
            CurrentFrame = 0;
            CurrentOffset = Vector2.Zero;
            CurrentScale = 1;
            EditManager = null;
            CurrentWindow = null;
            CurrentItem = null;
            CurrentActions = null;
            CurrentTitle = "";
        }

        public void LoadApt(string path)
        {
            UnloadApt();
            var entry = Game.ContentManager.FileSystem.GetFile(path);
            if (entry == null)
            {
                throw new AptLoadFailure(path);
            }

            AptFile aptFile;
            try
            {
                aptFile = AptFileHelper.FromFileSystemEntry(entry);
            }
            catch (FileNotFoundException fileNotFound)
            {
                throw new AptLoadFailure(fileNotFound.FileName);
            }

            CurrentAptPath = path;
            EditManager = new AptEditManager(aptFile);
        }

        public void LoadApt(AptFile aptFile, string? name = null)
        {
            UnloadApt();
            
            CurrentAptPath = name ?? aptFile.MovieName;
            EditManager = new AptEditManager(aptFile);
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

            if (EditManager != null)
            {
                CurrentWindow = new AptWindow(Game, Game.ContentManager, EditManager.AptFile);
                
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
                {
                    list.RemoveItem(list.Items.Keys.First());
                }
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
            if (EditManager is null)
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
