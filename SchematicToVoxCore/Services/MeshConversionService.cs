using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FileToVox.Services
{
	public class MeshConversionService
	{
		private readonly ConversionOptions _options;
		private readonly Action<string> _log;

		public MeshConversionService(ConversionOptions options, Action<string> log)
		{
			_options = options;
			_log = log ?? (msg => { });
		}

		public bool Run(CancellationToken cancellationToken = default)
		{
			string scriptPath = _options.Mesh2VoxScript;
			string pythonPath = _options.Mesh2VoxPython;

			if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
			{
				_log("[ERROR] mesh2vox.py not found at: " + (scriptPath ?? "(not set)"));
				_log("[ERROR] Please set the mesh2vox.py path in settings.");
				return false;
			}

			if (string.IsNullOrEmpty(pythonPath))
			{
				pythonPath = "python3";
			}

			string inputPath = Path.GetFullPath(_options.InputPath);
			if (!File.Exists(inputPath))
			{
				_log("[ERROR] Input file not found: " + inputPath);
				return false;
			}

			string outputDir = Path.GetDirectoryName(Path.GetFullPath(_options.OutputPath));
			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}

			int resolution = Math.Clamp(_options.MeshResolution, 1, 256);

			_log($"[INFO] Converting 3D mesh via mesh2vox (resolution={resolution})");
			_log($"[INFO] Input: {inputPath}");
			_log($"[INFO] Output directory: {outputDir}");

			string arguments = $"\"{scriptPath}\" \"{inputPath}\" --resolution {resolution} --output-dir \"{outputDir}\"";

			var startInfo = new ProcessStartInfo
			{
				FileName = pythonPath,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
			};

			// Activate venv if the script is inside one
			string venvDir = Path.GetDirectoryName(scriptPath);
			string venvBin = Path.Combine(venvDir, "venv", "bin");
			if (Directory.Exists(venvBin))
			{
				string venvPython = Path.Combine(venvBin, "python3");
				if (File.Exists(venvPython))
				{
					startInfo.FileName = venvPython;
					_log("[INFO] Using venv Python: " + venvPython);
				}
			}

			try
			{
				using var process = new Process { StartInfo = startInfo };

				process.OutputDataReceived += (sender, e) =>
				{
					if (e.Data != null) _log(e.Data);
				};
				process.ErrorDataReceived += (sender, e) =>
				{
					if (e.Data != null) _log("[mesh2vox] " + e.Data);
				};

				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				while (!process.HasExited)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						process.Kill(entireProcessTree: true);
						_log("[INFO] mesh2vox process cancelled.");
						return false;
					}
					process.WaitForExit(200);
				}

				if (process.ExitCode != 0)
				{
					_log($"[ERROR] mesh2vox exited with code {process.ExitCode}");
					return false;
				}

				// mesh2vox writes output as inputname.vox in the output dir
				// Rename if the user's desired output name differs
				string expectedOutput = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(inputPath) + ".vox");
				string desiredOutput = ConversionService.FormatOutputDestination(_options.OutputPath);

				if (!string.Equals(expectedOutput, desiredOutput, StringComparison.OrdinalIgnoreCase)
					&& File.Exists(expectedOutput))
				{
					if (File.Exists(desiredOutput))
						File.Delete(desiredOutput);
					File.Move(expectedOutput, desiredOutput);
					_log($"[INFO] Output renamed to: {desiredOutput}");
				}

				_log("[INFO] mesh2vox conversion complete!");
				return true;
			}
			catch (Exception ex)
			{
				_log("[ERROR] Failed to run mesh2vox: " + ex.Message);
				return false;
			}
		}
	}
}
