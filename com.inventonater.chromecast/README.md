# Chromecast for Unity

A Unity package for integrating Chromecast functionality into Unity applications. Based on the [SharpCaster](https://github.com/Tapanila/SharpCaster) library.

## Installation

### Requirements
- Unity 2022.3 or higher
- UniTask package (automatically installed via OpenUPM)

### Via Package Manager
1. Add the OpenUPM scoped registry to your project:
   - Open `Edit > Project Settings > Package Manager`
   - Add a new Scoped Registry:
     - Name: `OpenUPM`
     - URL: `https://package.openupm.com`
     - Scope(s): `com.cysharp.unitask`
2. Click Save
3. Open `Window > Package Manager`
4. Click the + button and select `Add package from git URL`
5. Enter the URL to this package repository

## Usage

```csharp
using Cysharp.Threading.Tasks;
using Inventonater.Chromecast;
using UnityEngine;

public class ChromecastExample : MonoBehaviour
{
    private async void Start()
    {
        // Find Chromecast devices on the network
        var locator = new UnityChromecastLocator();
        var devices = await locator.FindReceiversAsync();
        
        if (devices.Any())
        {
            var device = devices.First();
            Debug.Log($"Found Chromecast: {device.Name}");
            
            // Connect to the device
            var client = new ChromecastClient();
            await client.ConnectChromecast(device);
            
            // Launch application (Default Media Receiver)
            await client.LaunchApplicationAsync("CC1AD845");
            
            // Load a media file
            var media = new Media
            {
                ContentUrl = "https://commondatastorage.googleapis.com/gtv-videos-bucket/CastVideos/mp4/DesigningForGoogleCast.mp4"
            };
            await client.MediaChannel.LoadAsync(media);
        }
    }
}
```

## Limitations
- Currently optimized for Windows and macOS editor use
- Android support is in development
