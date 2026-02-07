namespace FileToVox.Services
{
	public class ConversionOptions
	{
		public string InputPath { get; set; }
		public string OutputPath { get; set; }
		public string InputColorFile { get; set; }
		public string InputPaletteFile { get; set; }

		public bool Excavate { get; set; }
		public bool Color { get; set; }
		public bool DisableQuantization { get; set; }
		public bool Debug { get; set; }

		public float GridSize { get; set; } = 10;
		public int HeightMap { get; set; } = 1;
		public int ColorLimit { get; set; } = 256;
		public int ChunkSize { get; set; } = 128;

		// mesh2vox options
		public int MeshResolution { get; set; } = 80;
		public string Mesh2VoxScript { get; set; }
		public string Mesh2VoxPython { get; set; }
	}
}
