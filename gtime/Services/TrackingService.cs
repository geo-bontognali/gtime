using System.Diagnostics;
using System.Text.Json;
using gtime.Models;
using Activity = gtime.Models.Activity;

namespace gtime.Services;

public class TrackingService
{
    public Activity? CurrentActivity { get; private set; }
    public event Func<Task> OnNewTrackingData = null!;
    public CursorPosition? CurrentCursorPosition { get; private set; }
    public UserState CurrentUserState { get; private set; } = UserState.Active;
    
    private bool cancellationFlag = false;
    private const int frequency = 5; // Target: 30sec
    private const int inactivityThreshold = 4;
    private readonly Queue<CursorPosition> cursorPositionBuffer = new (inactivityThreshold);
    private readonly IRepository repo;

    public TrackingService(IRepository repo)
    {
        this.repo = repo;
    }
    
    public async Task RunAsync()
    {
        while (!cancellationFlag)
        {
            CurrentActivity = GetCurrentActivity();
            if (CurrentActivity is not null)
                Console.WriteLine($"Current Activity: {CurrentActivity.Title}");
            
            CurrentCursorPosition = GetCurrentCursorPosition();
            if (CurrentCursorPosition is null)
            {
                CurrentUserState = UserState.Inactive;
            }
            else
            {
                Console.WriteLine($"Current Cursor Position: {CurrentCursorPosition}");
                CurrentUserState = GetUserState(CurrentCursorPosition); 
            }
            Console.WriteLine($"Current User State: {CurrentUserState}");

            var entry = new TrackingEntry()
            {
                Activity = CurrentActivity,
                UserState = CurrentUserState
            };
            await repo.Add(entry);         
            
            
            if(OnNewTrackingData is not null)
                await OnNewTrackingData.Invoke();

            await Task.Delay(frequency * 1000);
        }
        
        cancellationFlag = false;
    }

    public void Stop()
    {
        cancellationFlag = true;
    }

    private UserState GetUserState(CursorPosition currentCursorPosition)
    {
        if(cursorPositionBuffer.Count >= inactivityThreshold)
            cursorPositionBuffer.Dequeue();
        
        cursorPositionBuffer.Enqueue(currentCursorPosition);

        if (cursorPositionBuffer.Count < inactivityThreshold)
            return UserState.Active;
        
        for (var i = (cursorPositionBuffer.Count - 1); i > 0; i--)
            if (cursorPositionBuffer.ToArray()[i] != cursorPositionBuffer.ToArray()[i - 1])
                return UserState.Active;
        
        return UserState.Inactive;
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

    private static CursorPosition? GetCurrentCursorPosition()
    {
        var result = BashExec("hyprctl cursorpos");
        
        try
        {
            var x = int.Parse(result.Split(", ")[0]);
            var y = int.Parse(result.Split(", ")[1]);
            return new CursorPosition(x, y);
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