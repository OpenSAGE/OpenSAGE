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
        FailedToParseGeometry
    }

    public class AptEditorException : Exception
    {
        public ErrorType ErrorType { get; }
        public AptEditorException(ErrorType type, Exception innerException) :
            base("AptEditorException", innerException)
        {
            ErrorType = type;
        }

        public AptEditorException(ErrorType type) : base("AptEditorException")
        {
            ErrorType = type;
        }
    }
}
