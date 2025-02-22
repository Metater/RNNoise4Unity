# RNNoise4Unity

Unity UPM package for RNNoise audio denoising. Includes native libraries for Android, iOS and Windows.

This package has been put together using the following repositories:
* [gsgou/RNNoiseSharp](https://github.com/gsgou/RNNoiseSharp)
* [Yellow-Dog-Man/RNNoise.Net](https://github.com/Yellow-Dog-Man/RNNoise.Net)

## Installation
Ensure you have the NPM registry in the `packages.json` file of your Unity project with `com.adrenak.rnnoise4unity` as one of the scopes
```
"scopedRegistries": [
    {
        "name": "npmjs",
        "url": "https://registry.npmjs.org",
        "scopes": [
            "com.npmjs",
            "com.adrenak.rnnoise4unity"
        ]
    }
}
```

Add `"com.adrenak.rnnoise4unity" : "x.y.z"` to `dependencies` list in `packages.json` file where `x.y.z` is the version name.  
💬 You can see the versions on the NPM page [here](https://www.npmjs.com/package/com.adrenak.rnnoise4unity?activeTab=versions).  

# License
BSD 3-Clause