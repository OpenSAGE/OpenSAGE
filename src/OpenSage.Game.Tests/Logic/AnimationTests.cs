using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using Xunit;

namespace OpenSage.Tests.Logic;

public class AnimationTests
{
    private static readonly List<LazyAssetReference<MappedImage>> Images = [FakeMappedImage(1), FakeMappedImage(2), FakeMappedImage(3), FakeMappedImage(4)];

    private static LazyAssetReference<MappedImage> FakeMappedImage(int id)
    {
        return new LazyAssetReference<MappedImage>(new MappedImage { InternalId = id });
    }

    [Fact]
    public void TestOnceForwards()
    {
        var template = new AnimationTemplate { AnimationMode = AnimationMode.Once, Images = Images, Name = "DefaultHeal" };
        var animation = new Animation(template);

        uint frameCount = 1;
        for (var i = 1; i <= Images.Count; i++)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            Assert.Equal(i, animation.Current.InternalId);
        }

        Assert.False(animation.SetFrame(frameCount), $"expected frame {frameCount} to be false but was true");
    }

    [Fact]
    public void TestOnceBackwards()
    {
        var template = new AnimationTemplate { AnimationMode = AnimationMode.OnceBackwards, Images = Images, Name = "DefaultHeal" };
        var animation = new Animation(template);

        uint frameCount = 1;
        for (var i = Images.Count; i > 0; i--)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            Assert.Equal(i, animation.Current.InternalId);
        }

        Assert.False(animation.SetFrame(frameCount), $"expected frame {frameCount} to be false but was true");
    }

    [Fact]
    public void TestLoopForwards()
    {
        var template = new AnimationTemplate { AnimationMode = AnimationMode.Loop, Images = Images, Name = "DefaultHeal" };
        var animation = new Animation(template);

        uint frameCount = 1;
        for (var i = 1; i < Images.Count * 3; i++)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            Assert.Equal((i - 1) % template.Images.Count + 1, animation.Current.InternalId);
        }
    }

    [Fact]
    public void TestLoopBackwards()
    {
        var template = new AnimationTemplate { AnimationMode = AnimationMode.LoopBackwards, Images = Images, Name = "DefaultHeal", };
        var animation = new Animation(template);

        uint frameCount = 1;
        for (var i = Images.Count * 3; i > 0; i--)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            // 12 -> 4
            // 11 -> 3
            //  9 -> 1
            //  8 -> 4
            var expected = i % Images.Count;
            expected = expected == 0 ? Images.Count : expected; // there's probably a fancier way to do this
            Assert.Equal(expected, animation.Current.InternalId);
        }
    }

    [Fact]
    public void TestPingPong()
    {
        var template = new AnimationTemplate { AnimationMode = AnimationMode.PingPong, Images = Images, Name = "DefaultHeal" };
        var animation = new Animation(template);

        uint frameCount = 1;
        for (var i = 1; i <= Images.Count; i++)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            Assert.Equal(i, animation.Current.InternalId);
        }

        for (var i = Images.Count - 1; i > 0; i--)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            Assert.Equal(i, animation.Current.InternalId);
        }

        for (var i = 2; i <= Images.Count; i++)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
            Assert.Equal(i, animation.Current.InternalId);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void TestPlayToEnd(int delay)
    {
        var lastDisplayedFrame = delay * Images.Count; // if I have 4 images, with a delay of 5 frames between images, I'll be done on frame 1 + 5 + 5 + 5 + 5 = 21
        var msDelay =(1000 / Game.LogicFramesPerSecond) * delay;
        var template = new AnimationTemplate { AnimationMode = AnimationMode.Once, Images = Images, Name = "DefaultHeal", AnimationDelay = (int)msDelay };
        var animation = new Animation(template);

        uint frameCount = 1;
        for (var i = 1; i <= lastDisplayedFrame; i++)
        {
            Assert.True(animation.SetFrame(frameCount++), $"expected frame {frameCount - 1} to be true but was false");
        }

        Assert.False(animation.SetFrame(frameCount), $"expected frame {frameCount} to be false but was true");
    }
}
