using System;
using System.Collections.Generic;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    public enum ErrorType
    {
        Unspecified,
        CannotAddExistingCharacter,
        CannotDisableCharacterInUse,
        IncorrectValueType,
        PlayableMustHaveAtLeastOneFrame,
    }

    public class AptEditorException : Exception
    {
        public readonly ErrorType ErrorType;
        public virtual bool CanBeDownCasted { get { return false; } }
        public AptEditorException(ErrorType type) : base("AptEditorException") {
            ErrorType = type;
        }
    }
}