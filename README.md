# What is FileToVox?

FileToVox converts files into `.vox` files (MagicaVoxel). It is available as both a **command-line tool** and a **GUI application**.

## Supported Formats

- .asc (Esri ASCII raster format)
- .binvox
- .fbx
- .csv
- .json (House-made format)
- .obj
- .ply (Binary and ASCII)
- .png
- .schematic
- .tif
- .qb (Qubicle)
- .xyz (X Y Z R G B)
- folder (of .PNG)

FileToVox can import a folder of images (.PNG) where each image is a layer. (Useful for importing fractals from Mandelbulb3D)

It supports world regions, so you can convert a terrain bigger than 126^3 voxels!

## Project Structure

The solution contains three projects:

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

#### CLI Options

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

The GUI provides the same conversion features with a graphical interface — select input/output files, configure options, and monitor conversion progress with a log view.

# MeshToVox

MeshToVox is an external program from FileToVox that allows you to voxelize 3D object files.

Features:

- Load OBJ, FBX, GTLF, STL files
- Support textures and materials
- Export directly into .vox

More information [here](https://github.com/Zarbuz/FileToVox/wiki/7.-MeshToVox)

# Wiki

Please read the documentation of FileToVox [here](https://github.com/Zarbuz/FileToVox/wiki/1.-Introduction)

## Video Tutorials

- https://www.youtube.com/watch?v=sg3z2GaMJzM
- https://www.youtube.com/watch?v=fSo1iV1DE2U

## Renders
![](img/render.png)

![](img/EKGGrQaX0AAxg56.jfif)

![](img/EM3eWX2WoAABN5C.jfif)

![](img/EM9l60HW4AAa2ik.jfif)

