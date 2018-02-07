using System.Collections.Generic;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleSystemSystem : GameSystem
    {
        private readonly List<AttachedParticleSystem> _deadParticleSystems;

        public ParticleSystemSystem(Game game)
            : base(game)
        {
            _deadParticleSystems = new List<AttachedParticleSystem>();

            switch (game.SageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.BattleForMiddleEarth:
                case SageGame.BattleForMiddleEarthII:
                    game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\ParticleSystem.ini");
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Game.Scene3D == null)
            {
                return;
            }

            // TODO: This could be more efficient if we knew upfront about all particle systems.
            foreach (var attachedParticleSystem in Game.Scene3D.GetAllAttachedParticleSystems())
            {
                var particleSystem = attachedParticleSystem.ParticleSystem;

                particleSystem.Update(gameTime);

                if (particleSystem.State == ParticleSystemState.Dead)
                {
                    _deadParticleSystems.Add(attachedParticleSystem);
                }
            }

            foreach (var deadParticleSystem in _deadParticleSystems)
            {
                deadParticleSystem.Detach();
            }

            _deadParticleSystems.Clear();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            if (Game.Scene3D == null)
            {
                return;
            }

            // TODO: Keep particle count under GameData.MaxParticleCount
            foreach (var attachedParticleSystem in Game.Scene3D.GetAllAttachedParticleSystems())
            {
                var worldMatrix = attachedParticleSystem.ParticleSystem.GetWorldMatrix();

                attachedParticleSystem.ParticleSystem.BuildRenderList(
                    renderList,
                    worldMatrix);
            }
        }
    }
}
