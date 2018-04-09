﻿using System;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly RenderContext _renderContext;

        private RenderPipeline _renderPipeline;

        public GraphicsSystem(Game game)
            : base(game)
        {
            _renderContext = new RenderContext();
        }

        public override void Initialize()
        {
            Game.Panel.FramebufferChanged += OnFramebufferChanged;

            OnFramebufferChanged(this, EventArgs.Empty);

            base.Initialize();
        }

        private void OnFramebufferChanged(object sender, EventArgs e)
        {
            RemoveAndDispose(ref _renderPipeline);

            _renderPipeline = AddDisposable(new RenderPipeline(Game));
        }

        public override void Draw(GameTime gameTime)
        {
            _renderContext.Game = Game;
            _renderContext.GraphicsDevice = Game.GraphicsDevice;
            _renderContext.Graphics = this;
            _renderContext.Camera = Game.Scene3D?.Camera;
            _renderContext.Scene = Game.Scene3D;
            _renderContext.RenderTarget = Game.Panel.Framebuffer;
            _renderContext.GameTime = gameTime;

            _renderPipeline.Execute(_renderContext);

            base.Draw(gameTime);
        }
    }
}
