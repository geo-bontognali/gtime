using gtime.Models;

namespace gtime;

public class InMemoryRepo
{
    private List<TrackingEntry> trackingData = [];

    public IReadOnlyList<TrackingEntry> ReadAll() => trackingData.AsReadOnly();

    public TrackingEntry? Read(Guid id)
    {
        return trackingData.FirstOrDefault(x => x.Id == id);
    }
    
    public void Add(TrackingEntry entry)
    {
        trackingData.Add(entry);
    }
}