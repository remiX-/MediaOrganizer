using MahApps.Metro;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MediaOrganizer
{
	public class MainViewModel : BindableBase
	{
		#region Variables
		public ObservableCollection<VideoFile> Files { get; set; } = new ObservableCollection<VideoFile>();

		string theme;

		string seriesSortFolder;
		string seriesMoveFolder;
		bool seriesMoveAuto;

		string moviesSortFolder;
		bool moviesListedName;

		public string Theme
		{
			get { return theme; }
			set { SetProperty(ref theme, value); }
		}

		public string SeriesSortFolder
		{
			get { return seriesSortFolder; }
			set
			{
				if (value.Substring(value.Length - 1) != "\\")
					value += "\\";

				if (!Directory.Exists(value))
					value = string.Empty;

				SetProperty(ref seriesSortFolder, value);
			}
		}
		public string SeriesMoveFolder
		{
			get { return seriesMoveFolder; }
			set
			{
				if (value.Substring(value.Length - 1) != "\\")
					value += "\\";

				if (!Directory.Exists(value))
					value = string.Empty;

				SetProperty(ref seriesMoveFolder, value);
			}
		}
		public bool SeriesMoveAuto
		{
			get { return seriesMoveAuto; }
			set { SetProperty(ref seriesMoveAuto, value); }
		}

		public string MoviesSortFolder
		{
			get { return moviesSortFolder; }
			set
			{
				if (value.Substring(value.Length - 1) != "\\")
					value += "\\";

				if (!Directory.Exists(value))
					value = string.Empty;

				SetProperty(ref moviesSortFolder, value);
			}
		}
		public bool MoviesListedName
		{
			get { return moviesListedName; }
			set { SetProperty(ref moviesListedName, value); }
		}

		private DelegateCommand swapThemeCommand;
		public DelegateCommand SwapThemeCommand
		{
			get
			{
				if (swapThemeCommand == null)
				{
					swapThemeCommand = new DelegateCommand(
						() =>
						{
							var current = ThemeManager.DetectAppStyle();
							AppTheme inverse = ThemeManager.GetInverseAppTheme(current.Item1);
							ThemeManager.ChangeAppStyle(Application.Current, current.Item2, inverse);

							Theme = inverse.Name;
						}
					);
				}

				return swapThemeCommand;
			}
		}
		#endregion

		public MainViewModel()
		{
			Theme = Properties.Settings.Default.AppTheme;

			// Series
			seriesSortFolder = Properties.Settings.Default.SeriesSortFolder;
			seriesMoveFolder = Properties.Settings.Default.SeriesMoveFolder;
			seriesMoveAuto = Properties.Settings.Default.SeriesMoveAuto;

			// Movies
			moviesSortFolder = Properties.Settings.Default.MoviesSortFolder;
			moviesListedName = Properties.Settings.Default.MoviesListName;
		}

		public void AddItemsToView(List<VideoFile> files)
		{
			Files.Clear();
			Files.AddRange(files);
		}

		//public bool RemoveItemFromView(LocalImage item)
		//{
		//	return Uploads.Remove(item);
		//}

		//public bool HasFileInView(string filePath)
		//{
		//	return Uploads.FirstOrDefault(x => x.LocalPath == filePath) != null;
		//}

		//public bool UpdateUIForComplete(LocalImage image, ImgurImage image2)
		//{
		//	if (!Uploads.Remove(image))
		//		return false;

		//	CompleteUploads.Add(image2);

		//	return true;
		//}
	}
}