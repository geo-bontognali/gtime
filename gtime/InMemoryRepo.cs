using gtime.Models;

namespace gtime;

public class InMemoryRepo : IRepository
{
    private List<TrackingEntry> trackingData = [];

    public async Task<TrackingEntry[]> GetDay()
    {
        return trackingData.ToArray();
    }

    public async Task<TrackingEntry[]> GetDay(DateTime date)
    {
        return trackingData.ToArray();
    }

    public async Task Add(TrackingEntry entry)
    {
        trackingData.Add(entry);
    }
}