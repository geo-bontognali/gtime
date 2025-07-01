using System.Diagnostics;
using System.Text.Json;
using gtime.Models;
using Activity = gtime.Models.Activity;

namespace gtime.Services;

public class TrackingService(IRepository repo)
{
    public Activity? CurrentActivity { get; private set; }
    public DateTime LastScan { get; private set; }
    public bool IsIdle { get; private set; }
    public event Func<Task> OnNewTrackingData = null!;
    public event Func<Task> OnIpcError = null!;
    public int FrequencyInSeconds { get; } = 30; 
    
    private const string ipcPath = "/tmp/.gtime_ipc"; // Ensure this is of type tmpfs
    private bool cancellationFlag = false;

    public async Task RunAsync()
    {
        while (!cancellationFlag)
        {
            CurrentActivity = GetCurrentActivity();
            if (CurrentActivity is not null)
                Console.WriteLine($"Current Activity: {CurrentActivity.Title}");

            IsIdle = await IsIdleAsync(); 
            Console.WriteLine($"Is user Idle: {IsIdle}");
            Console.WriteLine($"Timestamp: {DateTime.Now}");

            var entry = new TrackingEntry()
            {
                Activity = CurrentActivity,
                IsIdle = IsIdle
            };
            LastScan = entry.CreatedOn;
            
            await repo.Add(entry);         
            OnNewTrackingData?.Invoke();
            await Task.Delay(FrequencyInSeconds * 1000);
        }
        
        cancellationFlag = false;
    }

    public bool IsHypridleInstalled()
    {
        var res = BashExec("which hypridle | grep 'no hypridle'");
        if (string.IsNullOrEmpty(res))
            return true;
        return false;
    }

    private async Task<bool> IsIdleAsync()
    {
        try
        {
            if (File.Exists(ipcPath))
                return (await File.ReadAllTextAsync(ipcPath))[..1] == "1"; 
            
            return false;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            OnIpcError?.Invoke();
        }
        return false;
    }

    private static Activity? GetCurrentActivity()
    {
        var result = BashExec("hyprctl activewindow -j");

        try
        {
            using var jsonDoc = JsonDocument.Parse(result);

            var activityTitle = jsonDoc.RootElement.GetProperty("title").GetString();
            var activityClass = jsonDoc.RootElement.GetProperty("class").GetString();

            if (activityTitle is null || activityClass is null)
                return null;
            
            return new Activity(activityTitle, activityClass);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(result);
            return null;
        }
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