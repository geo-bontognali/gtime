using gtime.Models;

namespace gtime;

public class InMemoryRepo : IRepository
{
    private List<TrackingEntry> trackingData = [];

    public async Task<List<TrackingEntry>> GetDay()
    {
        return trackingData.ToList();
    }

    public async Task<List<TrackingEntry>> GetDay(DateTime date)
    {
        return trackingData.ToList();
    }

    public async Task Add(TrackingEntry entry)
    {
        trackingData.Add(entry);
    }
}