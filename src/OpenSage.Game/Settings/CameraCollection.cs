using System.Collections.Generic;
using OpenSage.Data.Map;

namespace OpenSage.Settings
{
    public sealed class CameraCollection
    {
        private readonly Dictionary<string, NamedCamera> _camerasByName;

        public NamedCamera this[string name] => _camerasByName[name];

        public CameraCollection()
        {
            _camerasByName = new Dictionary<string, NamedCamera>();
        }

        public CameraCollection(IEnumerable<NamedCamera> cameras)
            : this()
        {
            if (cameras != null)
            {
                foreach (var camera in cameras)
                {
                    _camerasByName[camera.Name] = camera;
                }
            }
        }

        public bool Exists(string camera)
        {
            return _camerasByName.ContainsKey(camera);
        }
    }
}
