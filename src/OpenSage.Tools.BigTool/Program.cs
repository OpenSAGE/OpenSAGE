using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using ConsoleTables;
using OpenSage.FileFormats.Big;

namespace OpenSage.Tools.BigTool
{
    class Program
    {
        public enum CompressionMethod
        {
            None,
            RefPack,
        }

        public sealed class Options
        {
            [Option('Z', "compression", Default = CompressionMethod.None, Required = false, HelpText = "The compression method to use")]
            public CompressionMethod Compression { get; set; }

            [Option('r', "recursive", Default = false, Required = false, HelpText = "Use recursive to add directories to the archive")]
            public bool Recursive { get; set; }

            [Option('e', "extract", Default = false, Required = false, HelpText = "Extract entries from the archive", SetName = "extract")]
            public bool Extract { get; set; }

            [Option('d', "delete", Default = false, Required = false, HelpText = "Delete entries from the archive", SetName = "delete")]
            public bool Delete { get; set; }

            [Option('q', "quiet", Default = false, Required = false, HelpText = "Don't do any console output")]
            public bool Quiet { get; set; }

            [Option('u', "update", Default = false, Required = false, HelpText = "Update an existing archive", SetName = "update")]
            public bool Update { get; set; }

            [Option('l', "list", Default = false, Required = false, HelpText = "List contents of a zip archive", SetName = "list")]
            public bool List { get; set; }

            [Value(0, MetaName = "ArchiveName", HelpText = "Archive which should be used/created.", Required = true)]
            public string ArchiveName { get; set; }

            [Value(1, MetaName = "List of files/directories", HelpText = "List of files/directories that should be added to the archive", Required = false)]
            public IEnumerable<string> Files { get; set; }
        }

        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunOptions);
        }

        static bool ValidateInputFiles(Options opts)
        {
            if (opts.List || opts.Update || opts.Delete )
            {
                if (!File.Exists(opts.ArchiveName))
                {
                    Console.WriteLine("Specified archive does not exist ({0})", opts.ArchiveName);
                    return false;
                }
            }

            if (opts.Update || !opts.Delete)
            {
                foreach (var file in opts.Files)
                {
                    if (!File.Exists(file))
                    {
                        Console.WriteLine("Specified input file does not exist ({0})", file);
                        return false;
                    }
                }
            }

            return true;
        }

        static bool ValidateOptions(Options opts)
        {
            if (!opts.List && !opts.Extract && !opts.Files.Any())
            {
                Console.WriteLine("Must pass any files as arguments");
                return false;
            }

            return true;
        }

        static void ListMode(Options opts)
        {
            var archive = new BigArchive(opts.ArchiveName,BigArchiveMode.Read);
            Console.WriteLine("Archive: " + Path.GetFileName(archive.FilePath));

            var table = new ConsoleTable("Length", "Name");

            foreach (var entry in archive.Entries)
            {
                table.AddRow(entry.Length, entry.FullName);
            }

            table.Write(Format.Minimal);
        }

        static void DeleteMode(Options opts)
        {
            var archive = new BigArchive(opts.ArchiveName, BigArchiveMode.Update);
            foreach (var entryName in opts.Files)
            {
                try
                {
                    var entry = archive.Entries.First(x => x.FullName == entryName);
                    Console.WriteLine("deleting: {0}", entryName);
                    entry.Delete();
                }
                catch (InvalidOperationException e)
                {
                    Console.WriteLine(e.Message + ": " + entryName);
                }
            }
        }

        static void AddDirectoryToArchive(BigArchive archive, string dirpath, bool update = false)
        {
            foreach (string dir in Directory.GetDirectories(dirpath))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    AddFileToArchive(archive, file);
                }
                AddDirectoryToArchive(archive, dir);
            }
        }

        static void AddFileToArchive(BigArchive archive, string filepath, bool update = false)
        {
            BigArchiveEntry entry = null;

            if (!update)
            {
                Console.WriteLine("adding: {0}", filepath);
                entry = archive.CreateEntry(filepath);
            }
            else
            {
                try
                {
                    entry = archive.Entries.First(x => x.FullName == filepath);
                    Console.WriteLine("updating: {0}", filepath);
                }
                catch (InvalidOperationException e)
                {
                    entry = archive.CreateEntry(filepath);
                    Console.WriteLine("adding: {0}", filepath);
                }
            }

            using (var entryStream = entry.Open())
            {
                using (var fileStream = File.OpenRead(filepath))
                {    
                    fileStream.CopyTo(entryStream);
                }
            }
        }

        static void UpdateMode(Options opts)
        {
            var archive = new BigArchive(opts.ArchiveName, BigArchiveMode.Update);
            foreach (var entryName in opts.Files)
            {
                var attr = File.GetAttributes(entryName);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    AddDirectoryToArchive(archive, entryName, true);
                }
                else
                {
                    AddFileToArchive(archive, entryName, true);
                }
            }
        }

        static void CreateMode(Options opts)
        {
            var archive = new BigArchive(opts.ArchiveName, BigArchiveMode.Create);
            foreach (var entryName in opts.Files)
            {
                var attr = File.GetAttributes(entryName);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    AddDirectoryToArchive(archive, entryName);
                }
                else
                {
                    AddFileToArchive(archive, entryName);
                }
            }
        }

        static void ExtractMode(Options opts)
        {
            var archive = new BigArchive(opts.ArchiveName, BigArchiveMode.Read);
            foreach (var entry in archive.Entries)
            {
                using (var entryStream = entry.Open())
                {
                    var dirName = Path.GetDirectoryName(entry.FullName);
                    if (!string.IsNullOrEmpty(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }

                    using (var fileStream = File.OpenWrite(entry.FullName))
                    {
                        Console.WriteLine("extracting: {0}", entry.FullName);
                        entryStream.CopyTo(fileStream);
                    }
                }
            }
        }

        static void RunOptions(Options opts)
        {
            if (!ValidateOptions(opts))
            {
                return;
            }

            if (!ValidateInputFiles(opts))
            {
                return;
            }

            if (opts.List)
            {
                ListMode(opts);
                return;
            }

            if (opts.Delete)
            {
                DeleteMode(opts);
                return;
            }

            if (opts.Update)
            {
                UpdateMode(opts);
                return;
            }

            if (opts.Extract)
            {
                ExtractMode(opts);
                return;
            }

            CreateMode(opts);
        }
    }
}
