using ElectronNET.API;
using ElectronNET.API.Entities;
using gtime.Models;
using gtime.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace gtime.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    public required TrackingService TrackingService { get; set; }
    [Inject]
    public IJSRuntime JSRuntime { get; set; }
    [Inject]
    public required IRepository Repo { get; set; }


    private IJSObjectReference? js;
    private TimelineEntry[] timelineEntries = [];
    private bool showMissingDependencyWarning = false;

    protected override async Task OnInitializedAsync()
    {
        timelineEntries = await BuildTimelineAsync();
        showMissingDependencyWarning = !TrackingService.IsHypridleInstalled();
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            js = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Home.razor.js"); 
            await js.InvokeVoidAsync("initTimeline", (object)timelineEntries);
        }
    }

    private async Task<TimelineEntry[]> BuildTimelineAsync()
    {
        var sliceLength = TrackingService.FrequencyInSeconds;
        var trackingEntries = (await Repo.GetDay())
                             .OrderBy(te => te.CreatedOn) 
                             .ToList();

        if (trackingEntries.Count == 0)
            return [];

        var timeline = new List<TimelineEntry>();

        var groupStart   = trackingEntries[0];
        var prev         = groupStart;

        for (var i = 1; i < trackingEntries.Count; i++)
        {
            var curr = trackingEntries[i];

            var sameState     = curr.IsIdle == groupStart.IsIdle;
            var sameActivity  = 
                groupStart.IsIdle || curr.Activity?.Class == groupStart.Activity?.Class
                && curr.Activity?.Title == groupStart.Activity?.Title; 

            if (!sameState || !sameActivity)
            {
                timeline.Add(ToTimelineEntry(groupStart, prev));
                groupStart = curr;          
            }

            prev = curr;                   
        }

        timeline.Add(ToTimelineEntry(groupStart, prev));

        return timeline.ToArray();

        TimelineEntry ToTimelineEntry(TrackingEntry first, TrackingEntry last)
        {
            var endStamp = last.CreatedOn.AddSeconds(sliceLength);
            var isActive = !first.IsIdle; 

            return new TimelineEntry
            {
                Start       = first.CreatedOn.ToString("HH:mm"),
                End         = endStamp.ToString("HH:mm"),
                Type        = isActive ? "active" : "afk",
                Title       = isActive ? (first.Activity is not null) ? first.Activity.Class : "unknow" : "away",
                Description = isActive ? (first.Activity is not null) ? first.Activity.Title : string.Empty : string.Empty
            };
        }
    }
}