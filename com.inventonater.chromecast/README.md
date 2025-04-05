# SharpCaster for Unity

A Unity-compatible implementation of Google Chromecast client library, based on [SharpCaster](https://github.com/Tapanila/SharpCaster).

## Overview

This package allows Unity applications to:
- Discover Chromecast devices on the network
- Connect to Chromecast devices
- Launch applications
- Control media playback
- Control volume and mute state
- Get device and application status

## Installation

### Via Unity Package Manager

1. Open your Unity project
2. Go to Window > Package Manager
3. Click the "+" button > "Add package from git URL..."
4. Enter the URL of this repository
5. Click "Add"

### Adding from OpenUPM

This package requires a dependency from OpenUPM, add the scoped registry to your project:

```json
"scopedRegistries": [
  {
    "name": "package.openupm.com",
    "url": "https://package.openupm.com",
    "scopes": [
      "com.cysharp.unitask",
      "com.stalomeow.google-protobuf"
    ]
  }
],
"dependencies": {
  "com.inventonater.chromecast": "1.0.0",
  "com.cysharp.unitask": "2.5.0",
  "com.stalomeow.google-protobuf": "3.21.12"
}
```

## Features

- **Device discovery**: Find Chromecast devices on your local network
- **Device connection**: Connect to Chromecast devices via SSL/TLS
- **Application launch**: Launch applications on Chromecast devices
- **Media control**: Play, pause, seek, control volume, etc.
- **Status monitoring**: Get real-time updates on Chromecast status
- **Unity integration**: Easy-to-use MonoBehaviour wrapper for Unity apps

## Basic Usage

### Using the ChromecastManager

The simplest way to use this package is with the `ChromecastManager` component:

```csharp
// Get a reference to the ChromecastManager component
var manager = GetComponent<ChromecastManager>();

// Discover devices
var devices = await manager.DiscoverDevicesAsync();

if (devices.Any())
{
    // Connect to the first device found
    await manager.ConnectToDeviceAsync(devices.First());
    
    // Launch the default media receiver app
    await manager.LaunchApplicationAsync("CC1AD845");
    
    // Load and play a media file
    await manager.LoadMediaAsync("https://example.com/video.mp4");
    
    // Pause playback
    await manager.PauseAsync();
    
    // Set volume
    await manager.SetVolumeAsync(0.5f);
}
```

### Advanced Usage

For more advanced use cases, you can use the `UnityChromecastClient` directly:

```csharp
// Create a client and locator
var locator = new UnityChromecastLocator();
var client = new UnityChromecastClient();

// Find devices
var devices = await locator.FindReceiversAsync();

if (devices.Any())
{
    // Connect to a device
    await client.ConnectChromecast(devices.First());
    
    // Launch an application
    await client.LaunchApplicationAsync("CC1AD845");
    
    // Load media
    var media = new Media
    {
        ContentUrl = "https://example.com/video.mp4",
        ContentType = "video/mp4",
        StreamType = StreamType.Buffered,
        Metadata = new MediaMetadata
        {
            MetadataType = MetadataType.Generic,
            Title = "My Video",
            Subtitle = "Video Subtitle"
        }
    };
    
    await client.MediaChannel.LoadAsync(media);
}
```

## Platform Support

- **Windows/macOS Editor**: Fully supported (uses simulated devices in editor)
- **Standalone Windows/macOS**: Supported
- **Android**: Basic support (requires platform-specific mDNS implementation)
- **iOS**: Basic support (requires platform-specific mDNS implementation)
- **WebGL**: Not supported (due to WebSocket limitations)

## Dependencies

- [UniTask](https://github.com/Cysharp/UniTask): For better async/await support in Unity
- [Google.Protobuf for Unity](https://openupm.com/packages/com.stalomeow.google-protobuf/): For Protocol Buffer handling

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Credits

- Based on [SharpCaster](https://github.com/Tapanila/SharpCaster) by Tapanila
