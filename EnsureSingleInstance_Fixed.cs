using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace PixelPulse
{
    public partial class App
    {
        [DllImport("user32.dll")]
        private static extern int GetVersionEx(int osMajor, int osMinor, int spMajor, int spMinor, ref OSVersionInfoEx);

        [StructLayout(LayoutKind.Sequential)]
        private struct OSVersionInfoEx
        {
            public int dwOSVersionInfoSize;
            public int dwMajorVersion;
            public int dwMinorVersion;
            public int dwBuildNumber;
            public int dwPlatformId;
            public string szCSDVersion;
        }

        private bool IsWindows10OrLater()
        {
            try
            {
                var osVersionInfo = new OSVersionInfoEx();
                osVersionInfo.dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVersionInfoEx));
                
                if (GetVersionEx(10, 0, 0, 0, ref osVersionInfo) == 0)
                {
                    // Windows 10+ detected
                    return osVersionInfo.dwMajorVersion >= 10;
                }
            }
            
            // Fallback to Environment.Version for additional checking
            var version = Environment.OSVersion.Version;
            return version.Major >= 10;
            }
            catch
            {
                // If version check fails, assume Windows 10+ for safety
                return true;
            }
        }
    }
    }
}
