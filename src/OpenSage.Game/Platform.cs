using System;

namespace OpenSage
{
    public static class Platform
    {
        public static IPlatform CurrentPlatform;
    }

    public interface IPlatform
    {
        void Start();
        void Stop();

        GameWindow CreateWindow(IntPtr windowsWindowHandle);

        GameWindow CreateWindow(string title, int x, int y, int width, int height);

        Cursor CreateCursor(string filePath);
    }
}
