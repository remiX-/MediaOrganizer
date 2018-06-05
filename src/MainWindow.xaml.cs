using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using Forms = System.Windows.Forms;

namespace MediaOrganizer
{
	public partial class MainWindow : MetroWindow
	{
		#region Variables
		private MainViewModel MyVM;

		public static string[] acceptableExentions = { ".mkv", ".avi", ".mp4", ".m2ts", ".wmv", ".mov", ".mpeg", ".flv" };
		private string[] stringsToRemove =
		{
			".x264", "264",
			".us", "us",
			".2009", "2009",
			".2010", "2010",
			".2011", "2011",
			".2012", "2012",
			".2013", "2013",
			".2014", "2014",
			".2015", "2015",
			".2016", "2016",
			".2017", "2017"
		};
		public static string[] wordsToNotCapitalize = { "Of" };
		private string[] qualities =
		{
			//"x264", "264", //??
			"360p", "360",
			"720p", "720",
			"1080p", "1080"
		};
		#endregion

		#region Window Events
		public MainWindow()
		{
			InitializeComponent();
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			MyVM = (MainViewModel)DataContext;

			// Theme
			var current = ThemeManager.DetectAppStyle();
			ThemeManager.ChangeAppStyle(Application.Current, current.Item2, ThemeManager.GetAppTheme(Properties.Settings.Default.AppTheme));
		}

		private void MetroWindow_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.AppTheme = MyVM.Theme;
			Properties.Settings.Default.SeriesSortFolder = MyVM.SeriesSortFolder;
			Properties.Settings.Default.SeriesMoveFolder = MyVM.SeriesMoveFolder;
			Properties.Settings.Default.SeriesMoveAuto = MyVM.SeriesMoveAuto;
			Properties.Settings.Default.MoviesSortFolder = MyVM.MoviesSortFolder;
			Properties.Settings.Default.MoviesListName = MyVM.MoviesListedName;
			Properties.Settings.Default.Save();
		}
		#endregion

		#region asd
		#endregion

		#region Series
		private void SeriesSortFolderBrowse_Click(object sender, EventArgs e)
		{
			Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog()
			{
				Description = "Select a directory",
				SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
				Tag = "wot",
				ShowNewFolderButton = true
			};
			if (!string.IsNullOrEmpty(MyVM.SeriesSortFolder)) fbd.SelectedPath = MyVM.SeriesSortFolder;

			if (fbd.ShowDialog() == Forms.DialogResult.OK)
			{
				MyVM.SeriesSortFolder = fbd.SelectedPath;
			}
		}

		private void SeriesMoveFolderBrowse_Click(object sender, EventArgs e)
		{
			Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog();
			if (!string.IsNullOrEmpty(MyVM.SeriesSortFolder)) fbd.SelectedPath = MyVM.SeriesSortFolder;

			if (fbd.ShowDialog() == Forms.DialogResult.OK)
			{
				MyVM.SeriesSortFolder = fbd.SelectedPath;
			}
		}

		private void SeriesLoad_Click(object sender, RoutedEventArgs e)
		{
			var q = Directory.EnumerateFiles(MyVM.SeriesSortFolder, "*.*", SearchOption.AllDirectories)
				.Where(f => acceptableExentions.Contains(Path.GetExtension(f).ToLower()))
				.Select(x => new VideoFile(MyVM.SeriesSortFolder, x))
				.ToList();
			MyVM.AddItemsToView(q);
		}

		private void SeriesStart_Click(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(MyVM.SeriesSortFolder))
				return;

			// Check to see if the folder is empty or not
			if (MyVM.Files.Count == 0)
			{
				DisplayMessage("No files found in '" + MyVM.SeriesSortFolder + "'.", "Error", false);
				return;
			}

