using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using WixToolset.Dtf.WindowsInstaller;

namespace PixelPulse.Installer
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DownloadAndInstallNetRuntime(Session session)
        {
            try
            {
                session.Log("Starting .NET Runtime download and installation...");
                
                // Check if .NET 8 is already installed
                if (IsNet8Installed())
                {
                    session.Log(".NET 8 Runtime is already installed.");
                    return ActionResult.Success;
                }

                // Download .NET 8 Runtime
                var runtimeUrl = "https://download.visualstudio.microsoft.com/download/pr/dotnet-runtime-8.0.exe";
                var tempPath = Path.Combine(Path.GetTempPath(), "dotnet-runtime-8.0.exe");
                
                session.Log($"Downloading .NET 8 Runtime from: {runtimeUrl}");
                
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(runtimeUrl);
                await using var fileStream = File.Create(tempPath);
                await response.Content.CopyToAsync(fileStream);
                
                session.Log($"Downloaded .NET 8 Runtime to: {tempPath}");
                
                // Install silently
                session.Log("Installing .NET 8 Runtime...");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = tempPath,
                    Arguments = "/quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas" // Request admin privileges
                };
                
                using var process = Process.Start(startInfo);
                await process.WaitForExitAsync();
                
                // Cleanup
                try 
                { 
                    File.Delete(tempPath); 
                }
                catch 
                { 
                    session.Log($"Warning: Could not cleanup temp file: {tempPath}");
                }
                
                if (process.ExitCode == 0)
                {
                    session.Log(".NET 8 Runtime installed successfully.");
                    return ActionResult.Success;
                }
                else
                {
                    session.Log($".NET 8 Runtime installation failed with exit code: {process.ExitCode}");
                    return ActionResult.Failure;
                }
            }
            catch (Exception ex)
            {
                session.Log($"Error during .NET Runtime installation: {ex.Message}");
                return ActionResult.Failure;
            }
        }

        private static bool IsNet8Installed()
        {
            try
            {
                // Check registry for .NET 8
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full", 
                    false);
                
                if (key != null)
                {
                    var version = key.GetValue("Version")?.ToString();
                    key.Close();
                    
                    return !string.IsNullOrEmpty(version) && version.StartsWith("8.");
                }
                
                // Alternative check for newer .NET versions
                using var netKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v5", 
                    false);
                
                if (netKey != null)
                {
                    var version = netKey.GetValue("Version")?.ToString();
                    netKey.Close();
                    
                    return !string.IsNullOrEmpty(version) && 
                           (version.StartsWith("8.") || 
                            int.TryParse(version.Split('.')[0], out var major) && major >= 8);
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
