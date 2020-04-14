using Newtonsoft.Json;
using OpenSage.Content;
using OpenSage.Data;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;


namespace OpenSage.Input.KeyBinding
{
    public sealed class KeyBindingController
    {
        private List<KeyBinding> _keyBindings;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private const string KeyBindingsJsonPath = @"OSage-keybindings.json";

        private readonly ContentManager _contentManager;

        public KeyBindingController(ContentManager contentManager)
        {
            _contentManager = contentManager;
            var keyBindingEntry = _contentManager.UserDataFileSystem.GetFile(KeyBindingsJsonPath);
            if (keyBindingEntry != null)
            {
                var encoding = Encoding.ASCII;
                using (var stream = keyBindingEntry.Open())
                using (var reader = new StreamReader(stream, encoding))
                {
                    _keyBindings = JsonConvert.DeserializeObject<List<KeyBinding>>(reader.ReadToEnd());
                }
            }
            else
            {
                Logger.Info("Generating default keybindingFile");
                DefaultKeyBindings defaultKeyBindings = new DefaultKeyBindings();
                _keyBindings = defaultKeyBindings.keyBindings;
                // Deal with creating/saving a default for this file.
                using (var writer = new StreamWriter(File.Create(FileSystem.NormalizeFilePath(_contentManager.UserDataFileSystem.RootDirectory + '/' + KeyBindingsJsonPath))))
                {

                    writer.Write(JsonConvert.SerializeObject(defaultKeyBindings.keyBindings));
                    writer.Flush();
                    writer.Close();
                }
            }
        }

        public KeyBinding getBinding(KeyAction action, List<Key> pressedKeys)
        {
            KeyBinding foundBinding = null;

            if (pressedKeys.Count == 0)
            {
                return null;
            }

            if (_keyBindings == null)
            {
                Logger.Warn("Failed to find keybindings, key actions will not be enabled.");
                return null;
            }

            foreach (KeyBinding binding in _keyBindings)
            {
                if (binding.keyAction.Equals(action))
                {
                    bool missingKey = false;
                    foreach (Key bindingKey in binding.keys)
                    {
                        if (!pressedKeys.Contains(bindingKey))
                        {
                            missingKey = true;
                            break;
                        }
                    }

                    if (!missingKey)
                    {
                        foundBinding = binding;
                        break;
                    }
                }
            }

            return foundBinding;
        }

        public bool isAction(KeyAction action, List<Key> pressedKeys)
        {
            KeyBinding binding = getBinding(action, pressedKeys);
            return binding != null;
        }

        public int getInstance(KeyAction action, List<Key> pressedKeys)
        {
            KeyBinding binding = getBinding(action, pressedKeys);
            return binding != null ? binding.actionInstance : -1;
        }
    }
}
