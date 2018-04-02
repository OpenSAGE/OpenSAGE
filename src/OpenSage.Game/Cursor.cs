namespace OpenSage
{
    public sealed class Cursor : DisposableBase
    {
        //private readonly IntPtr _sdlCursorHandle;

        public Cursor(string filePath)
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
