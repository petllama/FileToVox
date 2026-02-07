using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileToVox.Gui.Services;
using FileToVox.Gui.ViewModels;
using System.Collections.Specialized;
using System.Linq;

namespace FileToVox.Gui.Views
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			AddHandler(DragDrop.DropEvent, OnDrop);
			AddHandler(DragDrop.DragOverEvent, OnDragOver);
		}

		protected override void OnLoaded(RoutedEventArgs e)
		{
			base.OnLoaded(e);

			if (DataContext is MainWindowViewModel vm)
			{
				vm.FileDialogService = new FileDialogService(this);

				// Auto-scroll log to bottom
				vm.LogMessages.CollectionChanged += OnLogMessagesChanged;
			}
		}

		private void OnLogMessagesChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				var listBox = this.FindControl<ListBox>("LogListBox");
				if (listBox != null && listBox.ItemCount > 0)
				{
					listBox.ScrollIntoView(listBox.ItemCount - 1);
				}
			}
		}

		private void OnDragOver(object sender, DragEventArgs e)
		{
			e.DragEffects = DragDropEffects.Copy;
			e.Handled = true;
		}

		private void OnDrop(object sender, DragEventArgs e)
		{
			if (e.Data.Contains(DataFormats.Files))
			{
				var files = e.Data.GetFiles();
				var firstFile = files?.FirstOrDefault();
				if (firstFile != null && DataContext is MainWindowViewModel vm)
				{
					vm.HandleFileDrop(firstFile.Path.LocalPath);
				}
			}
			e.Handled = true;
		}
	}
}
