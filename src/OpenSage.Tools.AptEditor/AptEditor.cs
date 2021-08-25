using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Tools.AptEditor.Apt;
using OpenSage.Tools.AptEditor.UI;

namespace OpenSage.Tools.AptEditor
{
    class AptEditor
    {
        public Game Game { get; private set; }
        public GameWindow Window => Game.Window;

        public AptSceneManager Manager { get; private set; }
        private List<AptEditInstance> _openFiles;
        public IReadOnlyList<AptEditInstance> OpenFiles => _openFiles.AsReadOnly();

        public FileSystemConstructor FileInteractor { get; private set; }

        public AptEditor(Game game)
        {
            // edit
            _openFiles = new();

            // debug
            // TODO create game inside this class
            InitDebugEnv(game);
        }

        public void InitDebugEnv(Game game)
        {
            Game = game;
            Manager = new(game);
            FileInteractor = new(game);
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
