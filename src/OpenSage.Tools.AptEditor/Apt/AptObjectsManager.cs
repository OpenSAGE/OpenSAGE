using System;
using System.Collections.Generic;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Tools.AptEditor.Apt.Editor;

namespace OpenSage.Tools.AptEditor.Apt
{
    public sealed class AptObjectsManager
    {
        private readonly Stack<IEditAction> _undoStack = new Stack<IEditAction>();
        private readonly Stack<IEditAction> _redoStack = new Stack<IEditAction>();

        public AptFile AptFile { get; }

        public AptObjectsManager(AptFile aptFile)
        {
            AptFile = aptFile;
        }

        public static AptObjectsManager Load(string rootPath, string aptPath)
        {
            var fs = new FileSystem(rootPath);
            var entry = fs.GetFile(aptPath);
            return new AptObjectsManager(AptFile.FromFileSystemEntry(entry));
        }

        public void Edit(IEditAction editAction)
        {
            _redoStack.Clear();
            editAction.Edit();
            _undoStack.Push(editAction);
        }

        public string? GetUndoDescription()
        {
            throw new NotImplementedException();
            /*if (_undoStack.TryPeek(out var editAction))
            {
                return editAction.Description;
            }
            return null;*/
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
