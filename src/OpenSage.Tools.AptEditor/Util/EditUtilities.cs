using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Tools.AptEditor.Util
{
    class EditUtilities
    {
    }

    public class EditManager
    {
        private readonly Stack<IEditAction> _undoStack = new();
        private readonly Stack<IEditAction> _redoStack = new();
        public EditManager()
        {

        }
        protected void PushActionNoEdit(IEditAction editAction)
        {
            _redoStack.Clear();
            if (_undoStack.TryPeek(out var previous) && previous.TryMerge(editAction))
            {
                // do nothing
            }
            else
            {
                _undoStack.Push(editAction);
            }
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
    }
    
}
