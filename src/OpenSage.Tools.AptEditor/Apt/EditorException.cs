using System;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    public enum ErrorType
    {
        Unspecified,
        CannotAddExistingCharacter,
        CannotDisableCharacterInUse,
        IncorrectValueType,
        PlayableMustHaveAtLeastOneFrame,
        FailedToParseGeometry,
        InvalidTextureIdInGeometry,
        PlaceObjectDepthAlreadyTaken,
        InvalidPlaceObjectDepth,
        InvalidCharacterId,
        PlaceObjectCircularDependency,
        InvalidCharacterInDependency
    }

    public class AptEditorException : Exception
    {
        public ErrorType ErrorType { get; }
        public AptEditorException(ErrorType type, Exception innerException) :
            base(innerException.Message, innerException)
        {
            ErrorType = type;
        }

        public AptEditorException(ErrorType type, string message = "AptEditorException") : base(message)
        {
            ErrorType = type;
        }
    }
}
