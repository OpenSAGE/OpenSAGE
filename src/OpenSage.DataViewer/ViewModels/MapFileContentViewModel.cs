using System;
using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel
    {
        private readonly MapFile _mapFile;
        private Map _map;

        private readonly GameTimer _gameTimer;

        private DepthStencilBuffer _depthStencilBuffer;

        private readonly Camera _camera;

        public MapCameraController CameraController { get; }

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
            get { return _map?.CurrentTimeOfDay ?? TimeOfDay.Morning; }
            set
            {
                _map.CurrentTimeOfDay = value;
                NotifyOfPropertyChange();
            }
        }

        public bool RenderWireframeOverlay
        {
            get { return _map?.Terrain.RenderWireframeOverlay ?? false; }
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
            _mapFile = MapFile.FromFileSystemEntry(file);

            _camera = new Camera();
            _camera.FieldOfView = 70;

            CameraController = new MapCameraController(_camera);

            _gameTimer = AddDisposable(new GameTimer());
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

        private void EnsureDepthStencilBuffer(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            if (_depthStencilBuffer != null
                && _depthStencilBuffer.Width == swapChain.BackBufferWidth
                && _depthStencilBuffer.Height == swapChain.BackBufferHeight)
            {
                return;
            }

            if (_depthStencilBuffer != null)
            {
                _depthStencilBuffer.Dispose();
                _depthStencilBuffer = null;
            }

            _depthStencilBuffer = new DepthStencilBuffer(
                graphicsDevice,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            _camera.Viewport = new Viewport(
                0, 
                0, 
                swapChain.BackBufferWidth, 
                swapChain.BackBufferHeight);
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _map = AddDisposable(new Map(
                _mapFile, 
                File.FileSystem,
                graphicsDevice));

            CameraController.Reset(_map.Terrain.BoundingBox.GetCenter());

            NotifyOfPropertyChange(nameof(CurrentTimeOfDay));
            NotifyOfPropertyChange(nameof(RenderWireframeOverlay));

            _gameTimer.Start();
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _gameTimer.Update();

            _map.Update(_gameTimer.CurrentGameTime);

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            EnsureDepthStencilBuffer(graphicsDevice, swapChain);
            renderPassDescriptor.SetDepthStencilDescriptor(_depthStencilBuffer);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(_camera.Viewport);

            _map.Draw(commandEncoder, _camera);

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
