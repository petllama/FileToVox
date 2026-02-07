using FileToVox.Converter;
using FileToVox.Converter.Image;
using FileToVox.Converter.PaletteSchematic;
using FileToVox.Converter.PointCloud;
using FileToVoxCore.Schematics;
using FileToVoxCore.Vox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FileToVox.Services
{
	public class ConversionService
	{
		private readonly ConversionOptions _options;
		private readonly Action<string> _log;

		public ConversionService(ConversionOptions options, Action<string> log)
		{
			_options = options;
			_log = log ?? (msg => { });
		}

		public void ValidateOptions()
		{
			if (_options.InputPath == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --i");
			if (_options.OutputPath == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --o");
			if (_options.GridSize < 10 || _options.GridSize > Schematic.MAX_WORLD_LENGTH)
				throw new ArgumentException("[ERROR] --grid-size argument must be greater than 10 and smaller than " + Schematic.MAX_WORLD_LENGTH);
			if (_options.HeightMap < 1)
				throw new ArgumentException("[ERROR] --heightmap argument must be positive");
			if (_options.ColorLimit < 0 || _options.ColorLimit > 256)
				throw new ArgumentException("[ERROR] --color-limit argument must be between 1 and 256");
			if (_options.ChunkSize <= 10 || _options.ChunkSize > 256)
				throw new ArgumentException("[ERROR] --chunk-size argument must be between 10 and 256");
		}

		public void DisplayArguments()
		{
			_log("[INFO] FileToVox v" + Assembly.GetExecutingAssembly().GetName().Version);
			_log("[INFO] Author: @Zarbuz. Contact : https://twitter.com/Zarbuz");

			if (_options.InputPath != null)
				_log("[INFO] Specified input path: " + _options.InputPath);
			if (_options.OutputPath != null)
				_log("[INFO] Specified output path: " + _options.OutputPath);
			if (_options.InputColorFile != null)
				_log("[INFO] Specified input color file: " + _options.InputColorFile);
			if (_options.InputPaletteFile != null)
				_log("[INFO] Specified palette file: " + _options.InputPaletteFile);
			if (_options.ColorLimit != 256)
				_log("[INFO] Specified color limit: " + _options.ColorLimit);
			if (_options.GridSize != 10)
				_log("[INFO] Specified grid size: " + _options.GridSize);
			if (_options.ChunkSize != 128)
				_log("[INFO] Specified chunk size: " + _options.ChunkSize);
			if (_options.Excavate)
				_log("[INFO] Enabled option: excavate");
			if (_options.Color)
				_log("[INFO] Enabled option: color");
			if (_options.HeightMap != 1)
				_log("[INFO] Enabled option: heightmap (value=" + _options.HeightMap + ")");
			if (_options.Debug)
				_log("[INFO] Enabled option: debug");
			if (_options.DisableQuantization)
				_log("[INFO] Enabled option: disable-quantization");

			_log("[INFO] Specified output path: " + Path.GetFullPath(_options.OutputPath));
		}

		public bool Run()
		{
			// Apply settings to shared state
			Schematic.CHUNK_SIZE = _options.ChunkSize;
			Schematic.DEBUG = _options.Debug;
			ConversionContext.DisableQuantization = _options.DisableQuantization;

			return ProcessFile();
		}

		private bool ProcessFile()
		{
			string path = Path.GetFullPath(_options.InputPath);
			bool isFolder = Directory.Exists(path);

			try
			{
				AbstractToSchematic converter;
				string[] files = _options.InputPath.Split(";");
				if (isFolder)
				{
					List<string> images = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
						.Where(s => s.EndsWith(".png") && !string.IsNullOrEmpty(s)).ToList();
					converter = new MultipleImageToSchematic(images, _options.Excavate, _options.InputColorFile, _options.ColorLimit);
					return SchematicToVox(converter);
				}
				if (files.Length > 1)
				{
					converter = new MultipleImageToSchematic(
						files.Where(s => s.EndsWith(".png") && !string.IsNullOrEmpty(s)).ToList(),
						_options.Excavate, _options.InputColorFile, _options.ColorLimit);
					return SchematicToVox(converter);
				}

				if (!File.Exists(path))
				{
					throw new FileNotFoundException("[ERROR] File not found at: " + path);
				}

				converter = GetConverter(path);
				if (converter != null)
				{
					return SchematicToVox(converter);
				}

				Console.WriteLine("[ERROR] Unsupported file extension !");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return false;
			}

			return true;
		}

		private AbstractToSchematic GetConverter(string path)
		{
			switch (Path.GetExtension(path))
			{
				case ".asc":
					return new ASCToSchematic(path);
				case ".binvox":
					return new BinvoxToSchematic(path);
				case ".csv":
					return new CSVToSchematic(path, _options.GridSize, _options.ColorLimit);
				case ".ply":
					return new PLYToSchematic(path, _options.GridSize, _options.ColorLimit);
				case ".png":
				case ".tif":
					return new ImageToSchematic(path, _options.InputColorFile, _options.HeightMap, _options.Excavate, _options.Color, _options.ColorLimit);
				case ".qb":
					return new QBToSchematic(path);
				case ".schematic":
					return new SchematicToSchematic(path, _options.Excavate);
				case ".xyz":
					return new XYZToSchematic(path, _options.GridSize, _options.ColorLimit);
				case ".vox":
					return new VoxToSchematic(path);
				case ".obj":
				case ".fbx":
					throw new Exception("[FAILED] Voxelization of 3D models is no longer done in FileToVox but with MeshToVox. Check the url : https://github.com/Zarbuz/FileToVox/releases for download link");
				default:
					return null;
			}
		}

		private bool SchematicToVox(AbstractToSchematic converter)
		{
			Schematic schematic = converter.WriteSchematic();
			Console.WriteLine($"[INFO] Vox Width: {schematic.Width}");
			Console.WriteLine($"[INFO] Vox Length: {schematic.Length}");
			Console.WriteLine($"[INFO] Vox Height: {schematic.Height}");

			if (schematic.Width > Schematic.MAX_WORLD_WIDTH || schematic.Length > Schematic.MAX_WORLD_LENGTH || schematic.Height > Schematic.MAX_WORLD_HEIGHT)
			{
				Console.WriteLine("[ERROR] Model is too big ! MagicaVoxel can't support model bigger than 2000x2000x1000");
				return false;
			}

			VoxWriter writer = new VoxWriter();

			if (_options.InputPaletteFile != null)
			{
				PaletteSchematicConverter converterPalette = new PaletteSchematicConverter(_options.InputPaletteFile);
				schematic = converterPalette.ConvertSchematic(schematic);
				return writer.WriteModel(FormatOutputDestination(_options.OutputPath), converterPalette.GetPalette(), schematic);
			}

			return writer.WriteModel(FormatOutputDestination(_options.OutputPath), null, schematic);
		}

		public void RunDebug()
		{
			VoxReader reader = new VoxReader();
			reader.LoadModel(FormatOutputDestination(_options.OutputPath));
		}

		public static string FormatOutputDestination(string outputPath)
		{
			outputPath = outputPath.Replace(".vox", "");
			outputPath += ".vox";
			return outputPath;
		}

		public static string DetectFormat(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return null;

			if (Directory.Exists(filePath))
				return "Folder (multiple PNG)";

			switch (Path.GetExtension(filePath)?.ToLowerInvariant())
			{
				case ".asc": return "ASC (ASCII Grid)";
				case ".binvox": return "Binvox";
				case ".csv": return "CSV (Point Cloud)";
				case ".ply": return "PLY (Point Cloud)";
				case ".png": return "PNG (Image/Heightmap)";
				case ".tif": return "TIFF (Image/Heightmap)";
				case ".qb": return "QB (Qubicle)";
				case ".schematic": return "Schematic (Minecraft)";
				case ".xyz": return "XYZ (Point Cloud)";
				case ".vox": return "VOX (MagicaVoxel)";
				case ".glb": return "GLB (3D Mesh via mesh2vox)";
				case ".gltf": return "GLTF (3D Mesh via mesh2vox)";
				case ".obj": return "OBJ (3D Mesh via mesh2vox)";
				case ".fbx": return "FBX (3D Mesh via mesh2vox)";
				case ".stl": return "STL (3D Mesh via mesh2vox)";
				case ".dae": return "DAE (3D Mesh via mesh2vox)";
				case ".3ds": return "3DS (3D Mesh via mesh2vox)";
				default: return "Unknown format";
			}
		}

		public static bool IsMeshFormat(string filePath)
		{
			if (string.IsNullOrEmpty(filePath))
				return false;

			switch (Path.GetExtension(filePath)?.ToLowerInvariant())
			{
				case ".glb":
				case ".gltf":
				case ".obj":
				case ".fbx":
				case ".stl":
				case ".dae":
				case ".3ds":
					return true;
				default:
					return false;
			}
		}
	}
}
