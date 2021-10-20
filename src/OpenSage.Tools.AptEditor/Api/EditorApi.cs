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
        public static OperationState StateSuccessful = new(0, "Successful");
        public static OperationState StateNoFileSelected = new(1, "No file selected");



        private List<string> _paths;
        private Dictionary<int, (AptStreamGetter?, TreeViewEditor, AptFile)> _openFiles;
        private int _newId;
        private int _selected;
        private AptStreamGetter? _selectedGetter => _openFiles.TryGetValue(_selected, out var s) ? s.Item1 : null;
        private TreeViewEditor? _selectedInstance => _openFiles.TryGetValue(_selected, out var s) ? s.Item2 : null;
        private AptFile? _selectedFile => _openFiles.TryGetValue(_selected, out var s) ? s.Item3 : null;
        private AptSceneInstance? _debugger;

        private bool _isSelected => _openFiles.ContainsKey(_selected);

        public TreeViewEditor? Selected => _selectedInstance;

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
                return new(2, e.Message);
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
                return StateSuccessful;
            }
            else
            {
                return new(1, "Inexistent file");
            }
        }

        public OperationState GetSelected()
        {
            if (_isSelected)
            {
                return new(0, JsonSerializer.Serialize(_selected));
            }
            else
            {
                return StateNoFileSelected;
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
            if (_isSelected)
            {
                var g = _selectedGetter;
                if (!string.IsNullOrEmpty(path))
                    g = new StandardStreamGetter(path);
                if (g == null)
                    return new(3, "Path unassigned");
                try
                {
                    AptDataDump d = new(_selectedFile!, _selectedGetter);
                    throw new NotImplementedException("Async Execution");
                    return StateSuccessful;
                }
                catch (Exception e)
                {
                    return new(2, JsonSerializer.Serialize(e));
                }
            }
            else
            {
                return StateNoFileSelected;
            }
        }

        public OperationState Close()
        {
            if (_isSelected)
            {
                _selected = 0;
                return StateSuccessful;
            }
            else
            {
                return StateNoFileSelected;
            }
        }

        // treeview operation

        // operations of operations

        public OperationState Undo() { return _isSelected ? _selectedInstance!.Undo() : StateNoFileSelected; }

        public OperationState Redo() { return _isSelected ? _selectedInstance!.Redo() : StateNoFileSelected; }

        public OperationState StartMerging(string description) { return _isSelected ? _selectedInstance!.StartMerging(description) : StateNoFileSelected; }

        public OperationState EndMerging() { return _isSelected ? _selectedInstance!.EndMerging() : StateNoFileSelected; }


        // node operations

        public OperationState AddNode(int parentNodeId) { return _isSelected ? _selectedInstance!.AddNode(parentNodeId) : StateNoFileSelected; }

        public OperationState RemoveNode(int id) { return _isSelected ? _selectedInstance!.RemoveNode(id) : StateNoFileSelected; }


        // todo overwrite(cut, copy, paste)

        // node info

        public OperationState GetType(int id) { return _isSelected ? _selectedInstance!.GetType(id) : StateNoFileSelected; }

        public OperationState GetFieldName(int id) { return _isSelected ? _selectedInstance!.GetFieldName(id) : StateNoFileSelected; }

        public OperationState GetFields(int id) { return _isSelected ? _selectedInstance!.GetFields(id) : StateNoFileSelected; }

        public OperationState GetChildren(int id) { return _isSelected ? _selectedInstance!.GetChildren(id) : StateNoFileSelected; }

        // field operations

        public OperationState Get(int id, string field) { return _isSelected ? _selectedInstance!.Get(id, field) : StateNoFileSelected; }

        public OperationState Set(int id, string field, string value) { return _isSelected ? _selectedInstance!.Set(id, field, value) : StateNoFileSelected; }

        // debugger operation

        public OperationState InitDebugger()
        {
            _debugger = new(""); // TODO
            return null;
        }

    }
}
