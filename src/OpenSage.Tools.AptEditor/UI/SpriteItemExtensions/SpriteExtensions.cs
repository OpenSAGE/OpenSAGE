using System;
using System.Collections.Generic;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Gui.Apt;
using Action = OpenSage.Data.Apt.FrameItems.Action;

namespace OpenSage.Tools.AptEditor.UI.SpriteItemExtensions
{
    internal static class SpriteExtensions
    {
        // Play frames without executing actions, since currently we can't handle all actions properly anyway.
        public static void PlayToFrameNoActions(this SpriteItem sprite, int frameNumber)
        {
            // reset to initial state
            sprite.Reset();

            if (frameNumber > sprite.GetFrames().Count)
            {
                frameNumber = sprite.GetFrames().Count;
            }

            while (sprite.CurrentFrame <= frameNumber)
            {
                sprite.UpdateNextFrameNoActions();
            }
        }

        public static void Reset(this DisplayItem display)
        {
            // reset to initial state
            display.Create(display.Character, display.Context, display.Parent);
            // reset all subitems
            if (display is SpriteItem sprite)
            {
                foreach (var item in sprite.Content.Items.Values)
                {
                    item.Reset();
                }
            }
        }

        private static void UpdateNextFrameNoActions(this SpriteItem sprite)
        {
            //get the current frame
            var frame = sprite.GetFrames()[sprite.CurrentFrame];

            //process all frame items, except labels and actions
            foreach (var item in frame.FrameItems)
            {
                switch (item)
                {
                    case FrameLabel frameLabel:
                    case Action action: // no actions
                        break;
                    default:
                        sprite.HandleFrameItem(item);
                        break;
                }
            }

            if (sprite.State == PlayState.PLAYING)
            {
                sprite.NextFrame();

                //reset to the start, we are looping by default
                if (sprite.CurrentFrame >= sprite.GetFrames().Count)
                {
                    sprite.GotoFrame(0);
                }
            }
            else if (sprite.State == PlayState.PENDING_STOPPED)
            {
                sprite.Stop(false);
            }

            //update all subItems
            foreach (var item in sprite.Content.Items.Values)
            {
                switch (item)
                {
                    case SpriteItem childSprite:
                        childSprite.UpdateNextFrameNoActions();
                        break;
                    case ButtonItem button:
                    case RenderItem render:
                        // currently these item's Update does nothing
                        item.Update(TimeInterval.Zero);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private static List<Frame> GetFrames(this SpriteItem sprite)
        {
            return ((Playable) sprite.Character).Frames;
        }
    }
}
