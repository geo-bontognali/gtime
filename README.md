# GTIME

## Requirements

- .NET9 Runtime
- hyprland
- hypridle
- jq

## How to use

### Minimize

On Hyprland windows can't be minimized like in conventional window managers.
In order to keep gtime out of the way it's sent to a +10 workspace at startup. Use the tray icon
to toggle it in and out of the current workspace.

## Development
This is a .NET 9 Blazor Server solution wrapped with ElectronNET.
Depends on the dotnet ElectronNET.CLI tool

### Hot reload

```
electronize start /watch
```

### Build

```
electronize build /target win
electronize build /target osx
electronize build /target linux
``` 


