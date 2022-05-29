using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenAS2.Base;
using OpenSage.FileFormats.Apt;

namespace OpenSage.Tools.AptTool.Base
{
    [JsonObject()]
    public record AptMovieMetadata
    {
        [JsonProperty("imports")]
        public List<(string, string, uint)> Imports { get; private set; }
        [JsonProperty("exports")]
        public List<(string, uint)> Exports { get; private set; }
        [JsonProperty("constants")]
        public List<ConstantEntry> ConstantEntries { get; private set; }
        [JsonProperty("width")]
        public uint ScreenWidth { get; private set; }
        [JsonProperty("height")]
        public uint ScreenHeight { get; private set; }
        [JsonProperty("msPerFrame")]
        public uint MillisecondsPerFrame { get; private set; }

        public AptMovieMetadata()
        {
            Imports = new();
            Exports = new();
            ConstantEntries = new();
            ScreenWidth = 1366;
            ScreenHeight = 768;
            MillisecondsPerFrame = 30;
        }

        public AptMovieMetadata(AptFile f)
        {
            var m = f.Movie;
            Imports = m.Imports.Select(x => (x.Movie, x.Name, x.Character)).ToList();
            Exports = m.Exports.Select(x => (x.Name, x.Character)).ToList();
            ConstantEntries = new(f.Constants.Entries);
            ScreenWidth = m.ScreenWidth;
            ScreenHeight = m.ScreenHeight;
            MillisecondsPerFrame = m.MillisecondsPerFrame;
        }

    }

}
