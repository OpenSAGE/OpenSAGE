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
        void Edit();
        void Undo();
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

        public void Edit() => _edit();

        public void Undo() => _undo();
    }

    public sealed class EditAction<T> : IEditAction
    {
        public string? Description { get; }
        public event EventHandler? OnEdit;
        /// <summary>
        /// Executes the edit action, <see cref="AptFile"/> and
        /// <see cref="_state"/> will be passed as parameters.
        /// Its return value is stored inside <see cref="_state"/> again.
        /// </summary>
        private readonly Func<T, T> _edit;
        private readonly Func<T, T> _undo;
        private T _state;

        public EditAction(Action edit, Action undo, string? description = null)
        {
            Description = description;
            _edit = _ => { edit(); return default!; };
            _undo = _ => { undo(); return default!; };
            _state = default!;
        }

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
    }
}
