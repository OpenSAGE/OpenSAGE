using System.Collections.Generic;

namespace OpenSage
{
    public sealed class UpdateableSystem : GameSystem
    {
        private readonly List<UpdateableComponent> _updateables;

        public UpdateableSystem(Game game) 
            : base(game)
        {
            RegisterComponentList(_updateables = new List<UpdateableComponent>());
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var updateable in _updateables)
            {
                updateable.Update(gameTime);
            }
        }
    }
}
