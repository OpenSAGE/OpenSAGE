using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptEditManager
    {
        private readonly Stack<IEditAction> _undoStack = new Stack<IEditAction>();
        private readonly Stack<IEditAction> _redoStack = new Stack<IEditAction>();

        public AptFile AptFile { get; }

        public AptEditManager(AptFile aptFile)
        {
            AptFile = aptFile;
        }

        public static AptEditManager Load(string rootPath, string aptPath)
        {
            var fs = new FileSystem(rootPath);
            var entry = fs.GetFile(aptPath);
            return new AptEditManager(AptFile.FromFileSystemEntry(entry));
        }

        public void Edit(IEditAction editAction)
        {
            _redoStack.Clear();
            if (_undoStack.TryPeek(out var previous) && previous.TryMerge(editAction))
            {
                previous.Edit();
            }
            else
            {
                editAction.Edit();
                _undoStack.Push(editAction);
            }
        }

        public string? GetUndoDescription()
        {
            if (!_undoStack.TryPeek(out var editAction))
            {
                return null;
            }
            return editAction.Description is string description
                ? $"Undo {description}"
                : "Undo";
        }
        public string? GetRedoDescription()
        {
            if (!_redoStack.TryPeek(out var editAction))
            {
                return null;
            }
            return editAction.Description is string description
                ? $"Redo {description}"
                : "Redo";
        }

        public void Undo()
        {
            var editAction = _undoStack.Pop();
            editAction.Undo();
            _redoStack.Push(editAction);
        }

        public void Redo()
        {
            var editAction = _redoStack.Pop();
            editAction.Edit();
            _undoStack.Push(editAction);
        }

        public AptDataDump GetAptDataDump()
        {
            return new AptDataDump(AptFile);
        }
    }

}
