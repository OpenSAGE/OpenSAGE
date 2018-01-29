using System;
using Veldrid.Sdl2;

namespace OpenSage
{
    public sealed class Sdl2Platform : IPlatform
    {
        public void Start()
        {
            Sdl2Native.SDL_Init(SDLInitFlags.Video);
        }

        public void Stop()
        {
            // ?
        }

        public Cursor CreateCursor(string filePath)
        {
            return new Sdl2Cursor();
        }

        public GameWindow CreateWindow(IntPtr windowsWindowHandle)
        {
            return new Sdl2GameWindow(windowsWindowHandle);
        }

        public GameWindow CreateWindow(string title, int x, int y, int width, int height)
        {
            return new Sdl2GameWindow(title, x, y, width, height);
        }
    }

    public sealed class Sdl2Cursor : Cursor
    {
        private readonly IntPtr _sdlCursorHandle;

        public Sdl2Cursor()
        {
            // TODO: Create a set of SDL_Surface objects, one for each frame of the animated cursor.
            //_sdlCursorHandle = Sdl2GameWindow.SDL_CreateColorCursor(surface, 0, 0);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            //Sdl2GameWindow.SDL_FreeCursor(_sdlCursorHandle);

            base.Dispose(disposeManagedResources);
        }
    }
}
