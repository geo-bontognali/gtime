using gtime.Models;
using gtime.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace gtime.Components.Pages;

public partial class Home
{
    [Inject]
    public required TrackingService TrackingService { get; set; }
    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    [Inject]
    public required IRepository Repo { get; set; }

    private IJSObjectReference? js;
    private TimelineEntry[] timelineEntries = [];

    protected override async Task OnInitializedAsync()
    {
        timelineEntries = await BuildTimeline();
        /*timelineEntries =
        [
            new TimelineEntry(){ Start="06:00", End="09:00", Type="active", Title="Morning Focus", Description="Implement new feature" },
            new TimelineEntry(){ Start="14:00", End="18:00", Type="afk", Title="away", Description="Implement new feature" }
        ];*/
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            js = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Home.razor.js"); 
            await js.InvokeVoidAsync("initTimeline", (object)timelineEntries);
        }
    }

    private async Task<TimelineEntry[]> BuildTimeline()
    {
        const int sliceLength = 5;                     // seconds per TrackingEntry
        var trackingEntries = (await Repo.GetDay())
                             .OrderBy(te => te.CreatedOn) // local time already
                             .ToList();

        if (trackingEntries.Count == 0)
            return [];

        var timeline = new List<TimelineEntry>();

        // seed the first group
        var groupStart   = trackingEntries[0];
        var prev         = groupStart;

        for (var i = 1; i < trackingEntries.Count; i++)
        {
            var curr = trackingEntries[i];

            var sameState     = curr.UserState == groupStart.UserState;
            var sameActivity  = 
                groupStart.UserState != UserState.Active || curr.Activity?.Class == groupStart.Activity?.Class
                && curr.Activity?.Title == groupStart.Activity?.Title; // AFK groups only care about state

            if (!sameState || !sameActivity)
            {
                // close current bundle
                timeline.Add(ToTimelineEntry(groupStart, prev));
                groupStart = curr;          // start a new bundle
            }

            prev = curr;                    // advance sliding window
        }

        // flush last bundle
        timeline.Add(ToTimelineEntry(groupStart, prev));

        return timeline.ToArray();

        // local function: converts a group’s first/last TrackingEntry into a TimelineEntry
        TimelineEntry ToTimelineEntry(TrackingEntry first, TrackingEntry last)
        {
            var endStamp = last.CreatedOn.AddSeconds(sliceLength); // inclusive visual range

            var isActive = first.UserState == UserState.Active;

            return new TimelineEntry
            {
                Start       = first.CreatedOn.ToString("HH:mm"),
                End         = endStamp.ToString("HH:mm"),
                Type        = isActive ? "active" : "afk",
                Title       = isActive ? first.Activity!.Class : "away",
                Description = isActive ? first.Activity!.Title : string.Empty
            };
        }
    }
}