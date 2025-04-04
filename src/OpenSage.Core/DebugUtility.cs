using System;

namespace OpenSage;

public static class DebugUtility
{
    public static void AssertCrash(bool condition, string message = "Impossible")
    {
        // In the future, we may want to downgrade this to a Debug.Assert call,
        // so that it only crashes in debug builds. For now, we want to crash
        // in all builds, so that we know about problems.
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    public static void Crash(string message = "Impossible")
    {
        // In the future, we may want to downgrade this to a Debug.Fail call,
        // so that it only crashes in debug builds. For now, we want to crash
        // in all builds, so that we know about problems.
        throw new Exception(message);
    }
}
