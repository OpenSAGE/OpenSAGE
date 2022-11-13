using System.Collections.Generic;
using System.IO;
using OpenSage.Content;

namespace OpenSage.Input.Cursors
{
    internal sealed class CursorManager : DisposableBase
    {
        private readonly Dictionary<string, Cursor> _cachedCursors;
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
            _cachedCursors = new Dictionary<string, Cursor>();

            _assetStore = assetStore;
            _contentManager = contentManager;
            _window = window;
        }

        public void SetCursor(string cursorName, in TimeInterval time)
        {
            if (!_cachedCursors.TryGetValue(cursorName, out var cursor))
            {
                var mouseCursor = _assetStore.MouseCursors.GetByName(cursorName);
                if (mouseCursor == null)
                {
                    return;
                }

                var cursorFileName = mouseCursor.Image;
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

                _cachedCursors[cursorName] = cursor = AddDisposable(new Cursor(cursorEntry, _window?.WindowScale ?? 1.0f));
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