			foreach (VideoFile file in MyVM.Files)
			{
				try
				{
					var oldName = Path.GetFileNameWithoutExtension(file.Current.Name).ToLower();

					foreach (string str in stringsToRemove)
					{
						oldName = oldName.Replace(str, "");
					}

					// Get the episode
					// Check to normal SxxExx first
					var fullEpisode = "";
					var index = 0;
					Match m = Regex.Match(oldName, @"s([0-9]+)e([0-9]+)", RegexOptions.IgnoreCase);
					if (m.Success)
					{
						fullEpisode = m.Value;
						index = oldName.IndexOf(fullEpisode);
					}
					else
					{
						// Format: s0e - 109
						m = Regex.Match(oldName, @"\d+");
						while (m.Success)
						{
							fullEpisode = m.Value;
							index = oldName.IndexOf(fullEpisode);

							if (m.Length == 3)
							{
								fullEpisode = "0" + fullEpisode;
								break;
							}

							m = m.NextMatch();
						}

						// Convert to SxxExx
						fullEpisode = "s" + fullEpisode.Substring(0, 2) + "e" + fullEpisode.Substring(2, 2);
					}

					var season = int.Parse(fullEpisode.Substring(1, 2));
					// Get name of the series and remove fullstops
					var seriesName = CommonMethods.ReplacePeriodsWithSpacesAndTrim(oldName.Substring(0, index));
					seriesName = CommonMethods.TitleCase(seriesName);

					var newFileName = seriesName + " " + fullEpisode.ToUpper() + file.Current.Extension; // Full name of the episode

					if (!MyVM.SeriesMoveAuto || string.IsNullOrWhiteSpace(MyVM.SeriesMoveFolder))
					{
						// Just rename the file and remains at the exact location it is in
						file.SetDestination(file.Current.DirectoryName, newFileName);
					}
					else
					{
						// Get new full path of where to move the series
						var seriesMoveDir = Path.Combine(MyVM.SeriesMoveFolder, seriesName, "Season " + season);

						if (!Directory.Exists(seriesMoveDir))
							Directory.CreateDirectory(seriesMoveDir);

						file.SetDestination(seriesMoveDir, newFileName);
					}

					if (!file.Destination.Exists)
					{
						file.Current.MoveTo(file.Destination.FullName);
						file.Status = FileStatus.Moved;
					}
					else
					{
						//file.NewPathDisplay = "Already Exists";
						file.Status = FileStatus.AlreadyExists;
					}
				}
				catch (Exception ex)
				{
					file.NewPathDisplay = "Error: " + ex.Message;
					file.Status = FileStatus.Error;
				}
			}
		}

		private void SeriesUndo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				foreach (VideoFile file in MyVM.Files)
				{
					file.Destination.Refresh();
					if (!file.Destination.Exists)
					{
						DisplayMessage("Failed to locate file with path '" + file.Destination.FullName + "'.", "Error");
						continue;
					}

					file.Destination.MoveTo(file.Current.FullName);
				}

				DisplayMessage("Moved all previously organized episodes back to it's original location.", "Success", false);
			}
			catch (Exception ex)
			{
				DisplayMessage("Something went wrong in btn_SeriesRedo_Click\n\n" + ex.Message, "Error");
			}
		}
		#endregion

		#region Movies
		private void MoviesSortFolderBrowse_Click(object sender, EventArgs e)
		{
			Forms.FolderBrowserDialog fbd = new Forms.FolderBrowserDialog()
			{
				Description = "Select a directory",
				SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
				Tag = "wot",
				ShowNewFolderButton = true
			};
			if (!string.IsNullOrEmpty(MyVM.MoviesSortFolder)) fbd.SelectedPath = MyVM.MoviesSortFolder;

			if (fbd.ShowDialog() == Forms.DialogResult.OK)
			{
				MyVM.MoviesSortFolder = fbd.SelectedPath;
			}
		}

		private void MoviesLoad_Click(object sender, RoutedEventArgs e)
		{
			var q = Directory.GetFiles(MyVM.MoviesSortFolder, "*.*", SearchOption.AllDirectories)
				.Where(f => acceptableExentions.Contains(Path.GetExtension(f).ToLower()))
				.Select(x => new VideoFile(MyVM.MoviesSortFolder, x))
				.ToList();
			MyVM.AddItemsToView(q);
		}

		private void MoviesStart_Click(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(MyVM.MoviesSortFolder))
				return;

			// Check to see if the folder is empty or not
			if (MyVM.Files.Count == 0)
			{
				DisplayMessage("No files found in '" + MyVM.MoviesSortFolder + "'.", "Error", false);
				return;
			}

			DirectoryInfo dirRoot = new DirectoryInfo(MyVM.MoviesSortFolder);
			foreach (VideoFile file in MyVM.Files)
			{
				try
				{
					DirectoryInfo dirParent = file.Current.Directory;
					
					var oldMovieName = Path.GetFileNameWithoutExtension(file.Current.Name).ToLower();
					var isDirectChildOfRootFolder = dirRoot.Name == dirParent.Name;
					string movieName, movieFullName;
					string year = "", quality = "";

					// Search for year and quality within file name
					Match m = Regex.Match(oldMovieName, @"\d+");
					while (m.Success)
					{
						if (qualities.Contains(m.Value))
							quality = m.Value;
						else if (m.Value.Length == 4)
							year = m.Value;

						m = m.NextMatch();
					}

					// If no year was found within the file name, try find it in directory name
					// The file must also not be a direct child of root folder
					if (string.IsNullOrWhiteSpace(year) && !isDirectChildOfRootFolder)
					{
						oldMovieName = dirParent.Name;

						m = Regex.Match(oldMovieName, @"\d+");
						while (m.Success)
						{
							if (qualities.Contains(m.Value))
								quality = m.Value;
							else if (m.Value.Length == 4)
								year = m.Value;

							m = m.NextMatch();
						}
					}

					// Get new name using reference of year or quality within the name
					if (!string.IsNullOrWhiteSpace(year))
						movieName = oldMovieName.Substring(0, oldMovieName.IndexOf(year) - 1);
					else if (!string.IsNullOrWhiteSpace(quality))
						movieName = oldMovieName.Substring(0, oldMovieName.IndexOf(quality) - 1);
					else
						movieName = oldMovieName; // no year or quality - no reference point from where to get name
					movieName = CommonMethods.ReplacePeriodsWithSpacesAndTrim(movieName);

					// Move "The" to end
					if (MyVM.MoviesListedName)
					{
						movieName = CommonMethods.GetListedName(movieName);
					}

					movieName = CommonMethods.TitleCase(movieName);
					movieFullName = movieName + file.Current.Extension;

					file.SetDestination(file.Current.DirectoryName, movieFullName);

					// Determine whether or not the file needs its own directory
					if (isDirectChildOfRootFolder)
					{
						DirectoryInfo dirNew = new DirectoryInfo(Path.Combine(dirParent.FullName, movieName + (!string.IsNullOrWhiteSpace(year) ? " (" + year + ")" : "")));
						if (!dirNew.Exists)	
							dirNew.Create();
						file.SetDestination(dirNew.FullName, movieFullName);
					}

					// Rename file
					if (!File.Exists(file.Destination.FullName))
					{
						file.Current.MoveTo(file.Destination.FullName);
						file.Status = FileStatus.Moved;
					}
					else
					{
						//file.NewPathDisplay = "Already exists";
						file.Status = FileStatus.AlreadyExists;
					}

					// Rename directory
					if (!string.IsNullOrWhiteSpace(year))
					{
						// The file is in its own folder
						// If year has a value, change parent folder to new name with year
						DirectoryInfo dirNew = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(file.Current.DirectoryName), movieName + " (" + year + ")"));

						if (file.Current.DirectoryName == dirNew.FullName)
						{
							// Already in appropriate folder
							file.SetDestination(file.Current.DirectoryName, movieFullName);
						}
						else
						{
							// Needs to be moved
							file.SetDestination(dirNew.FullName, movieFullName);

							if (!dirNew.Exists)
							{
								//Directory.Move(file.CurrentPath.DirectoryName, dirNew.FullName);
								Directory.Move(file.Current.DirectoryName, file.Destination.DirectoryName);
							}
						}
					}
					else
					{
						// Check whether or not the directory is named appropriately?
					}
				}
				catch (Exception ex)
				{
					file.NewPathDisplay = "Error: " + ex.Message;
					file.Status = FileStatus.Error;
				}
			}
		}

		private void MoviesUndo_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				foreach (VideoFile file in MyVM.Files)
				{
					file.Destination.Refresh();
					if (!file.Destination.Exists)
					{
						DisplayMessage("Failed to locate file with path '" + file.Destination.FullName + "'.", "Error");
						continue;
					}

					file.Destination.MoveTo(file.Current.FullName);
				}

				DisplayMessage("Moved all previously organized episodes back to it's original location.", "Success", false);
			}
			catch (Exception ex)
			{
				DisplayMessage("Something went wrong in btn_SeriesRedo_Click\n\n" + ex.Message, "Error");
			}
		}
		#endregion

		private void DisplayMessage(string text, string title, bool isError = true)
		{
			MessageBox.Show(text, title, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);
		}
	}
}