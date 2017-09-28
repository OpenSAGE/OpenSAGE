using System;
using System.Collections.Generic;
using System.Numerics;
using Caliburn.Micro;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel, IRenderableViewModel
    {
        private readonly Map _map;

        private readonly GameTimer _gameTimer;

        private readonly Camera _camera;

        public MapCameraController CameraController { get; }

        CameraController IRenderableViewModel.CameraController => CameraController;

        public IEnumerable<TimeOfDay> TimesOfDay
        {
            get
            {
                yield return TimeOfDay.Morning;
                yield return TimeOfDay.Afternoon;
                yield return TimeOfDay.Evening;
                yield return TimeOfDay.Night;
            }
        }

        public TimeOfDay CurrentTimeOfDay
        {
            get { return _map.CurrentTimeOfDay; }
            set
            {
                _map.CurrentTimeOfDay = value;
                NotifyOfPropertyChange();
            }
        }

        public bool RenderWireframeOverlay
        {
            get { return _map.Terrain.RenderWireframeOverlay; }
            set
            {
                _map.Terrain.RenderWireframeOverlay = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mousePosition;
        public string MousePosition
        {
            get => _mousePosition;
            set
            {
                _mousePosition = value;
                NotifyOfPropertyChange();
            }
        }

        private string _tilePosition;
        public string TilePosition
        {
            get => _tilePosition;
            set
            {
                _tilePosition = value;
                NotifyOfPropertyChange();
            }
        }

        private string _hoveredObjectInfo;
        public string HoveredObjectInfo
        {
            get => _hoveredObjectInfo;
            set
            {
                _hoveredObjectInfo = value;
                NotifyOfPropertyChange();
            }
        }

        private DateTime _nextFrameTimeUpdate = DateTime.MinValue;

        private string _frameTime;
        public string FrameTime
        {
            get => _frameTime;
            set
            {
                _frameTime = value;
                NotifyOfPropertyChange();
            }
        }

        public MapFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            var gameContext = AddDisposable(new GameContext(
                file.FileSystem, graphicsDevice));

            var mapFile = MapFile.FromFileSystemEntry(file);

            _map = AddDisposable(new Map(
                mapFile,
                gameContext));

            _camera = new Camera();
            _camera.FieldOfView = 70;

            CameraController = new MapCameraController(_camera);
            CameraController.Reset(_map.Terrain.BoundingBox.GetCenter());

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();
        }

        public void OnMouseMove(int x, int y)
        {
            if (_map == null)
            {
                return;
            }

            var ray = _camera.ScreenPointToRay(new Vector2(x, y));

            var intersectionPoint = _map.Terrain.Intersect(ray);

            if (intersectionPoint != null)
            {
                MousePosition = $"Pos: ({intersectionPoint.Value.X.ToString("F1")}, {intersectionPoint.Value.Y.ToString("F1")}, {intersectionPoint.Value.Z.ToString("F1")})";

                var tilePosition = _map.Terrain.HeightMap.GetTilePosition(intersectionPoint.Value);
                if (tilePosition != null)
                {
                    TilePosition = $"Tile: ({tilePosition.Value.X}, {tilePosition.Value.Y})";
                }
                else
                {
                    TilePosition = "Tile: No intersection";
                }
            }
            else
            {
                MousePosition = "Pos: No intersection";
                TilePosition = "Tile: No intersection";
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain, RenderPassDescriptor renderPassDescriptor)
        {
            _gameTimer.Update();

            _map.Update(_gameTimer.CurrentGameTime);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            _camera.Viewport = new Viewport(
                0,
                0,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            commandEncoder.SetViewport(_camera.Viewport);

            _map.Draw(commandEncoder, _camera, _gameTimer.CurrentGameTime);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);

            //_gameTimer.Update();

            //var now = DateTime.Now;
            //if (now > _nextFrameTimeUpdate)
            //{
            //    FrameTime = $"{_gameTimer.CurrentGameTime.ElapsedGameTime.TotalMilliseconds}ms";
            //    _nextFrameTimeUpdate = now.AddSeconds(1);
            //}
        }
    }
}
