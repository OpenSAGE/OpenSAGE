using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Tools.AptEditor.UI;
using OpenSage.Data;

namespace OpenSage.Tools.AptEditor
{
    class AptEditor : IDisposable
    {
        public Game Game { get; private set; }
        public GameWindow Window => Game.Window;

        public AptSceneManager Manager { get; private set; }

        public ICollection<string> SearchPaths => _searchPaths.Keys;
        private SortedDictionary<string, string> _searchPaths;
        public IReadOnlyList<AptSceneManager> Consoles => _consoles.AsReadOnly();
        private List<AptSceneManager> _consoles;
        public IReadOnlyList<AptEditInstance> OpenFiles => _openFiles.AsReadOnly();
        private List<AptEditInstance> _openFiles;
        public FileSystemConstructor FileInteractor { get; private set; }

        public AptEditor()
        {
            // edit
            _openFiles = new();

            // debug
            // TODO create game inside this class
            _consoles = new();
            _searchPaths = new();
            // InitConsole(game);
        }

        public void Dispose()
        {

        }

        public void AddSearchPath(string path, string mapped = "") { _searchPaths[path] = mapped; }
        public Func<string, bool> RemoveSearchPath => _searchPaths.Remove;

        public void InitConsole(Game game = null)
        {
            Game = game;
            Manager = new(game);
            FileInteractor = new(game);
        }

        public void AddStarchPaths()
        {

        }

        public void InitWindow(Action<Game>? attachAdditionalRenderers, Action<Game>? detachAdditionalRenderers)
        {
            var rootPath = _searchPaths.Count > 0 ? _searchPaths.Keys.First() : ""; // TODO weird...
            var installation = new GameInstallation(new AptEditorDefinition(), rootPath);
            using var game = new Game(installation, null, new Configuration { LoadShellMap = false });

            // add all paths
            foreach(var kvp in _searchPaths)
            {
                var orig = kvp.Key;
                var mapped = kvp.Value;
                var mapping = FileUtilities.GetFilesMappingByDirectory(orig, mapped, out var _, out var __);
                game.ContentManager.FileSystem.LoadFiles(mapping, isPhysicalFile: true, loadArtOnly: false);
            }


            if (attachAdditionalRenderers != null)
                attachAdditionalRenderers(game);
            try
            {
                game.ShowMainMenu();
                game.Run();
            }
            finally
            {
                if (detachAdditionalRenderers != null)
                    detachAdditionalRenderers(game);
            }
        }

        public bool Open(string path, out AptEditInstance edit)
        {
            edit = null;
            return false;
        }

        public bool Close(int fileIndex)
        {
            return false;
        }

        public bool Save(int fileIndex)
        {
            return false;
        }

        public bool SaveAs(int fileIndex, string rootPath)
        {
            return false;
        }

        public bool Debug(int fileIndex)
        {
            if (Game == null)
                throw new InvalidOperationException("Debugging environment not initialized. Please initialize it first.");
            var file = _openFiles[fileIndex];
            var apt = file.AptFile;

            return false;
        }
    }
}
