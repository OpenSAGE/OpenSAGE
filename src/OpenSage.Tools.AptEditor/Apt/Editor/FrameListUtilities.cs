using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Tools.AptEditor.UI;

namespace OpenSage.Tools.AptEditor.Apt.Editor
{

    internal class FrameListUtilities
    {
        public bool Active => _manager.CurrentCharacter is Playable;
        public IReadOnlyList<Frame> CurrentFrames => _storedFrames;
        private AptSceneManager _manager;
        private Character _currentCharacter => _manager.CurrentCharacter;
        private List<Frame> _storedFrames;

        // return true if they are new frames
        public bool Reset(AptSceneManager manager, bool force = false)
        {
            _manager = manager;
            if(!Active)
            {
                return false;
            }

            var currentFrames = ((Playable)_manager.CurrentCharacter).Frames;
            if(!force && ReferenceEquals(currentFrames, _storedFrames))
            {
                return false;
            }

            _storedFrames = currentFrames;
            return true;
        }

        public void DeleteFrame(int frameNumber)
        {
            if(_storedFrames.Count <= 1)
            {
                throw new AptEditorException(ErrorType.PlayableMustHaveAtLeastOneFrame);
            }
            
            // if it's last frame, switch to the previous frame first
            if(frameNumber == _storedFrames.Count - 1)
            {
                _manager.PlayToFrame(_storedFrames.Count - 1);
            }
            else
            {
                _manager.PlayToFrame(frameNumber);
            }

            var editAction = new NonSymmetricEditAction<(List<Frame>, Frame)>();
            editAction.Description = "Remove frame";
            editAction.Do = (_, framesAndFrame) =>
            {
                var (frames, frame) = framesAndFrame;
                frames.RemoveAt(frameNumber);
                return framesAndFrame;
            };
            editAction.Undo = (_, framesAndFrame) =>
            {
                var (frames, frame) = framesAndFrame;
                frames.Insert(frameNumber, frame);
                return framesAndFrame;
            };
            editAction.TargetValue = (_storedFrames, _storedFrames[frameNumber]);

            _manager.AptManager.Edit(editAction);
        }

        public void AppendFrame()
        {
            var editAction = new ListAddAction<Frame>(Frame.Create(new List<FrameItem>()));
            var editingFrames = _storedFrames;
            editAction.Description = "Add new frame";
            editAction.FindList = (_) => { return editingFrames; };

            _manager.AptManager.Edit(editAction);
        }

    }
}
