using System;
using System.Collections;
using System.IO;
using System.Numerics;
using System.Text;
using ImGuiNET;
using OpenSage.Tools.BigEditor.Util;

namespace OpenSage.Tools.BigEditor.UI
{
    internal sealed class FileBrowser : DisposableBase
    {
        private static string _currentPath;
        private static string _currentFile;
        private static int _currentSelection;
        private static Hashtable _paths;

        public FileBrowser() {
            _paths = new Hashtable();

            _paths.Add("CNC_GENERALS_PATH", Environment.GetEnvironmentVariable("CNC_GENERALS_PATH"));
            _paths.Add("CNC_GENERALS_ZH_PATH", Environment.GetEnvironmentVariable("CNC_GENERALS_ZH_PATH"));
            _paths.Add("Current Directory", Environment.CurrentDirectory);

            _currentPath = "/";
        }

        public string Draw()
        {
            string currentDir = Environment.CurrentDirectory;

            ImGui.BeginChild("sidebar", new Vector2(350, -30), true);

            ImGui.BeginChild("files list", Vector2.Zero, true, ImGuiWindowFlags.HorizontalScrollbar);

            if (ImGui.TreeNodeEx("Go to..."))
            {
                foreach (DictionaryEntry path in _paths)
                {
                    if (ImGui.Selectable(path.Key.ToString()))
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
                ImGui.CloseCurrentPopup();

                return _currentFile;
            }

            ImGui.NewLine();
            ImGui.SameLine(ImGui.GetWindowWidth() - 104);

            if (ImGui.Button("Open"))
            {
                ImGui.CloseCurrentPopup();

                return _currentFile;
            }

            ImGui.SetItemDefaultFocus();

            ImGui.SameLine();

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            return "";
        }

        private static bool FileChooser() {
            DirectoryInfo current = null;
            FileInfo[] files = null;

            try {
                current = new DirectoryInfo(_currentPath);
                files = current.GetFiles("*.big");
            } catch {
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

            int i = 0;
            foreach (var file in files)
            {
                if (ImGui.Selectable(file.Name, i == _currentSelection, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseDoubleClicked(0))
                    {
                        ImGui.CloseCurrentPopup();

                        return true;
                    }

                    _currentSelection = i;
                    _currentFile = file.FullName;
                }

                ImGui.NextColumn();

                ImGui.Text(file.CreationTime.ToLocalTime().ToString());

                ImGui.NextColumn();

                string size = file.Length.ToString();
                if (file.Length / 1024 > 1)
                {
                    size = $"{file.Length / 1024}K";
                }
                if (file.Length / 1024 > 1024)
                {
                    size = $"{file.Length / 1024 / 1024}M";
                }

                ImGui.Text(size);

                ImGui.NextColumn();

                i++;
            }

            ImGui.EndChild(); // browser

            ImGui.EndChild(); // container

            return false;
        }

        private static bool DirTree(string currentPath = "/") {
            DirectoryInfo current = new DirectoryInfo(currentPath);
            DirectoryInfo[] dirs = current.GetDirectories();

            if (ImGui.TreeNodeEx(currentPath, ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var dir in dirs)
                {
                    SubTree(dir.Name + "/", "/" + dir.Name + "/");
                }

                ImGui.TreePop();

                return true;
            }

            return false;
        }

        private static bool SubTree(string label, string path) {
            DirectoryInfo current = null;
            DirectoryInfo[] dirs = null;

            try {
                current = new DirectoryInfo(path);
                dirs = current.GetDirectories();
            } catch (Exception e) {
                if (ImGui.TreeNodeEx(label)) {
                    ImGui.Text(e.Message);
                    ImGui.TreePop();
                }
                return false;
            }

            if (dirs.Length <= 0) {
                if (ImGui.TreeNodeEx(label))
                {
                    if (ImGui.IsItemHovered() && ImGui.IsItemDeactivated() || ImGui.IsItemHovered() && ImGui.IsItemActivated())
                    {
                        _currentPath = path;
                    }

                    ImGui.TreePop();
                }

                return true;
            }

            if (ImGui.TreeNodeEx(label))
            {
                if (ImGui.IsItemHovered() && ImGui.IsItemDeactivated() || ImGui.IsItemHovered() && ImGui.IsItemActivated())
                {
                    _currentPath = path;
                }

                foreach (var dir in dirs)
                {
                    SubTree(dir.Name + "/", path + dir.Name + "/");
                }

                ImGui.TreePop();

                return true;
            }

            return false;
        }
    }
}
