# FileToVox GUI

A cross-platform graphical interface for FileToVox, built with Avalonia UI.

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Running

From the repository root:

```bash
dotnet run --project FileToVox.Gui
```

Or build first, then run the binary directly:

```bash
dotnet build -c Release
./FileToVox.Gui/bin/Release/net8.0/FileToVox.Gui
```

## Usage

### 1. Select Files

- **Input**: Click **Browse** or drag and drop a file onto the window. Supported formats:
  - `.png`, `.tif` (image/heightmap)
  - `.ply`, `.csv`, `.xyz`, `.asc` (point cloud)
  - `.binvox`, `.qb`, `.schematic`, `.vox` (voxel formats)
  - A folder containing multiple `.png` files
- **Output**: Auto-populated as a `.vox` file alongside the input. Change it with **Browse** if needed.

The detected input format is shown below the input field.

### 2. Configure Options

All options are the same as the CLI flags:

| Option | Description | Default |
|--------|-------------|---------|
| **Color** | Enable color when generating heightmaps from PNG/TIFF | Off |
| **Excavate** | Remove voxels with no air-exposed faces | Off |
| **Debug** | Read back the generated file after conversion | Off |
| **Disable Quantization** | Skip color quantization (keeps first 255 unique colors as-is) | Off |
| **Color File** | Load colors from an external file | None |
| **Palette** | Use a custom MagicaVoxel palette | None |
| **Color Limit** | Maximum palette colors (0-256) | 256 |
| **Chunk Size** | Voxel chunk size (10-256) | 128 |
| **Heightmap Factor** | Height multiplier for PNG heightmaps | 1 |
| **Grid Size** | Grid size for point cloud formats (10-2000) | 10 |

### 3. Convert

Click **Convert** to start. Progress and log messages appear in the bottom panel. Click **Cancel** to stop a running conversion, or **Clear Log** to reset the log output.

### 4. Output

The resulting `.vox` file can be opened in [MagicaVoxel](https://ephtracy.github.io/).

## CLI

The original command-line interface is still available:

```bash
dotnet run --project FileToVox.Cli -- --i input.png --o output.vox
```

Run with `--help` for all options.
