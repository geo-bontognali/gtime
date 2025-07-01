using gtime.Services;
using Microsoft.AspNetCore.Components;

namespace gtime.Components.Pages;

public partial class Debug : ComponentBase, IDisposable
{
    [Inject]
    public required TrackingService TrackingService { get; set; }
    
    protected override Task OnInitializedAsync()
    {
        TrackingService.OnNewTrackingData += Update;
        return base.OnInitializedAsync();
    }

    private async Task Update()
    {
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TrackingService.OnNewTrackingData -= Update;
    }
}