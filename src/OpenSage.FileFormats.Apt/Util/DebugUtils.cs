using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenAS2.Base;
using System.IO;

namespace OpenSage.FileFormats.Apt
{
    public static class DebugUtils
    {

        public static void SaveInstructionsBinary(InstructionStorage insts, string path)
        {
            using (BinaryWriter writer = new(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write)))
            using (BinaryMemoryChain mp = new())
            {
                insts.Write(writer, mp);
            }

        }

        public static InstructionStorage ReadInstructionsbinary(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                var insts = InstructionStorage.Parse(fs, 0);
                return insts;
            }
        }

        public static void OutputAllAptCodesBinary(AptFile file, string pathFolder)
        {
            // TODO ensure path folder

            Dictionary<int, string> spriteNames = new();
            foreach (var imp in file.Movie.Imports)
                spriteNames[(int) imp.Character] = imp.Name;
            foreach (var imp in file.Movie.Exports)
                spriteNames[(int) imp.Character] = imp.Name;
            
            for (int ci = 0; ci < file.Movie.Characters.Count; ++ci)
            {
                if (file.Movie.Characters[ci] is Playable pl)
                    for (int i = 0; i < pl.Frames.Count; ++i)
                        for (int j = 0; j < pl.Frames[i].FrameItems.Count; ++j)
                        {
                            var item = pl.Frames[i].FrameItems[j];
                            if (item is FrameItems.Action act)
                            {
                                var insts = act.Instructions;
                                var fname = pathFolder + $"/{file.MovieName}_C{ci}_F{i}_I{j}_Action.apt";
                                SaveInstructionsBinary(insts, fname);
                            }
                            else if (item is InitAction iact)
                            {
                                var insts = iact.Instructions;
                                var isp = (int) iact.Sprite;
                                var sp = file.Movie.Characters[isp] is Sprite && spriteNames.TryGetValue(isp, out var spname) ?
                                    spname : "Sprite{isp}";
                                var fname = pathFolder + $"/{file.MovieName}_C{ci}_F{i}_I{j}_{sp}_Init.apt";
                                SaveInstructionsBinary(insts, fname);
                            }
                            else if (item is PlaceObject plo && plo.Flags.HasFlag(PlaceObjectFlags.HasClipAction))
                            {
                                foreach (var pv in plo.ClipEvents)
                                {
                                    var insts = pv.Instructions;
                                    var isp = ((int) pv.Flags).ToString("X");
                                    var fname = pathFolder + $"/{file.MovieName}_C{ci}_F{i}_I{j}_Event{isp}.apt";
                                    SaveInstructionsBinary(insts, fname);
                                }
                            }
                        }
                else
                    continue;
            }
        }

        // string

        public static void SaveInstructions(InstructionStorage insts, string path)
        {
            var s = StringParsingUtils.FormInstructionStorage(insts.GetPositionedInstructions());
            File.WriteAllText(path, s);
        }

        public static void OutputAllAptCodes(AptFile file, string pathFolder)
        {
            // TODO ensure path folder

            // save constants
            File.WriteAllText(pathFolder + $"/{file.MovieName}_Constants.cst",
                StringParsingUtils.FormConstantStorage(file.Constants.Entries)
                );

            Dictionary<int, string> spriteNames = new();
            foreach (var imp in file.Movie.Imports)
                spriteNames[(int) imp.Character] = imp.Name;
            foreach (var imp in file.Movie.Exports)
                spriteNames[(int) imp.Character] = imp.Name;

            for (int ci = 0; ci < file.Movie.Characters.Count; ++ci)
            {
                if (file.Movie.Characters[ci] is Playable pl)
                    for (int i = 0; i < pl.Frames.Count; ++i)
                        for (int j = 0; j < pl.Frames[i].FrameItems.Count; ++j)
                        {
                            var item = pl.Frames[i].FrameItems[j];
                            if (item is FrameItems.Action act)
                            {
                                var insts = act.Instructions;
                                var fname = pathFolder + $"/{file.MovieName}_C{ci}_F{i}_I{j}_Action.asc";
                                SaveInstructions(insts, fname);
                            }
                            else if (item is InitAction iact)
                            {
                                var insts = iact.Instructions;
                                var isp = (int) iact.Sprite;
                                var sp = file.Movie.Characters[isp] is Sprite && spriteNames.TryGetValue(isp, out var spname) ?
                                    spname : "Sprite{isp}";
                                var fname = pathFolder + $"/{file.MovieName}_C{ci}_F{i}_I{j}_{sp}_Init.asc";
                                SaveInstructions(insts, fname);
                            }
                            else if (item is PlaceObject plo && plo.Flags.HasFlag(PlaceObjectFlags.HasClipAction))
                            {
                                foreach (var pv in plo.ClipEvents)
                                {
                                    var insts = pv.Instructions;
                                    var isp = ((int) pv.Flags).ToString("X");
                                    var fname = pathFolder + $"/{file.MovieName}_C{ci}_F{i}_I{j}_Event{isp}.asc";
                                    SaveInstructions(insts, fname);
                                }
                            }
                        }
                else
                    continue;
            }
        }
    }
}
