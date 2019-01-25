using Veldrid.Sdl2;

namespace OpenSage
{
    public static class Platform
    {
        public static void Start()
        {
            Sdl2Native.SDL_Init(SDLInitFlags.Video);
        }

        public static void Stop()
        {
            // ?
        }
    }
}
