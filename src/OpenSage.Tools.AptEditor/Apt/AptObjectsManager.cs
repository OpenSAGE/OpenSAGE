using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Writer;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptObjectsManager
    {
        private readonly Stack<IEditAction> _undoStack = new Stack<IEditAction>();
        private readonly Dictionary<int, int> _characterIdMap = new Dictionary<int, int>();
        public AptFile AptFile { get; private set; }
        public List<Frame> RealMovieFrames { get; }
        // TODO: Store a copy of AptFile instead of a reference to an extern AptFile
        public AptObjectsManager(AptFile aptFile)
        {
            AptFile = aptFile;
            RealMovieFrames = AptFile.Movie.Frames.ToList();
        }

        public static AptObjectsManager Load(string rootPath, string aptPath)
        {
            var fs = new FileSystem(rootPath);
            var entry = fs.GetFile(aptPath);
            return new AptObjectsManager(AptFile.FromFileSystemEntry(entry));
        }

        public void Edit(IEditAction editAction)
        {
            editAction.Execute(AptFile);
            _undoStack.Push(editAction);
        }

        public string GetUndoDescription()
        {
            return _undoStack.Peek().Description;
        }

        public void Undo()
        {
            _undoStack.Pop().Execute(AptFile);
        }

        public AptDataDump GetAptDataDump()
        {
            return new AptDataDump(AptFile);
        }

    }

}
