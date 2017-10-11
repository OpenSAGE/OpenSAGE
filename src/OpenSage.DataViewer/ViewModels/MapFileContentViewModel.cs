using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.Graphics.Cameras;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel, IGameViewModel
    {
        private Scene _scene;

        private TerrainComponent _terrain;

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
            get { return _scene?.Settings.TimeOfDay ?? TimeOfDay.Morning; }
            set
            {
                _scene.Settings.TimeOfDay = value;
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
            
        }

        void IGameViewModel.LoadScene(Game game)
        {
            _scene = game.ContentManager.Load<Scene>(File.FilePath, uploadBatch: null);

            _terrain = _scene.Entities[0].GetComponent<TerrainComponent>();

            var cameraEntity = new Entity();
            _scene.Entities.Add(cameraEntity);

            //var cameraController = new MapCameraController();
            //cameraEntity.Components.Add(cameraController);
            //cameraController.Reset(_terrain.Entity.GetEnclosingBoundingBox().GetCenter());

            game.Scene = _scene;

            NotifyOfPropertyChange(nameof(CurrentTimeOfDay));
        }

        // TODO: Hook this up as a component.
        public void OnMouseMove(int x, int y)
        {
            //var ray = Game.Scene.Camera.ScreenPointToRay(new Vector2(x, y));

            //var intersectionPoint = _terrain.Intersect(ray);

            //if (intersectionPoint != null)
            //{
            //    MousePosition = $"Pos: ({intersectionPoint.Value.X.ToString("F1")}, {intersectionPoint.Value.Y.ToString("F1")}, {intersectionPoint.Value.Z.ToString("F1")})";

            //    var tilePosition = _terrain.HeightMap.GetTilePosition(intersectionPoint.Value);
            //    if (tilePosition != null)
            //    {
            //        TilePosition = $"Tile: ({tilePosition.Value.X}, {tilePosition.Value.Y})";
            //    }
            //    else
            //    {
            //        TilePosition = "Tile: No intersection";
            //    }
            //}
            //else
            //{
            //    MousePosition = "Pos: No intersection";
            //    TilePosition = "Tile: No intersection";
            //}
        }
    }
}
