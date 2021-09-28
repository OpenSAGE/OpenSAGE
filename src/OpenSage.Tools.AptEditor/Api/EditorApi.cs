using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenSage.FileFormats.Apt;
using OpenSage.Tools.AptEditor.Util;

namespace OpenSage.Tools.AptEditor.Api
{
    public class EditorApi : IDisposable
    {
        private List<string> _paths;
        private Dictionary<int, (AptStreamGetter?, TreeViewEditor, AptFile)> _openFiles;
        private int _newId;
        private int _selected;
        private AptStreamGetter? _selectedGetter => _openFiles.TryGetValue(_selected, out var s) ? s.Item1 : null;
        private TreeViewEditor _selectedInstance => _openFiles.TryGetValue(_selected, out var s) ? s.Item2 : null;
        private AptFile _selectedFile => _openFiles.TryGetValue(_selected, out var s) ? s.Item3 : null;
        private AptSceneInstance? _debugger;


        // construction

        public EditorApi()
        {
            _paths = new();
            _openFiles = new();
            _newId = 1;
            _selected = 0;
        }

        public void Dispose()
        {

        }

        // file operation

        public OperationState Open(string path)
        {
            try
            {
                AptStreamGetter g = new StandardStreamGetter(path);
                AptFile f = AptFile.Parse(g);
                TreeViewEditor tl = new(f);
                var id = _newId++;
                _openFiles[id] = (g, tl, f);
                return new(0, JsonSerializer.Serialize(id));
            }
            catch (Exception e)
            {
                return new(2, JsonSerializer.Serialize(e));
            }
        }

        public OperationState New(string name, int w, int h, int msPerFrame)
        {
            try
            {
                AptFile f = AptFile.CreateEmpty(name, w, h, msPerFrame);
                TreeViewEditor tl = new(f);
                var id = _newId++;
                _openFiles[id] = (null, tl, f);
                return new(0, JsonSerializer.Serialize(id));
            }
            catch (Exception e)
            {
                return new(2, JsonSerializer.Serialize(e));
            }
        }

        public OperationState Select(int id)
        {
            if (_openFiles.ContainsKey(id))
            {
                _selected = id;
                return new(0, "Successful");
            }
            else
            {
                return new(1, "Inexistent file");
            }
        }

        public OperationState GetSelected()
        {
            if (_openFiles.ContainsKey(_selected))
            {
                return new(0, JsonSerializer.Serialize(_selected));
            }
            else
            {
                return new(1, "No file selected");
            }
        }

        public OperationState GetOpenFiles()
        {
            IEnumerable<(int, string)> arr = _openFiles.Select(x => (x.Key,
            x.Value.Item1 == null ? "[Native]" :
            x.Value.Item1.GetPath(DataType.Apt)));
            return new(0, JsonSerializer.Serialize(arr));
        }

        public OperationState SaveAs(string? path = null)
        {
            if (_openFiles.ContainsKey(_selected))
            {
                var g = _selectedGetter;
                if (!string.IsNullOrEmpty(path))
                    g = new StandardStreamGetter(path);
                if (g == null)
                    return new(3, "Path unassigned");
                try
                {
                    AptDataDump d = new(_selectedFile, _selectedGetter);
                    throw new NotImplementedException("Async Execution");
                    return new(0, "Successful");
                }
                catch (Exception e)
                {
                    return new(2, JsonSerializer.Serialize(e));
                }
            }
            else
            {
                return new(1, "No file selected");
            }
        }

        public OperationState Close()
        {
            if (_openFiles.ContainsKey(_selected))
            {
                _selected = 0;
                return new(0, "Successful");
            }
            else
            {
                return new(1, "No file selected");
            }
        }

        // treeview operation



        // debugger operation

        public OperationState InitDebugger()
        {
            return null;
        }

    }
}
