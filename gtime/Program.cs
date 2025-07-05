using ElectronNET.API;
using gtime;
using gtime.Services;
using ElectronApp = gtime.Components.App;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddElectron();
builder.Services.AddSingleton<TrackingService>();
builder.Services.AddSingleton<TrayManager>();
builder.Services.AddSingleton<IRepository, JsonRepo>();
    
builder.WebHost.UseElectron(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<ElectronApp>()
    .AddInteractiveServerRenderMode();

var activityWatcher = app.Services.GetRequiredService<TrackingService>();
var trayManager = app.Services.GetRequiredService<TrayManager>();
var reloadCount = 0;
Task.Run(async () => { await activityWatcher.RunAsync(); }); 

if (HybridSupport.IsElectronActive)
{
    Task.Run(async () => {
        var window = await Electron.WindowManager.CreateWindowAsync();
        window.OnClosed += () => {
            Electron.App.Quit();
        };
        window.OnReadyToShow += () => {
            if (reloadCount < 1)
            {
                window.Reload();
                reloadCount++; 
            }
            else
            {
                trayManager.HideWindow();
            }
        };
    });
}

app.Run();