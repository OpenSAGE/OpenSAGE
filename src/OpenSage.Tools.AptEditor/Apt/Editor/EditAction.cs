using System;
using System.Collections.Generic;
using OpenSage.Data.Apt;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    /// <summary>
    /// An Edit Action which performs edit by calling `Execute`, 
    /// and its effect can be reversed by calling `Execute` again.
    /// </summary>
    public interface IEditAction
    {
        string Description { get; }
        void Execute(AptFile aptFile);
    }

    /// <summary>
    /// An IEditAction which can be reversed by simply executing the same function again
    /// </summary>
    public class SymmetricEditAction<T> : IEditAction
    {
        public string Description { get; set; }

        /// <summary>
        /// Executes the edit action, `AptFile` and `TargetValue` will be passed as parameters.
        /// Its return value is stored inside `TargetValue` again, so when it's executed again,
        /// the edit action will be reversed.
        /// </summary>
        public Func<AptFile, T, T> AssignAndReturnPrevious;
        public T TargetValue;

        public void Execute(AptFile aptFile)
        {
            TargetValue = AssignAndReturnPrevious(aptFile, TargetValue);
        }
    }

    /// <summary>
    /// An IEditAction which uses two different functions `Do` and `Undo`
    /// To edit or reverse the edit of Apt File.
    /// </summary>
    public class NonSymmetricEditAction<T> : IEditAction
    {
        public string Description { get; set; }

        /// <summary>
        /// Executes the edit action, `AptFile` and `TargetValue` will be passed as parameters.
        /// Its return value is stored inside `TargetValue` again.
        /// After execution, `Do` and `Undo` will be swapped, so the next call to `Execute` will 
        /// reverse the changes.
        /// </summary>
        public Func<AptFile, T, T> Do;

        /// <summary>
        /// Reverses the edit action, `AptFile` and `TargetValue` will be passed as parameters.
        /// Its return value is stored inside `TargetValue` again.
        /// After execution, `Do` and `Undo` will be swapped, so the next call to `Execute` will 
        /// Redo the changes.
        /// </summary>
        public Func<AptFile, T, T> Undo;
        public T TargetValue;
        public void Execute(AptFile aptFile)
        {
            TargetValue = Do(aptFile, TargetValue);
            var temp = Undo;
            Undo = Do;
            Do = temp;
        }
    }

    public class ListAddAction<T> : IEditAction
    {
        public string Description { get; set; }
        public Func<AptFile, List<T>> FindList;
        public Action<AptFile, List<T>> BeforeDo;
        public Action<AptFile, List<T>> AfterDo;
        public Action<AptFile, List<T>> BeforeUndo;
        public Action<AptFile, List<T>> AfterUndo;
        private T _targetValue;
        private NonSymmetricEditAction<T> _underlyingAction;

        public ListAddAction(T targetValue)
        {
            _targetValue = targetValue;
        }

        public void Execute(AptFile aptFile)
        {
            if(_underlyingAction == null)
            {
                _underlyingAction = new NonSymmetricEditAction<T>();
                _underlyingAction.Do = (apt, value) =>
                {
                    var list = FindList(apt);
                    BeforeDo?.Invoke(apt, list);
                    list.Add(value);
                    AfterDo?.Invoke(apt, list);
                    return value;
                };
                _underlyingAction.Undo = (apt, value) =>
                {
                    var list = FindList(apt);
                    BeforeUndo?.Invoke(apt, list);
                    list.RemoveAt(list.Count - 1);
                    AfterUndo?.Invoke(apt, list);
                    return value;
                };
                _underlyingAction.TargetValue = _targetValue;
            }
        }
    }
}