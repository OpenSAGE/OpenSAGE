using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Veldrid;

namespace OpenSage.Input.KeyBinding
{
    public enum KeyAction {
        CAMERA_LOAD_POSITION,
        CAMERA_SAVE_POSITION,
        // Add future keyActions here + update existing hard-coded references
    }

    public sealed class KeyBinding
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public KeyAction keyAction { get; set; }

        [JsonProperty (ItemConverterType = typeof(StringEnumConverter))]
        public List<Key> keys { get; set; }

        public String description { get; set; }

        // Used to denote seperate instances of the same action
        // I.e. addControlGroup has 0-9 instances in vanilla ZH
        public int actionInstance { get; set; }

        public KeyBinding()
        {
            
        }

    }
}
