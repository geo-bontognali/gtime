using gtime.Models;

namespace gtime;

public interface IRepository
{
    Task Add(TrackingEntry entry);
    Task<List<TrackingEntry>> GetDay();
    Task<List<TrackingEntry>> GetDay(DateTime date);
}