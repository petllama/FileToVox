using FileToVox.Services;
using FileToVoxCore.Schematics;
using NDesk.Options;
using System;
using System.Collections.Generic;

namespace FileToVox.Cli
{
	class Program
	{
		private static bool SHOW_HELP;

		public static void Main(string[] args)
		{
			ConversionOptions conversionOptions = new ConversionOptions();

			OptionSet options = new OptionSet()
			{
				{"i|input=", "input path", v => conversionOptions.InputPath = v},
				{"o|output=", "output path", v => conversionOptions.OutputPath = v},
				{"c|color", "enable color when generating heightmap", v => conversionOptions.Color = v != null},
				{"cm|color-from-file=", "load colors from file", v => conversionOptions.InputColorFile = v},
				{"cl|color-limit=", "set the maximal number of colors for the palette", (int v) => conversionOptions.ColorLimit = v},
				{"cs|chunk-size=", "set the chunk size", (int v) => conversionOptions.ChunkSize = v},
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air", v => conversionOptions.Excavate = v != null},
				{"h|help", "help informations", v => SHOW_HELP = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => conversionOptions.HeightMap = v},
				{"p|palette=", "set the palette", v => conversionOptions.InputPaletteFile = v},
				{"gs|grid-size=", "set the grid-size", (float v) => conversionOptions.GridSize = v},
				{"d|debug", "enable the debug mode", v => conversionOptions.Debug = v != null},
				{"dq|disable-quantization", "Disable the quantization step", v => conversionOptions.DisableQuantization = v != null},
			};

			try
			{
				List<string> extra = options.Parse(args);

				if (SHOW_HELP)
				{
					Console.WriteLine("Usage: FileToVox --i INPUT --o OUTPUT");
					Console.WriteLine("Options: ");
					options.WriteOptionDescriptions(Console.Out);
					Environment.Exit(0);
				}

				ConversionService service = new ConversionService(conversionOptions, msg => Console.WriteLine(msg));
				service.ValidateOptions();
				service.DisplayArguments();
				bool success = service.Run();

				if (success && conversionOptions.Debug)
				{
					service.RunDebug();
				}

				Console.WriteLine("[INFO] Done.");
				if (conversionOptions.Debug)
				{
					Console.ReadKey();
				}
			}
			catch (Exception e)
			{
				Console.Write("FileToVox: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `FileToVox --help` for more informations.");
				Console.ReadLine();
			}
		}
	}
}
