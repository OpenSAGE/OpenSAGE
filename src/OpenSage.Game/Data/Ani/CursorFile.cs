using System;
using OpenSage.IO;

namespace OpenSage.Data.Ani
{
    /// <summary>
    /// An abstraction on top of <see cref="AniFile"/> and <see cref="CurFile"/>.
    /// </summary>
    public sealed class CursorFile
    {
        public static CursorFile FromFileSystemEntry(FileSystemEntry entry)
        {
            if (entry.FilePath.EndsWith(".ani"))
            {
                var aniFile = AniFile.FromFileSystemEntry(entry);

                var jiffy = TimeSpan.FromSeconds(1 / 60.0);

                CursorAnimationFrame[] animationFrames;
                if (aniFile.Sequence != null)
                {
                    animationFrames = new CursorAnimationFrame[aniFile.Sequence.FrameIndices.Length];

                    for (var i = 0; i < animationFrames.Length; i++)
                    {
                        var frameDuration = aniFile.Rates != null
                            ? aniFile.Rates.Durations[i]
                            : aniFile.DefaultFrameDisplayRate;

                        animationFrames[i] = new CursorAnimationFrame(
                            aniFile.Sequence.FrameIndices[i],
                             frameDuration * jiffy);
                    }
                }
                else
                {
                    animationFrames = Array.Empty<CursorAnimationFrame>();
                }

                return new CursorFile(
                    aniFile.Images,
                    animationFrames);
            }
            else if (entry.FilePath.EndsWith(".cur"))
            {
                var curFile = CurFile.FromFileSystemEntry(entry);

                return new CursorFile(
                    new[] { curFile.Image },
                    Array.Empty<CursorAnimationFrame>());
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public readonly CursorImage[] Images;

        public readonly CursorAnimationFrame[] AnimationFrames;

        private CursorFile(CursorImage[] images, CursorAnimationFrame[] animationFrames)
        {
            Images = images;
            AnimationFrames = animationFrames;
        }
    }
}
