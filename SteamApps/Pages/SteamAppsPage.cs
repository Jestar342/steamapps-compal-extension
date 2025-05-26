// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Linq;
using System.Net;
using ABI.Windows.Management.Deployment.Preview;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Microsoft.Win32;

namespace SteamApps;

internal sealed partial class SteamAppsPage : ListPage
{
    public SteamAppsPage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png");
        Title = "Steam Applications";
        Name = "Open";
    }

    private record InstalledApp(int Installed, string? Name, string AppId);

    public override IListItem[] GetItems()
    {
        var appIds = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam\Apps")?.GetSubKeyNames();

        if (appIds == null || appIds.Length == 0)
        {
            return
            [
                new ListItem(new NoOpCommand()) { Title = "No Steam applications found." }
            ];
        }

        var installedApps = appIds.Select(appId =>
        {
            var installed = (int)Registry.GetValue(@$"{BaseKey}\{appId}", "Installed", 0)!;
            var name = Registry.GetValue($@"{BaseKey}\{appId}", "Name", null) as string;
            return new InstalledApp(installed, name, appId);
        }).Where(ia => ia.Installed is 1 && ia.Name is not null);

        return installedApps.Select(ia => new ListItem(new OpenUrlCommand($"steam://launch/{ia.AppId}/dialog")
        {
            Name = ia.Name!,
            Icon = IconHelpers.FromRelativePath("Assets\\StoreLogo.png"),
        })
        {
            Title = ia.Name!,
            Subtitle = $"Launch {ia.Name}"
        }).ToArray<IListItem>();
    }

    private const string BaseKey = @"HKEY_CURRENT_USER\Software\Valve\Steam\Apps";
}