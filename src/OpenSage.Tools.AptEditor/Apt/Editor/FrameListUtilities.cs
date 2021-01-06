using System;
using System.Collections.Generic;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Tools.AptEditor.UI;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{
    internal class FrameListUtilities
    {
        public bool Active => Frames != null;
        public Playable? CurrentCharacter { get; private set; }
        public List<Frame>? Frames => CurrentCharacter?.Frames;
        private AptSceneManager? _manager;

        // return true if they are new frames
        public bool Reset(AptSceneManager manager)
        {
            _manager ??= manager;
            if (_manager != manager)
            {
                throw new NotSupportedException();
            }

            var newCharacter = _manager?.CurrentCharacter as Playable;
            if (CurrentCharacter != newCharacter)
            {
                CurrentCharacter = newCharacter;
                return CurrentCharacter != null;
            }

            return false;
        }

        public void DeleteFrame(int frameNumber)
        {
            if (Frames is null)
            {
                throw new InvalidOperationException();
            }

            if (Frames.Count <= 1)
            {
                throw new AptEditorException(ErrorType.PlayableMustHaveAtLeastOneFrame);
            }

            // if it's last frame, switch to the previous frame first
            var nextFrame = frameNumber == Frames.Count - 1
                ? Frames.Count - 1
                : frameNumber;
            _manager!.PlayToFrame(nextFrame);

            var frameToBeRemoved = Frames[frameNumber];
            var editAction = new EditAction(() => Frames.RemoveAt(frameNumber),
                                            () => Frames.Insert(frameNumber, frameToBeRemoved),
                                            "Remove frame");
            _manager.AptManager!.Edit(editAction);
        }

        public void AppendFrame()
        {
            if (Frames is null)
            {
                throw new InvalidOperationException();
            }

            var newFrame = Frame.Create(new List<FrameItem>());
            var editAction = new EditAction(() => Frames.Add(newFrame),
                                            () => Frames.RemoveAt(Frames.Count - 1),
                                            "Add new frame");
            _manager!.AptManager!.Edit(editAction);
        }

    }
}
