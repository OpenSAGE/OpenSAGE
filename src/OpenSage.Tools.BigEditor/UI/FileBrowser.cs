using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenSage.Tools.BigEditor.Util;

namespace OpenSage.Tools.BigEditor.UI
{
    internal sealed class FileBrowser : DisposableBase
    {
        private static string _currentPath;
        private static string _currentFile;
        private static string _currentExportFileName;
        private static int _currentSelection;
        private static FileBrowserType _type;
        private static Hashtable _paths;


        public FileBrowser()
        {
            _paths = new Hashtable();

            _paths.Add("CNC_GENERALS_PATH", ImGuiUtility.NormalizePath(Environment.GetEnvironmentVariable("CNC_GENERALS_PATH")));
            _paths.Add("CNC_GENERALS_ZH_PATH", ImGuiUtility.NormalizePath(Environment.GetEnvironmentVariable("CNC_GENERALS_ZH_PATH")));
            _paths.Add("Current Directory", ImGuiUtility.NormalizePath(Environment.CurrentDirectory));

            _currentExportFileName = "";
            _currentPath = "/";
        }

        public string Draw(FileBrowserType type, string exportFileName = "")
        {
            _type = type;
            _currentExportFileName = exportFileName;

            if (type == FileBrowserType.Save || type == FileBrowserType.Export)
            {
                ImGui.InputText("File name", ref _currentExportFileName, 255);
            }

            ImGui.BeginChild("body");

            ImGui.BeginChild("sidebar", new Vector2(350, -30), true);

            ImGui.BeginChild("files list", Vector2.Zero, true, ImGuiWindowFlags.HorizontalScrollbar);

            if (ImGui.TreeNodeEx("Go to..."))
            {
                foreach (DictionaryEntry path in _paths)
                {
                    if (ImGui.Selectable(path.Key.ToString(), _currentPath == path.Value.ToString().Replace("\\", "")))
                    {
                        _currentPath = path.Value.ToString().Replace("\\", "");
                    }
                }

                ImGui.TreePop();
            }

            DirTree("/");

            ImGui.EndChild(); // end files list

            ImGui.EndChild(); // end sidebar

            ImGui.SameLine();

            if (FileChooser())
            {
                if (_type == FileBrowserType.Export || _type == FileBrowserType.Save)
                {
                    return _currentPath;
                }

                ImGui.CloseCurrentPopup();

                return _currentFile;
            }

            ImGui.NewLine();
            ImGui.SameLine(ImGui.GetWindowWidth() - 120);

            if (ImGui.Button(GetButtonName(_type)))
            {
                if (_type == FileBrowserType.Open)
                {
                    if (Directory.Exists(_currentFile))
                    {
                        _currentPath = _currentFile;
                    }
                    else
                    {
                        ImGui.CloseCurrentPopup();

                        return _currentFile;
                    }
                }
                else
                {
                    ImGui.CloseCurrentPopup();

                    return _currentFile;
                }
            }

            ImGui.SetItemDefaultFocus();

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndChild(); // end body

            return "";
        }

