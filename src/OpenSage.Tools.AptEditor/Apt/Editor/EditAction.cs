using System;
using OpenSage.Data.Apt;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    /// <summary>
    /// An Edit Action which performs edit by calling <see cref="Edit()"/>, 
    /// and its effect can be reversed by calling <see cref="Undo()"/>.
    /// </summary>
    public interface IEditAction
    {
        string? Description { get; }
        public event EventHandler? OnEdit;
        void Edit();
        void Undo();
        bool TryMerge(IEditAction edit);
    }

    public sealed class EditAction : IEditAction
    {
        public string? Description { get; }
        public event EventHandler? OnEdit;
        private readonly Action _edit;
        private readonly Action _undo;

        public EditAction(Action edit, Action undo, string? description = null)
        {
            Description = description;
            _edit = edit;
            _undo = undo;
        }

        public void Edit()
        {
            _edit();
            OnEdit?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            _undo();
            OnEdit?.Invoke(this, EventArgs.Empty);
        }

        public bool TryMerge(IEditAction edit) => false;
    }

    public sealed class EditAction<T> : IEditAction
    {
        public string? Description { get; set; }
        public event EventHandler? OnEdit;
        /// <summary>
        /// Executes the edit action, <see cref="AptFile"/> and
        /// <see cref="_state"/> will be passed as parameters.
        /// Its return value is stored inside <see cref="_state"/> again.
        /// </summary>
        private readonly Func<T, T> _edit;
        private readonly Func<T, T> _undo;
        private T _state;

        public EditAction(Action<T> edit, Action<T> undo, T state, string? description = null)
        {
            Description = description;
            _edit = s => { edit(s); return s; };
            _undo = s => { undo(s); return s; };
            _state = state;
        }

        public EditAction(Func<T, T> edit, Func<T, T> undo, T state, string? description = null)
        {
            Description = description;
            _edit = edit;
            _undo = undo;
            _state = state;
        }

        public EditAction(Func<T, T> editOrUndo, T state, string? description = null)
        {
            Description = description;
            _edit = editOrUndo;
            _undo = editOrUndo;
            _state = state;
        }

        public void Edit()
        {
            _state = _edit(_state);
            OnEdit?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            _state = _undo(_state);
            OnEdit?.Invoke(this, EventArgs.Empty);
        }

        public bool TryMerge(IEditAction edit) => false;
    }

    public sealed class MergeableEdit : IEditAction
    {
        public string? Description { get; set; }
        public DateTimeOffset Time { get; private set; }
        public TimeSpan Threshold { get; set; }
        public string EditTypeId { get; }

        public event EventHandler? OnEdit;
        private bool _valid;
        private Action _edit;
        private readonly Action _restore;

        public MergeableEdit(TimeSpan threshold, string editTypeId, Action edit, Action restore, string? description = null)
        {
            Description = description;
            Time = DateTimeOffset.UtcNow;
            Threshold = threshold;
            EditTypeId = editTypeId;
            _valid = true;
            _edit = edit;
            _restore = restore;
            
        }

        public void Edit()
        {
            CheckState();
            _edit();
            OnEdit?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            CheckState();
            _restore();
            OnEdit?.Invoke(this, EventArgs.Empty);
        }

        public bool TryMerge(IEditAction edit)
        {
            if (edit is not MergeableEdit other)
            {
                return false;
            }
            CheckState();
            other.CheckState();
            var interval = other.Time - Time;
            if (interval.Ticks < 0)
            {
                throw new InvalidOperationException();
            }
            if (interval > Threshold)
            {
                return false;
            }
            if (EditTypeId != other.EditTypeId)
            {
                return false;
            }
            Time += (interval * 0.75);
            Threshold = (Threshold + other.Threshold) / 2;
            _edit = other._edit;
            OnEdit = other.OnEdit;
            other._valid = false;
            return true;
        }

        private void CheckState()
        {
            if(!_valid)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
