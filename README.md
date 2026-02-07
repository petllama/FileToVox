# FileToVox (Fork)

> Forked from [Zarbuz/FileToVox](https://github.com/Zarbuz/FileToVox). See the original project for full documentation, wiki, and tutorials.

This fork adds an **Avalonia desktop GUI** and restructures the solution to target **.NET 8.0**.

## What Changed

- **Avalonia GUI** (`FileToVox.Gui`) — graphical interface for file conversion with file pickers, option controls, and a live log view
- **Restructured solution** — conversion logic extracted into a shared library (`SchematicToVoxCore`) used by both the CLI and GUI
- **Upgraded to .NET 8.0** across all projects

## Supported Formats

.asc, .binvox, .csv, .fbx, .json, .obj, .ply, .png, .qb, .schematic, .tif, .xyz, and folders of .PNG images.

See the [original wiki](https://github.com/Zarbuz/FileToVox/wiki/1.-Introduction) for detailed format documentation.

## Project Structure

| Project | Description |
|---|---|
| **SchematicToVoxCore** | Shared library containing all conversion logic |
| **FileToVox.Cli** | Command-line interface |
| **FileToVox.Gui** | Desktop GUI built with Avalonia |

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building

```bash
dotnet build FileToVox.sln
```

To create a self-contained release build for your platform:

```bash
# Linux
dotnet publish FileToVox.Cli/FileToVox.Cli.csproj -c Release -r linux-x64

# Windows
dotnet publish FileToVox.Cli/FileToVox.Cli.csproj -c Release -r win-x64

# macOS
dotnet publish FileToVox.Cli/FileToVox.Cli.csproj -c Release -r osx-x64
```

## Running

### CLI

```bash
dotnet run --project FileToVox.Cli -- --i INPUT --o OUTPUT
```

#### Options

| Option | Description |
|---|---|
| `-i`, `--input` | Input file path |
| `-o`, `--output` | Output file path |
| `-c`, `--color` | Enable color when generating heightmap |
| `-cm`, `--color-from-file` | Load colors from file |
| `-cl`, `--color-limit` | Max number of colors for the palette |
| `-cs`, `--chunk-size` | Set the chunk size |
| `-e`, `--excavate` | Delete voxels with no face connected to air |
| `-hm`, `--heightmap` | Create voxel terrain from heightmap (PNG only) |
| `-p`, `--palette` | Set the palette |
| `-gs`, `--grid-size` | Set the grid size |
| `-d`, `--debug` | Enable debug mode |
| `-dq`, `--disable-quantization` | Disable the quantization step |
| `-h`, `--help` | Show help |

#### Example

```bash
dotnet run --project FileToVox.Cli -- --i mymodel.obj --o output
```

### GUI

```bash
dotnet run --project FileToVox.Gui
```

Select input/output files, configure options, and monitor conversion progress with a log view.

## License

MIT — see [LICENSE](LICENSE). Original project by [Zarbuz](https://github.com/Zarbuz).
