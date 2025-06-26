using gtime.Models;

namespace gtime;

public interface IRepository
{
    Task Add(TrackingEntry entry);
    Task<TrackingEntry[]> GetDay();
    Task<TrackingEntry[]> GetDay(DateTime date);
}