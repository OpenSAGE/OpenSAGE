using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Cameras
{
    public class CameraSystem : DisposableBase
    {
        private CameraInputState _cameraInputState;
        private readonly CameraInputMessageHandler _cameraInputMessageHandler;
        private readonly GamePanel Panel;
        public readonly Camera Camera;
        public readonly ICameraController Controller;


        private bool _cameraEnableInput;
        public bool EnableInput
        { 
            get => _cameraEnableInput;
            set
            {
                if (value && !_cameraEnableInput)
                {
                    _cameraInputMessageHandler.ResetMouse(Panel);
                }
                _cameraEnableInput = value;
            }
        }

        private void RegisterInputHandler(InputMessageHandler handler, InputMessageBuffer inputMessageBuffer)
        {
            inputMessageBuffer.Handlers.Add(handler);
            AddDisposeAction(() => inputMessageBuffer.Handlers.Remove(handler));
        }

        private CameraSystem(Game game, Func<Viewport> getViewport, InputMessageBuffer inputMessageBuffer)
        {
            Camera = new Camera(getViewport);
            Panel = game.Panel;
            _cameraInputMessageHandler = new CameraInputMessageHandler();
            RegisterInputHandler(_cameraInputMessageHandler, inputMessageBuffer);
        }

        public CameraSystem(
            Game game, 
            ICameraController controller, 
            Func<Viewport> getViewport, 
            InputMessageBuffer inputMessageBuffer)
            : this(game, getViewport, inputMessageBuffer)
        {
            Controller = controller;
        }

        public CameraSystem(
            Game game,
            Terrain.Terrain terrain,
            Func<Viewport> getViewport,
            InputMessageBuffer inputMessageBuffer)
            : this(game, getViewport, inputMessageBuffer)
        {
            Controller = new RtsCameraController(
                game.AssetStore.GameData.Current, Camera, terrain.HeightMap, game.Panel)
                {
                    TerrainPosition = terrain.HeightMap.GetPosition(
                        terrain.HeightMap.Width / 2,
                        terrain.HeightMap.Height / 2)
                };
        }

        public void LogicTick(in TimeInterval gameTime)
        {
            _cameraInputMessageHandler?.UpdateInputState(ref _cameraInputState);

            if (EnableInput)
            {
                Controller.UpdateInput(_cameraInputState, gameTime);
            }
            Controller.UpdateCamera(Camera, gameTime);
        }
    }
}