using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.DataViewer.Framework;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel
    {
        private readonly MapFile _mapFile;
        private Map _map;

        private DepthStencilBuffer _depthStencilBuffer;

        private Matrix4x4 _projectionMatrix;
        private Viewport _viewport;

        public MapCamera Camera { get; }

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

        public MapFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _mapFile = MapFile.FromFileSystemEntry(file);

            Camera = new MapCamera();
        }

        public void OnMouseMove(int x, int y)
        {
            if (_map == null)
            {
                return;
            }

            var view = Camera.ViewMatrix;
            var world = Matrix4x4.Identity;

            var pos1 = _viewport.Unproject(new Vector3(x, y, 0), ref _projectionMatrix, ref view, ref world);
            var pos2 = _viewport.Unproject(new Vector3(x, y, 1), ref _projectionMatrix, ref view, ref world);
            var dir = Vector3.Normalize(pos2 - pos1);

            var ray = new Ray(pos1, dir);

            var intersectionPoint = _map.Terrain.Intersect(ray);

            if (intersectionPoint != null)
            {
                MousePosition = $"Pos: ({intersectionPoint.Value.X.ToString("F1")}, {intersectionPoint.Value.Y.ToString("F1")}, {intersectionPoint.Value.Z.ToString("F1")})";

                var tilePosition = _map.Terrain.HeightMap.GetTilePosition(intersectionPoint.Value);
                if (tilePosition != null)
                {
                    TilePosition = $"Tile: ({tilePosition.Value.X}, {tilePosition.Value.Y})";
                    HoveredObjectInfo = _map.Terrain.GetTileDescription(tilePosition.Value.X, tilePosition.Value.Y);
                }
                else
                {
                    TilePosition = "Tile: No intersection";
                    HoveredObjectInfo = string.Empty;
                }
            }
            else
            {
                MousePosition = "Pos: No intersection";
                TilePosition = "Tile: No intersection";
                HoveredObjectInfo = string.Empty;
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

            _viewport = new Viewport(
                0, 
                0, 
                swapChain.BackBufferWidth, 
                swapChain.BackBufferHeight);

            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                (float) (70 * System.Math.PI / 180),
                swapChain.BackBufferWidth / (float) swapChain.BackBufferHeight,
                0.1f,
                5000.0f);
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _map = AddDisposable(new Map(
                _mapFile, 
                File.FileSystem,
                graphicsDevice));

            Camera.Reset(_map.Terrain.BoundingBox.GetCenter());

            NotifyOfPropertyChange(nameof(CurrentTimeOfDay));
            NotifyOfPropertyChange(nameof(RenderWireframeOverlay));
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            EnsureDepthStencilBuffer(graphicsDevice, swapChain);
            renderPassDescriptor.SetDepthStencilDescriptor(_depthStencilBuffer);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(_viewport);

            var cameraPosition = Camera.Position;
            var view = Camera.ViewMatrix;

            _map.Draw(commandEncoder, ref cameraPosition, ref view, ref _projectionMatrix);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
