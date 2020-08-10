using System;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace OpenSage.Utilities
{
    internal static class Sdl2Interop
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_Cursor SDL_CreateColorCursor_Delegate(SDL_Surface Sdl2Surface, int hot_x, int hot_y);

        private static readonly SDL_CreateColorCursor_Delegate CreateColorCursorImpl = Sdl2Native.LoadFunction<SDL_CreateColorCursor_Delegate>("SDL_CreateColorCursor");

        public static SDL_Cursor SDL_CreateColorCursor(SDL_Surface Sdl2Surface, int hotX, int hotY) => CreateColorCursorImpl(Sdl2Surface, hotX, hotY);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_FreeCursor_Delegate(SDL_Cursor Sdl2Cursor);

        private static readonly SDL_FreeCursor_Delegate FreeCursorImpl = Sdl2Native.LoadFunction<SDL_FreeCursor_Delegate>("SDL_FreeCursor");

        public static void SDL_FreeCursor(SDL_Cursor Sdl2Cursor) => FreeCursorImpl(Sdl2Cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_SetCursor_Delegate(SDL_Cursor Sdl2Cursor);

        private static readonly SDL_SetCursor_Delegate SetCursorImpl = Sdl2Native.LoadFunction<SDL_SetCursor_Delegate>("SDL_SetCursor");

        public static void SDL_SetCursor(SDL_Cursor Sdl2Cursor) => SetCursorImpl(Sdl2Cursor);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate SDL_Surface SDL_CreateRGBSurfaceWithFormatFrom_Delegate(void* pixels, int width, int height, int depth, int pitch, SDL_PixelFormat format);

        private static readonly SDL_CreateRGBSurfaceWithFormatFrom_Delegate CreateRGBSurfaceWithFormatFromImpl = Sdl2Native.LoadFunction<SDL_CreateRGBSurfaceWithFormatFrom_Delegate>("SDL_CreateRGBSurfaceWithFormatFrom");

        public static unsafe SDL_Surface SDL_CreateRGBSurfaceWithFormatFrom(void* pixels, int width, int height, int depth, int pitch, SDL_PixelFormat format) => CreateRGBSurfaceWithFormatFromImpl(pixels, width, height, depth, pitch, format);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_FreeSurface_Delegate(SDL_Surface Sdl2Surface);

        private static readonly SDL_FreeSurface_Delegate FreeSurfaceImpl = Sdl2Native.LoadFunction<SDL_FreeSurface_Delegate>("SDL_FreeSurface");

        public static void SDL_FreeSurface(SDL_Surface Sdl2Surface) => FreeSurfaceImpl(Sdl2Surface);

        public struct SDL_Cursor
        {
            public readonly IntPtr NativePointer;

            public SDL_Cursor(IntPtr pointer)
            {
                NativePointer = pointer;
            }

            public static implicit operator IntPtr(SDL_Cursor Sdl2Cursor) => Sdl2Cursor.NativePointer;
            public static implicit operator SDL_Cursor(IntPtr pointer) => new SDL_Cursor(pointer);
        }

        public struct SDL_Surface
        {
            public readonly IntPtr NativePointer;

            public SDL_Surface(IntPtr pointer)
            {
                NativePointer = pointer;
            }

            public static implicit operator IntPtr(SDL_Surface Sdl2Surface) => Sdl2Surface.NativePointer;
            public static implicit operator SDL_Surface(IntPtr pointer) => new SDL_Surface(pointer);
        }

        public enum SDL_PixelFormat
        {
            SDL_PIXELFORMAT_ABGR8888 = 376840196,
        }
    }
}
