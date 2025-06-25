using gtime.Services;
using Microsoft.AspNetCore.Components;

namespace gtime.Components.Pages;

public partial class Home
{
    [Inject]
    public required TrackingService TrackingService { get; set; }
    [Inject]
    public required InMemoryRepo DB { get; set; }

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }
}