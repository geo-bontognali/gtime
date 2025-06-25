namespace gtime.Models;

public record Activity(string Title, string Class)
{
    public DateTime UtcDateTime { get; } = DateTime.UtcNow;
    public DateTime DateTime =>  UtcDateTime.ToLocalTime();
}