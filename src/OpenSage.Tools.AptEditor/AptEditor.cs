using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.Util;
using OpenSage.Tools.AptEditor.UI;
using OpenSage.Data;
using OpenSage.FileFormats.Apt;

namespace OpenSage.Tools.AptEditor
{
    class AptEditor : IDisposable
    {

        public IEnumerable<string> SearchPaths => _searchPaths.Keys;
        private SortedDictionary<string, string> _searchPaths;
        public IEnumerable<AptSceneInstance> Consoles => _consoles.AsReadOnly();
        private List<AptSceneInstance> _consoles;
        public IEnumerable<string> OpenFiles => _openFiles.Keys;
        private SortedDictionary<string, AptEditInstance> _openFiles;

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



        public bool Open(string rootPath, out AptEditInstance edit)
        {
            rootPath = rootPath.NormalizePath();
            if (_openFiles.ContainsKey(rootPath))
            {
                edit = _openFiles[rootPath];
                return false;
            }
            else
            {
                var apt = AptFile.Parse(rootPath);
                edit = new AptEditInstance(apt);
                _openFiles[rootPath] = edit;
                return true;
            }
        }

        public bool Close(string rootPath)
        {
            rootPath = rootPath.NormalizePath();
            var flag = _openFiles.TryGetValue(rootPath, out var edit);
            if (flag && edit != null)
            {
                // close without save
                // TODO see if anything needed
                _openFiles.Remove(rootPath);
                return true;
            }
            return false;
        }

        public bool Save(AptEditInstance edit)
        {
            return false;
        }

        public bool SaveAs(int fileIndex, string rootPath)
        {
            return false;
        }

        public void AddSearchPath(string path, string mapped = "") { _searchPaths[path] = mapped; }
        public Func<string, bool> RemoveSearchPath => _searchPaths.Remove;

        public void InitConsole(Action<Game>? attachAdditionalRenderers, Action<Game>? detachAdditionalRenderers)
        {
            var rootPath = _searchPaths.Count > 0 ? _searchPaths.Keys.First() : ""; // TODO weird...
            var installation = new GameInstallation(new AptEditorDefinition(), rootPath);
            using var game = new Game(installation, null, new Configuration { LoadShellMap = false });
            var scene = new AptSceneInstance(game);


            // add all paths
            foreach (var kvp in _searchPaths)
            {
                var orig = kvp.Key;
                var mapped = kvp.Value;
                var mapping = FileUtilities.GetFilesMappingByDirectory(orig, mapped, out var _, out var __);
                game.ContentManager.FileSystem.LoadFiles(mapping, isPhysicalFile: true, loadArtOnly: false);
            }

            // TODO multi-thread & file?
            _consoles.Add(scene);

            if (attachAdditionalRenderers != null)
                attachAdditionalRenderers(game);
            try
            {
                game.ShowMainMenu();
                game.Run();
            }
            finally
            {
                _consoles.Remove(scene);
                if (detachAdditionalRenderers != null)
                    detachAdditionalRenderers(game);
            }
        }

        public bool Debug(string fileIndex)
        {
            if (_consoles.Count == 0)
                throw new InvalidOperationException("Debugging environment not initialized. Please initialize it first.");
            var file = _openFiles[fileIndex];
            var apt = file.AptFile;

            return false;
        }
    }
}
