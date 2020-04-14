using System.Collections.Generic;
using System;
using Veldrid;

namespace OpenSage.Input.KeyBinding
{

    public sealed class DefaultKeyBindings
    {
		public List<KeyBinding> keyBindings { get; set; }

		public DefaultKeyBindings()
		{
			keyBindings = new List<KeyBinding>();

			keyBindings.AddRange(getCameraSaveLoadPositionDefaultKeyBindings());	
		}

		private List<KeyBinding> getCameraSaveLoadPositionDefaultKeyBindings()
		{
			List<KeyBinding> saveLoadBindings = new List<KeyBinding>();
			for (int i = 0 ; i < 10; i++)
			{
				KeyBinding saveBinding = new KeyBinding();
				saveBinding.keyAction = KeyAction.CAMERA_SAVE_POSITION;
				List<Key> keys = new List<Key>();
				keys.Add(Key.AltLeft);
				keys.Add(Key.LShift);
				keys.Add((Key) Enum.Parse(typeof(Key), "Number" + i));
				saveBinding.keys = keys;
				saveBinding.actionInstance = i;
				saveBinding.description = "Store a camera position (" + i +") for later recall";
				saveLoadBindings.Add(saveBinding);

				KeyBinding loadBinding = new KeyBinding();
				loadBinding.keyAction = KeyAction.CAMERA_LOAD_POSITION;
				List<Key> loadKeys = new List<Key>();
				loadKeys.Add(Key.AltLeft);
				loadKeys.Add((Key) Enum.Parse(typeof(Key), "Number" + i));
				loadBinding.keys = loadKeys;
				loadBinding.actionInstance = i;
				loadBinding.description = "Load a previously saved camera position (" + i +")";
				saveLoadBindings.Add(loadBinding);
			}
			return saveLoadBindings;
		}
	}
}
