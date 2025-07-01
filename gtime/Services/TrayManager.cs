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
                Label = "Toggle visibility",
                Click = ToggleHide
            };

            Electron.Tray.Show(Path.Combine(env.ContentRootPath, "Assets/electron_32x32.png"), menu);
            Electron.Tray.SetToolTip("GTIME - Rightclick to toggle visibility");
        }
        
        //ToggleHide();
    }

    private void ToggleHide()
    {
        if (isHidden)
        {
            Console.WriteLine("Showing...");
            var showCmd =
                "CURRENT_WS=$(hyprctl activeworkspace -j | jq -r '.id')\nWIN_ID=$(hyprctl clients -j | jq -r '.[] | select(.title == \"gtime\" ) | .address')\n\nif [ -n \"$WIN_ID\" ]; then\n  hyprctl dispatch movetoworkspacesilent \"$CURRENT_WS\" \"$WIN_ID\"\nfi\n";
            var r = BashExec(showCmd);
            Console.WriteLine(showCmd);
            Console.WriteLine(r);
        }
        else
        {
            Console.WriteLine("Hiding...");
            var hideCmd =
                "WIN_ID=$(hyprctl clients -j | jq -r '.[] | select(.title == \"gtime\") | .address')\n\nif [ -n \"$WIN_ID\" ]; then\n  hyprctl dispatch movetoworkspacesilent 7 \"$WIN_ID\"\nfi";
            var r = BashExec(hideCmd);
            Console.WriteLine(hideCmd);
            Console.WriteLine(r);
        }
        isHidden = !isHidden;
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