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


    public class MergedEditAction : IEditAction
    {
        public string? CustomDescription { get; set; }
        public string? Description { get
            {
                if (!string.IsNullOrEmpty(CustomDescription))
                    return CustomDescription;
                else if (Actions.Count == 0)
                    return "Empty Action";
                else if (Actions.Count == 1)
                    return Actions.First().Description;
                else
                    return $"{Actions.First().Description}...{Actions.Last().Description}";
            } }

        public event EventHandler? OnEdit;

        public readonly List<IEditAction> Actions = new();

        public void Edit()
        {
            foreach (var ea in Actions)
                ea.Edit();
        }

        public bool TryMerge(IEditAction edit) => false;

        public void Undo()
        {
            foreach (var ea in Actions.Reverse<IEditAction>())
                ea.Undo();
        }
    }

    public class EditManager
    {
        private readonly Stack<IEditAction> _undoStack = new();
        private readonly Stack<IEditAction> _redoStack = new();
        public EditManager()
        {
            _mergeActions = false;
            _actions = new();
        }

        private bool _mergeActions;
        private MergedEditAction _actions;
        protected bool ForceMergeActions
        {
            get { return _mergeActions; }
            set
            {
                if (!value)
                {
                    _undoStack.Push(_actions);
                    _actions = new();
                }
                _mergeActions = value;
            }
        }

        protected string? CustomMergedActionsDescription
        {
            get { return _actions.CustomDescription; }
            set { _actions.CustomDescription = value; }
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
                if (ForceMergeActions)
                    _actions.Actions.Add(editAction);
                else
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
                if (ForceMergeActions)
                    _actions.Actions.Add(editAction);
                else
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
            if (ForceMergeActions)
                throw new InvalidOperationException("Please stop merging first");

            var editAction = _undoStack.Pop();
            editAction.Undo();
            _redoStack.Push(editAction);
        }

        public void Redo()
        {
            if (ForceMergeActions)
                throw new InvalidOperationException("Please stop merging first");

            var editAction = _redoStack.Pop();
            editAction.Edit();
            _undoStack.Push(editAction);
        }
    }
    
}