        private static bool FileChooser()
        {
            Adapter adapter = null;

            try
            {
                if (_type == FileBrowserType.ImportFromDir || _type == FileBrowserType.ExportToDir)
                {
                    adapter = new Adapter(_currentPath, "", AdapterFlags.OnlyDirectories);
                }
                else
                {
                    adapter = new Adapter(_currentPath);
                }
            }
            catch
            {
                return false;
            }

            ImGui.BeginChild("container", new Vector2(0, -30));

            ImGui.BeginChild("file chooser", new Vector2(0, 24));

            ImGui.Columns(3, "Files", false);


            ImGui.SetColumnOffset(1, ImGui.GetWindowWidth() - 320);
            ImGui.SetColumnOffset(2, ImGui.GetWindowWidth() - 120);

            ImGui.Separator();
            ImGui.Text("Name"); ImGui.NextColumn();
            ImGui.Text("Date"); ImGui.NextColumn();
            ImGui.Text("Size"); ImGui.NextColumn();
            ImGui.Separator();

            ImGui.EndChild(); // file chooser

            ImGui.BeginChild("browser");

            ImGui.Columns(3, "list", false);
            ImGui.SetColumnOffset(1, ImGui.GetWindowWidth() - 320);
            ImGui.SetColumnOffset(2, ImGui.GetWindowWidth() - 120);

            if (adapter.RootDirectory.CompareTo("/") != 0)
            {
                if (ImGui.Selectable("..", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(0))
                    {
                        _currentPath = adapter.RootDirectory;
                    }
                }

                ImGui.NextColumn();
                ImGui.NextColumn();
                ImGui.NextColumn();
            }

            int i = 0;
            foreach (var entry in adapter.Entries)
            {
                if (ImGui.Selectable(entry.Name, i == _currentSelection, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(0))
                    {
                        if (entry.IsFile)
                        {
                            ImGui.CloseCurrentPopup();

                            return true;
                        }
                        else
                        {
                            i = -1;
                            _currentPath = entry.FullName;
                        }
                    }

                    _currentSelection = i;
                    _currentFile = entry.FullName;
                }

                ImGui.NextColumn();

                ImGui.Text(entry.CreationTime.ToLocalTime().ToString());

                ImGui.NextColumn();

                ImGui.Text(ImGuiUtility.GetFormatedSize(entry.Length));

                ImGui.NextColumn();

                i++;
            }

            ImGui.EndChild(); // browser

            ImGui.EndChild(); // container

            return false;
        }

        private static bool DirTree(string currentPath = "/")
        {
            DirectoryInfo current = new DirectoryInfo(currentPath);
            DirectoryInfo[] dirs = current.GetDirectories();

            if (ImGui.TreeNodeEx(currentPath, ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var dir in dirs)
                {
                    SubTree(dir.Name + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar + dir.Name + Path.DirectorySeparatorChar);
                }

                ImGui.TreePop();

                return true;
            }

            return false;
        }

        private static bool SubTree(string label, string path)
        {
            DirectoryInfo current = null;
            DirectoryInfo[] dirs = null;

            try
            {
                current = new DirectoryInfo(path);
                dirs = current.GetDirectories();
            }
            catch (Exception e)
            {
                if (ImGui.TreeNodeEx(label))
                {
                    ImGui.Text(e.Message);
                    ImGui.TreePop();
                }
                return false;
            }

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.AllowItemOverlap;

            if (_currentPath.StartsWith(path))
            {
                flags = ImGuiTreeNodeFlags.DefaultOpen;

                if (ImGuiUtility.NormalizePath(_currentPath).CompareTo(path) == 0)
                {
                    flags |= ImGuiTreeNodeFlags.Selected;
                }
            }
            else
            {
                flags = ImGuiTreeNodeFlags.None;
            }

            if (dirs.Length <= 0)
            {
                if (ImGui.TreeNodeEx(label, flags))
                {
                    if (ImGui.IsItemHovered() && ImGui.IsItemDeactivated() || ImGui.IsItemHovered() && ImGui.IsItemActivated())
                    {
                        _currentPath = path;
                    }

                    ImGui.TreePop();
                }

                return true;
            }

            if (ImGui.TreeNodeEx(label, flags))
            {
                if (ImGui.IsItemHovered() && ImGui.IsItemDeactivated() || ImGui.IsItemHovered() && ImGui.IsItemActivated())
                {
                    _currentPath = path;
                }

                foreach (var dir in dirs)
                {
                    SubTree(dir.Name + Path.DirectorySeparatorChar, path + dir.Name + Path.DirectorySeparatorChar);
                }

                ImGui.TreePop();

                return true;
            }

            return false;
        }

        private static string GetButtonName(FileBrowserType type)
        {
            switch (type)
            {
                case FileBrowserType.Open:
                    {
                        return " Open ";
                    }
                case FileBrowserType.Save:
                    {
                        return " Save ";
                    }
                case FileBrowserType.ImportFromDir:
                case FileBrowserType.Import:
                    {
                        return "Import";
                    }
                case FileBrowserType.ExportToDir:
                case FileBrowserType.Export:
                    {
                        return "Export";
                    }

                default:
                    {
                        return "Open";
                    }
            }
        }
    }
}
