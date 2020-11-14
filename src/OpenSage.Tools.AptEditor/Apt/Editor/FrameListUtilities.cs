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
        public bool Reset(AptSceneManager manager, bool force = false)
        {
            _manager ??= manager;
            if(_manager != manager)
            {
                throw new NotSupportedException();
            }

            var newCharacter = _manager?.CurrentCharacter as Playable;
            if(CurrentCharacter != newCharacter)
            {
                CurrentCharacter = newCharacter;
                return CurrentCharacter != null;
            }

            return false;
        }

        public void DeleteFrame(int frameNumber)
        {
            if(Frames is null)
            {
                throw new InvalidOperationException();
            }

            if(Frames.Count <= 1)
            {
                throw new AptEditorException(ErrorType.PlayableMustHaveAtLeastOneFrame);
            }

            // if it's last frame, switch to the previous frame first
            var nextFrame = frameNumber == Frames.Count - 1
                ? Frames.Count - 1
                : frameNumber;
            _manager!.PlayToFrame(nextFrame);

            var editAction = new NonSymmetricEditAction<(List<Frame>, Frame)>
            {
                Description = "Remove frame",
                Do = (_, framesAndFrame) =>
                {
                    var (frames, frame) = framesAndFrame;
                    frames.RemoveAt(frameNumber);
                    return framesAndFrame;
                },
                Undo = (_, framesAndFrame) =>
                {
                    var (frames, frame) = framesAndFrame;
                    frames.Insert(frameNumber, frame);
                    return framesAndFrame;
                },
                TargetValue = (Frames, Frames[frameNumber])
            };

            _manager.AptManager!.Edit(editAction);
        }

        public void AppendFrame()
        {
            if (Frames is null)
            {
                throw new InvalidOperationException();
            }

            var editAction = new ListAddAction<Frame>(Frame.Create(new List<FrameItem>()));
            var editingFrames = Frames;
            editAction.Description = "Add new frame";
            editAction.FindList = (_) => { return editingFrames; };

            _manager!.AptManager!.Edit(editAction);
        }

    }
}
