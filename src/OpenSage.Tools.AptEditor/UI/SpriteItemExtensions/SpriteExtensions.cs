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

            for(var i = 0; i <= frameNumber; ++i)
            {
                sprite.UpdateNextFrameNoActions();
            }
        }

        public static void Reset(this DisplayItem display)
        {
            // reset to initial state
            display.Create(display.Character, display.Context, display.Parent);
            if (display is SpriteItem sprite)
            {
                // stop the sprite, otherwise OpenSage may execute some
                // actionscript which we might not be able to handle yet
                sprite.Stop();

                // reset all subitems
                foreach (var item in sprite.Content.Items.Values)
                {
                    item.Reset();
                }
            }
        }

        public static void UpdateNextFrameNoActions(this SpriteItem sprite)
        {
            //get the current frame
            var frame = sprite.GetFrames()[sprite.CurrentFrame];

            //process all frame items, except labels and actions
            foreach (var item in frame.FrameItems)
            {
                switch (item)
                {
                    case FrameLabel _:
                    case Action _: // no actions
                        break;
                    default:
                        sprite.HandleFrameItem(item);
                        break;
                }
            }

            sprite.NextFrame();
            //reset to the start, we are looping by default
            if (sprite.CurrentFrame >= sprite.GetFrames().Count)
            {
                sprite.GotoFrame(0);
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

            sprite.Stop();
        }

        private static List<Frame> GetFrames(this SpriteItem sprite)
        {
            return ((Playable) sprite.Character).Frames;
        }
    }
}
