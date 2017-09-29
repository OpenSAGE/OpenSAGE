using System.Collections.Generic;

namespace OpenSage.Graphics.Animation
{
    public sealed class AnimationSystem : GameSystem
    {
        private readonly List<AnimationComponent> _animations;

        public AnimationSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_animations = new List<AnimationComponent>());
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var animation in _animations)
            {
                animation.Update(gameTime);
            }
        }
    }
}
