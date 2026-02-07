using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileToVox.Gui.Services;
using FileToVox.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileToVox.Gui.ViewModels
{
	public partial class MainWindowViewModel : ViewModelBase
	{
		private const string DEFAULT_MESH2VOX_SCRIPT = "/home/petllama/Voxel_projects/mesh2vox/mesh2vox.py";

		private CancellationTokenSource _cts;

		private static readonly FilePickerFileType AllSupportedTypes = new("All Supported Formats")
		{
			Patterns = new[] { "*.png", "*.tif", "*.asc", "*.binvox", "*.csv", "*.ply", "*.qb", "*.schematic", "*.xyz", "*.vox",
				"*.glb", "*.gltf", "*.obj", "*.fbx", "*.stl", "*.dae", "*.3ds" },
			MimeTypes = new[] { "image/png", "image/tiff", "model/gltf-binary", "model/gltf+json", "model/obj", "model/stl", "application/octet-stream" }
		};

		private static readonly FilePickerFileType ImageTypes = new("Images")
		{
			Patterns = new[] { "*.png", "*.tif" },
			MimeTypes = new[] { "image/png", "image/tiff" }
		};

		private static readonly FilePickerFileType PointCloudTypes = new("Point Clouds")
		{
			Patterns = new[] { "*.ply", "*.csv", "*.xyz", "*.asc" },
			MimeTypes = new[] { "application/octet-stream" }
		};

		private static readonly FilePickerFileType VoxelTypes = new("Voxel Formats")
		{
			Patterns = new[] { "*.binvox", "*.qb", "*.schematic", "*.vox" },
			MimeTypes = new[] { "application/octet-stream" }
		};

		private static readonly FilePickerFileType MeshTypes = new("3D Models (mesh2vox)")
		{
			Patterns = new[] { "*.glb", "*.gltf", "*.obj", "*.fbx", "*.stl", "*.dae", "*.3ds" },
			MimeTypes = new[] { "model/gltf-binary", "model/gltf+json", "model/obj", "model/stl", "application/octet-stream" }
		};

		private static readonly FilePickerFileType VoxType = new("VOX Files")
		{
			Patterns = new[] { "*.vox" },
			MimeTypes = new[] { "application/octet-stream" }
		};

		private static readonly FilePickerFileType AllFilesType = new("All Files")
		{
			Patterns = new[] { "*" },
			MimeTypes = new[] { "*/*" }
		};

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
		private string _inputPath = "";

		[ObservableProperty]
		[NotifyCanExecuteChangedFor(nameof(ConvertCommand))]
		private string _outputPath = "";

		[ObservableProperty]
		private string _inputColorFile = "";

		[ObservableProperty]
		private string _inputPaletteFile = "";

		[ObservableProperty]
		private bool _excavate;

		[ObservableProperty]
		private bool _color;

		[ObservableProperty]
		private bool _disableQuantization;

		[ObservableProperty]
		private bool _debug;

		[ObservableProperty]
		private int _colorLimit = 256;

		[ObservableProperty]
		private int _chunkSize = 128;

		[ObservableProperty]
		private int _heightMap = 1;

		[ObservableProperty]
		private float _gridSize = 10;

		[ObservableProperty]
		private int _meshResolution = 80;

		[ObservableProperty]
		private bool _isMeshInput;

		[ObservableProperty]
		private bool _isConverting;

		[ObservableProperty]
		private string _statusText = "Ready";

		[ObservableProperty]
		private string _detectedFormat = "";

		[ObservableProperty]
		private double _progress;

		public ObservableCollection<string> LogMessages { get; } = new();

		public IFileDialogService FileDialogService { get; set; }

		partial void OnInputPathChanged(string value)
		{
			DetectedFormat = ConversionService.DetectFormat(value) ?? "";
			IsMeshInput = ConversionService.IsMeshFormat(value);
			if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(OutputPath))
			{
				OutputPath = Path.ChangeExtension(value, ".vox");
			}
		}

		[RelayCommand]
		private async Task BrowseInput()
		{
			if (FileDialogService == null) return;
			string path = await FileDialogService.OpenFileAsync(
				"Select Input File",
				new[] { AllSupportedTypes, MeshTypes, ImageTypes, PointCloudTypes, VoxelTypes, AllFilesType });
			if (!string.IsNullOrEmpty(path))
			{
				InputPath = path;
			}
		}

		[RelayCommand]
		private async Task BrowseOutput()
		{
			if (FileDialogService == null) return;
			string path = await FileDialogService.SaveFileAsync(
				"Select Output File",
				new[] { VoxType, AllFilesType });
			if (!string.IsNullOrEmpty(path))
			{
				OutputPath = path;
			}
		}

		[RelayCommand]
		private async Task BrowseColorFile()
		{
			if (FileDialogService == null) return;
			string path = await FileDialogService.OpenFileAsync(
				"Select Color File",
				new[] { AllFilesType });
			if (!string.IsNullOrEmpty(path))
			{
				InputColorFile = path;
			}
		}

		[RelayCommand]
		private async Task BrowsePalette()
		{
			if (FileDialogService == null) return;
			string path = await FileDialogService.OpenFileAsync(
				"Select Palette File",
				new[] { AllFilesType });
			if (!string.IsNullOrEmpty(path))
			{
				InputPaletteFile = path;
			}
		}

		private bool CanConvert() => !string.IsNullOrWhiteSpace(InputPath) && !string.IsNullOrWhiteSpace(OutputPath) && !IsConverting;

		[RelayCommand(CanExecute = nameof(CanConvert))]
		private async Task Convert()
		{
			IsConverting = true;
			StatusText = "Converting...";
			Progress = 0;
			ConvertCommand.NotifyCanExecuteChanged();

			_cts = new CancellationTokenSource();

			var options = new ConversionOptions
			{
				InputPath = InputPath,
				OutputPath = OutputPath,
				InputColorFile = string.IsNullOrEmpty(InputColorFile) ? null : InputColorFile,
				InputPaletteFile = string.IsNullOrEmpty(InputPaletteFile) ? null : InputPaletteFile,
				Excavate = Excavate,
				Color = Color,
				DisableQuantization = DisableQuantization,
				Debug = Debug,
				ColorLimit = ColorLimit,
				ChunkSize = ChunkSize,
				HeightMap = HeightMap,
				GridSize = GridSize,
				MeshResolution = MeshResolution,
				Mesh2VoxScript = DEFAULT_MESH2VOX_SCRIPT,
			};

			bool isMesh = ConversionService.IsMeshFormat(InputPath);

			try
			{
				await Task.Run(() =>
				{
					var logger = new GuiLogger(msg =>
					{
						Avalonia.Threading.Dispatcher.UIThread.Post(() =>
						{
							LogMessages.Add(msg);
						});
					});

					Action<string> log = msg =>
					{
						Avalonia.Threading.Dispatcher.UIThread.Post(() =>
						{
							LogMessages.Add(msg);
						});
					};

					bool success;

					if (isMesh)
					{
						var meshService = new MeshConversionService(options, log);
						success = meshService.Run(_cts.Token);
					}
					else
					{
						TextWriter originalOut = Console.Out;
						Console.SetOut(logger);

						try
						{
							_cts.Token.ThrowIfCancellationRequested();

							var service = new ConversionService(options, msg =>
							{
								Console.WriteLine(msg);
							});

							service.ValidateOptions();
							service.DisplayArguments();
							success = service.Run();

							if (success && options.Debug)
							{
								service.RunDebug();
							}
						}
						finally
						{
							Console.SetOut(originalOut);
						}
					}

					Avalonia.Threading.Dispatcher.UIThread.Post(() =>
					{
						StatusText = success ? "Conversion complete!" : "Conversion failed.";
						Progress = 100;
					});
				}, _cts.Token);
			}
			catch (OperationCanceledException)
			{
				StatusText = "Conversion cancelled.";
				LogMessages.Add("[INFO] Conversion was cancelled by user.");
			}
			catch (Exception ex)
			{
				StatusText = "Error: " + ex.Message;
				LogMessages.Add("[ERROR] " + ex.Message);
			}
			finally
			{
				IsConverting = false;
				ConvertCommand.NotifyCanExecuteChanged();
			}
		}

		[RelayCommand]
		private void Cancel()
		{
			_cts?.Cancel();
		}

		[RelayCommand]
		private void ClearLog()
		{
			LogMessages.Clear();
		}

		public void HandleFileDrop(string filePath)
		{
			InputPath = filePath;
			if (string.IsNullOrEmpty(OutputPath))
			{
				OutputPath = Path.ChangeExtension(filePath, ".vox");
			}
		}
	}
}
