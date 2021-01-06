using System;
using System.IO;
using ImGuiNET;
using Veldrid;

namespace OpenSage.Tools.AptEditor.Util
{
    internal static class ImGuiFontHelpers
    {
        public static ImFontPtr LoadSystemFont(this ImGuiRenderer renderer, string fontFileName)
        {
            var windows = Environment.GetEnvironmentVariable("WINDIR");
            if (windows == null)
            {
                return null;
            }
            var fontPath = Path.Combine(windows, "Fonts", fontFileName);
            var font = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 13);
            if (!font.IsNull())
            {
                renderer.RecreateFontDeviceTexture();
            }
            return font;
        }

        public static bool IsNull(this ImFontPtr pointer)
        {
            return pointer.Equals(new ImFontPtr());
        }
    }

    internal class ImGuiFontSetter : IDisposable
    {
        private bool _disposed;

        public ImGuiFontSetter(ImFontPtr pointer)
        {
            if (!pointer.IsNull())
            {
                ImGui.PushFont(pointer);
            }
            else
            {
                _disposed = true;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ImGui.PopFont();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
