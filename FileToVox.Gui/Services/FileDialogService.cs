using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileToVox.Gui.Services
{
	public interface IFileDialogService
	{
		Task<string> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType> filters);
		Task<string> SaveFileAsync(string title, IReadOnlyList<FilePickerFileType> filters);
	}

	public class FileDialogService : IFileDialogService
	{
		private readonly Window _window;

		public FileDialogService(Window window)
		{
			_window = window;
		}

		public async Task<string> OpenFileAsync(string title, IReadOnlyList<FilePickerFileType> filters)
		{
			var result = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				Title = title,
				AllowMultiple = false,
				FileTypeFilter = filters
			});

			return result.Count > 0 ? result[0].Path.LocalPath : null;
		}

		public async Task<string> SaveFileAsync(string title, IReadOnlyList<FilePickerFileType> filters)
		{
			var result = await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
			{
				Title = title,
				FileTypeChoices = filters
			});

			return result?.Path.LocalPath;
		}
	}
}
