using System.Diagnostics;
using ElectronNET.API;
using ElectronNET.API.Entities;

namespace gtime.Services;

public class TrayManager
{
    private bool isHidden = false;
    
    public TrayManager(IWebHostEnvironment env)
    {
        if (Electron.Tray.MenuItems.Count == 0)
        {
            Console.WriteLine("Setting up tray...");
            var menu = new MenuItem
            {
                Label = "Show",
                Click = ShowWindow
            };

            Electron.Tray.Show(Path.Combine(env.ContentRootPath, "Assets/electron_32x32.png"), menu);
            Electron.Tray.SetToolTip("GTIME");
            Electron.Tray.OnClick += ShowWindowFromTray;
        }
    }

    private void ShowWindowFromTray(TrayClickEventArgs _, Rectangle __)
    {
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (!isHidden)
            return;
        
        Console.WriteLine("Showing...");
        var showCmd = GetEmbeddedScript("gtime.scripts.show_window.sh");
        BashExec(showCmd);
        isHidden = false;
    }
    
    public void HideWindow()
    {
        if (isHidden)
            return;
        
        Console.WriteLine("Hiding...");
        var hideCmd = GetEmbeddedScript("gtime.scripts.hide_window.sh");
        BashExec(hideCmd);
        isHidden = true;
    }

    private string GetEmbeddedScript(string resourceName)
    {
        var assembly = typeof(TrayManager).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new InvalidOperationException($"Resource not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
    
    
    private static string BashExec(string cmd)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{cmd}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process();
            process.StartInfo = psi;
            process.Start();

            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return string.IsNullOrEmpty(error) ? output : error;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.Message;
        }
    }
}