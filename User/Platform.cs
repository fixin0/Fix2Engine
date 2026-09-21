using System;
using System.Runtime.InteropServices;

namespace Fix2Engine.User;

public static class Platform
{

    public static string PlatformInfo()
    {

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "WIN32/NT Kernel";
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "OSX";
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "LINUX";
        }

        return ":/";

    }

}
