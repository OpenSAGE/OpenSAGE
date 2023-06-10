using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Content;

namespace OpenSage.Input.Cursors
{
    internal sealed class CursorManager : DisposableBase
    {
        private readonly Dictionary<(string, CursorDirection), Cursor> _cachedCursors;
        private Cursor _currentCursor;

        private readonly AssetStore _assetStore;
        private readonly ContentManager _contentManager;
        private readonly GameWindow _window;

        public bool IsCursorVisible
        {
            set
            {
                if (_window != null)
                {
                    _window.IsCursorVisible = value;
                }
            }
        }

        public CursorManager(AssetStore assetStore, ContentManager contentManager, GameWindow window)
        {
            _cachedCursors = new Dictionary<(string, CursorDirection), Cursor>();

            _assetStore = assetStore;
            _contentManager = contentManager;
            _window = window;
        }

        private static uint DirectionLayout1(CursorDirection direction)
        {
            return 0;
        }
        private static uint DirectionLayout2(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.Up => 0,
                CursorDirection.RightUp => 0,
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 0,
                CursorDirection.Down => 0,
                CursorDirection.LeftDown => 1,
                CursorDirection.Left => 1,
                CursorDirection.LeftUp => 1,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private static uint DirectionLayout3(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.RightUp => 0,
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 0,
                CursorDirection.Down => 1,
                CursorDirection.LeftDown => 1,
                CursorDirection.Left => 2,
                CursorDirection.LeftUp => 2,
                CursorDirection.Up => 2,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private static uint DirectionLayout4(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 1,
                CursorDirection.Down => 1,
                CursorDirection.LeftDown => 1,
                CursorDirection.Left => 2,
                CursorDirection.LeftUp => 3,
                CursorDirection.Up => 3,
                CursorDirection.RightUp => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private static uint DirectionLayout5(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 1,
                CursorDirection.Down => 1,
                CursorDirection.LeftDown => 2,
                CursorDirection.Left => 3,
                CursorDirection.LeftUp => 3,
                CursorDirection.Up => 4,
                CursorDirection.RightUp => 4,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private static uint DirectionLayout6(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 1,
                CursorDirection.Down => 1,
                CursorDirection.LeftDown => 2,
                CursorDirection.Left => 3,
                CursorDirection.LeftUp => 4,
                CursorDirection.Up => 5,
                CursorDirection.RightUp => 5,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private static uint DirectionLayout7(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 1,
                CursorDirection.Down => 2,
                CursorDirection.LeftDown => 3,
                CursorDirection.Left => 4,
                CursorDirection.LeftUp => 4,
                CursorDirection.Up => 5,
                CursorDirection.RightUp => 6,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        private static uint DirectionLayout8(CursorDirection direction)
        {
            return direction switch
            {
                CursorDirection.Right => 0,
                CursorDirection.RightDown => 1,
                CursorDirection.Down => 2,
                CursorDirection.LeftDown => 3,
                CursorDirection.Left => 4,
                CursorDirection.LeftUp => 5,
                CursorDirection.Up => 6,
                CursorDirection.RightUp => 7,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private delegate uint DirectionLayoutDelegate(CursorDirection direction);
        private static readonly DirectionLayoutDelegate[] DirectionLayout = {
            DirectionLayout1, DirectionLayout2, DirectionLayout3, DirectionLayout4,
            DirectionLayout5, DirectionLayout6, DirectionLayout7, DirectionLayout8
        };

        public void SetCursor(string cursorName, in TimeInterval time)
        {
            SetCursor(cursorName, CursorDirection.Right, time);
        }

        public void SetCursor(string cursorName, CursorDirection direction, in TimeInterval time)
        {
            (string, CursorDirection)key = (cursorName, direction);
            if (!_cachedCursors.TryGetValue(key, out var cursor))
            {
                var mouseCursor = _assetStore.MouseCursors.GetByName(cursorName);
                if (mouseCursor == null)
                {
                    return;
                }

                var cursorFileName = mouseCursor.Image;
                if (mouseCursor.Directions > 0)
                {
                    int directions = mouseCursor.Directions > 8 ? 8 : mouseCursor.Directions;
                    cursorFileName += DirectionLayout[directions - 1](direction);
                }
                if (string.IsNullOrEmpty(Path.GetExtension(cursorFileName)))
                {
                    cursorFileName += ".ani";
                }

                var cursorDirectory = Path.Combine("Data", "Cursors");

                var cursorFilePath = Path.Combine(cursorDirectory, cursorFileName);
                var cursorEntry = _contentManager.FileSystem.GetFile(cursorFilePath);

                // a few cursors are in all lowercase instead of their normal PascalCase naming
                if (cursorEntry is null)
                {
                    cursorFilePath = Path.Combine(cursorDirectory, cursorFileName.ToLower());
                    cursorEntry = _contentManager.FileSystem.GetFile(cursorFilePath);
                }

                _cachedCursors[key] = cursor = AddDisposable(new Cursor(cursorEntry, _window?.WindowScale ?? 1.0f));
            }

            if (_currentCursor == cursor)
            {
                return;
            }

            _currentCursor = cursor;
            _currentCursor.Apply(time);
        }

        public void Update(in TimeInterval time)
        {
            _currentCursor?.Update(time);
        }
    }
}
