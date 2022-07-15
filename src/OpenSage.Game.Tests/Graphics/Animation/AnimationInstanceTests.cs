//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using OpenSage.Graphics;
//using OpenSage.Graphics.Animation;
//using OpenSage.Logic.Object;
//using Xunit;

//namespace OpenSage.Tests.Graphics.Animation
//{
//    public class AnimationInstanceTests
//    {
//        [Fact]
//        public void OnceForwardsTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.Once,
//                AnimationFlags.StartFrameFirst, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 0; i < 5; i++)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                Assert.Equal(i, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void OnceBackwardsTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.OnceBackwards,
//                AnimationFlags.StartFrameLast, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 4; i >= 0; i--)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                Assert.Equal(i, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void StartAtEndForwardsTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.Once,
//                AnimationFlags.StartFrameLast, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 4; i >= 0; i--)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                // we should always show the last frame
//                Assert.Equal(4, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void StartAtBeginningBackwardsTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.OnceBackwards,
//                AnimationFlags.StartFrameFirst, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 4; i >= 0; i--)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                // we should always show the first frame
//                Assert.Equal(0, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void StartAtBeginningManualTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.Manual,
//                AnimationFlags.StartFrameFirst, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 0; i < 5; i++)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                // always show the first frame, since the mode is manual
//                Assert.Equal(0, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void StartAtEndManualTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.Manual,
//                AnimationFlags.StartFrameLast, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 0; i < 5; i++)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                // always show the last frame
//                Assert.Equal(4, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void LoopForwardsTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.Loop,
//                AnimationFlags.StartFrameFirst, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 0; i < 15; i++)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                Assert.Equal(i % 5, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Fact]
//        public void LoopBackwardsTest()
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.LoopBackwards,
//                AnimationFlags.StartFrameLast, null, new QuoteUnquoteRandom());

//            instance.Play();
//            var time = TimeInterval.Zero;
//            for (var i = 14; i >= 0; i--)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                Assert.Equal(i % 5, modelBoneInstances[0].AnimatedOffset.Translation.X);
//            }
//        }

//        [Theory]
//        [InlineData(0)]
//        [InlineData(1111)]
//        [InlineData(2222)]
//        [InlineData(3333)]
//        [InlineData(4000)]
//        [InlineData(4001)]
//        [InlineData(5000)]
//        [InlineData(5001)]
//        public void OnceForwardsRandomStartTest(int rngSeed)
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();
//            var random = new QuoteUnquoteRandom(rngSeed);

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.Once,
//                AnimationFlags.RandomStart, null, random);

//            instance.Play();
//            var time = TimeInterval.Zero;
//            float? previous = null;
//            for (var i = 0; i < 5; i++)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));

//                if (previous is null)
//                {
//                    previous = modelBoneInstances[0].AnimatedOffset.Translation.X;
//                    continue;
//                }

//                var target = previous.Value + 1;
//                if (target > animation.Clips[0].Keyframes.Last().Value.FloatValue)
//                {
//                    target = animation.Clips[0].Keyframes.Last().Value.FloatValue;
//                }

//                Assert.Equal(target, modelBoneInstances[0].AnimatedOffset.Translation.X, new ToleranceEqualityComparer(0.001f));
//                previous = modelBoneInstances[0].AnimatedOffset.Translation.X;
//            }
//            instance.Stop();
//        }

//        [Theory]
//        [InlineData(0)]
//        [InlineData(1111)]
//        [InlineData(2222)]
//        [InlineData(3333)]
//        [InlineData(4000)]
//        [InlineData(4001)]
//        [InlineData(5000)]
//        [InlineData(5001)]
//        public void OnceBackwardsRandomStartTest(int rngSeed)
//        {
//            var modelBoneInstances = NewDefaultModelBoneInstances();
//            var animation = NewBasicAnimationInstance();
//            var random = new QuoteUnquoteRandom(rngSeed);

//            var instance = new AnimationInstance(modelBoneInstances, animation, AnimationMode.OnceBackwards,
//                AnimationFlags.RandomStart, null, random);

//            instance.Play();

//            var time = TimeInterval.Zero;
//            float? previous = null;
//            for (var i = 5; i > 0; i--)
//            {
//                instance.Update(time);
//                time = new TimeInterval(time.TotalTime + TimeSpan.FromSeconds(1),
//                    TimeSpan.FromSeconds(1));
//                if (previous is null)
//                {
//                    previous = modelBoneInstances[0].AnimatedOffset.Translation.X;
//                    continue;
//                }

//                var target = previous.Value - 1;
//                if (target < 0)
//                {
//                    target = 0;
//                }

//                Assert.Equal(target, modelBoneInstances[0].AnimatedOffset.Translation.X, new ToleranceEqualityComparer(0.001f));
//                previous = modelBoneInstances[0].AnimatedOffset.Translation.X;
//            }
//            instance.Stop();
//        }

//        private static ModelBoneInstance[] NewDefaultModelBoneInstances()
//        {
//            var testBone = new ModelBone(0, "myBone", null, Vector3.Zero, Quaternion.Identity);
//            return new[]
//            {
//                new ModelBoneInstance(testBone)
//            };
//        }

//        private static W3DAnimation NewBasicAnimationInstance()
//        {
//            var animationClips = new[]
//            {
//                new AnimationClip(AnimationClipType.TranslationX, 0, new []
//                {
//                    new Keyframe(TimeSpan.FromSeconds(0), new KeyframeValue
//                    {
//                        FloatValue = 0,
//                    }),
//                    new Keyframe(TimeSpan.FromSeconds(1), new KeyframeValue
//                    {
//                        FloatValue = 1,
//                    }),
//                    new Keyframe(TimeSpan.FromSeconds(2), new KeyframeValue
//                    {
//                        FloatValue = 2,
//                    }),
//                    new Keyframe(TimeSpan.FromSeconds(3), new KeyframeValue
//                    {
//                        FloatValue = 3,
//                    }),
//                    new Keyframe(TimeSpan.FromSeconds(4), new KeyframeValue
//                    {
//                        FloatValue = 4,
//                    }),
//                })
//            };

//            return new W3DAnimation(string.Empty, TimeSpan.FromSeconds(5), animationClips);
//        }
//    }

//    /// <summary>
//    /// Guaranteed random!
//    /// </summary>
//    internal class QuoteUnquoteRandom : Random
//    {
//        private readonly int _random;

//        /// <summary>
//        /// Constructs a new "random" number generator which will always return 0.
//        /// </summary>
//        public QuoteUnquoteRandom() : this(0)
//        {
//        }

//        /// <summary>
//        /// Constructs a new "random" number generator.
//        /// </summary>
//        /// <param name="random">The "random" value which should always be returned.</param>
//        public QuoteUnquoteRandom(int random)
//        {
//            _random = random;
//        }

//        public override int Next()
//        {
//            return _random;
//        }

//        public override int Next(int maxValue)
//        {
//            return Next();
//        }

//        public override int Next(int minValue, int maxValue)
//        {
//            return Next(minValue);
//        }
//    }

//    internal class ToleranceEqualityComparer : IEqualityComparer<float>
//    {
//        private readonly float _tolerance;

//        public ToleranceEqualityComparer(float tolerance)
//        {
//            _tolerance = tolerance;
//        }

//        public bool Equals(float x, float y)
//        {
//            return Math.Abs(x - y) <= _tolerance;
//        }

//        public int GetHashCode(float obj)
//        {
//            return obj.GetHashCode();
//        }
//    }
//}
