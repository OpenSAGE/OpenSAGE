using ImGuiNET;

namespace OpenSage.Tools.AptEditor.Util
{
    internal ref struct ImGuiIDHelper
    {
        private int? _id;

        public ImGuiIDHelper(string type, ref int id)
        {
            _id = ++id;
            ImGui.PushID(type.GetHashCode());
            ImGui.PushID(_id.Value);
        }

        public void Dispose()
        {
            if (_id is null)
            {
                return;
            }
            _id = null;
            ImGui.PopID();
            ImGui.PopID();
        }
    }
}
