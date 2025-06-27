using System.Text.Json;
using gtime.Models;

namespace gtime;

public class JsonRepo : IRepository
{
    private List<TrackingEntry> trackingEntries = [];
    private string jsonPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".local", "gtime", "tracking"
    );
    private DateTime activeDay = DateTime.Today;
    
    public JsonRepo()
    {
        if (!Directory.Exists(jsonPath))
            Directory.CreateDirectory(jsonPath);

        LoadAsync(activeDay).GetAwaiter().GetResult();
    }
    
    private async Task LoadAsync(DateTime selectedDay)
    {
        var jsonFile = Path.Combine(jsonPath, selectedDay.ToString("dd-MM-yyyy") + ".json");
        if (!File.Exists(jsonFile))
            trackingEntries = [];
           
        var json = await File.ReadAllTextAsync(jsonFile);
        trackingEntries = JsonSerializer.Deserialize<List<TrackingEntry>>(json) ?? [];
    }
    
    public async Task SaveAsync()
    {
        var jsonFile = Path.Combine(jsonPath, DateTime.UtcNow.ToLocalTime().ToString("dd-MM-yyyy") + ".json");
        var json = JsonSerializer.Serialize(trackingEntries, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonFile, json);
    }

    public async Task Add(TrackingEntry entry)
    {
        if (activeDay.Date != DateTime.Today)
        {
            activeDay = DateTime.Today;
            await LoadAsync(activeDay);
        }
        trackingEntries.Add(entry);
        await SaveAsync();  
    }

    public async Task<List<TrackingEntry>> GetDay()
    {  
        if(activeDay.Date == DateTime.Today)
            return trackingEntries;
        
        activeDay = DateTime.Today;
        await LoadAsync(activeDay);
        return trackingEntries; 
    }

    public async Task<List<TrackingEntry>> GetDay(DateTime date)
    {
        if(activeDay.Date == date.Date)
            return trackingEntries;
        
        activeDay = date;
        await LoadAsync(activeDay);
        return trackingEntries; 
    }
}