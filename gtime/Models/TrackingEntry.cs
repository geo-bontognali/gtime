namespace gtime.Models;

public record TrackingEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Activity? Activity { get; set; }
    public UserState UserState { get; set; }
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime CreatedOn =>  CreatedOnUtc.ToLocalTime();
    
}