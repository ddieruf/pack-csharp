# pack-csharp
A .NET (c#) wrapper library for the [Cloud Native Buildpacks](https://buildpacks.io/) pack cli.

## Whats going on

I borrowed the ideas of a process runner from the [`dotnet watch`](https://github.com/dotnet/sdk/tree/main/src/BuiltInTools/dotnet-watch/Internal) command and simplified a
bit. I also embedded a specific version of the [pack cli](https://buildpacks.io/docs/tools/pack/cli/pack/) within the package (which is why it's ~40mb) for linux, macos, and windows. During construction the correct binary
is reconstructed locally for the given OS. Then when either the `pack`, `inspect`, or `version` method is run a process spec is built and run in the background
using a runner. The output is fed back as a collection of lines for logging.

## Get started

Add the nuget distributed package to your project.

```powershell
dotnet add package pack-csharp
```

Initialize the constructor

```csharp
using pack-csharp

var logger = new LoggerFactory().CreateLogger<Pack>();
var cts = new CancellationTokenSource();
const bool quiet = false;
const bool verbose = true;
const bool timeStamps = true;

var pack = new Pack(cts.Token, logger, quiet, verbose, timeStamps);
```

Create an artifact to be containerized

```powershell
dotnet publish my-project -o "c:\pub"

# optionally you could zip it up and provide the full path to zip
# Compress-Archive -Path "c:\pub" -DestinationPath "c:\artifact.zip"
```

(make sure docker is running)

Build the container image

```csharp
var output = pack.Build("some-repo/my-image", "paketobuildpacks/builder:base", "c:\pub");
logger.LogInformation(string.Join(Environment.NewLine, output));
```

Inspect the resulting image

```csharp
var inspection = pack.Inspect("some-repo/my-image");
logger.LogInformation(inspection));
```

## Options

When running `pack.Build` you can optionally provide a `PackBuildSpec`. This offers all kinds of options to manipulate and configure the image and build environment.
