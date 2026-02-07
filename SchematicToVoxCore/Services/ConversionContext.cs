namespace FileToVox.Services
{
	public static class ConversionContext
	{
		[System.ThreadStatic]
		public static bool DisableQuantization;
	}
}
