using Prism.Mvvm;
using System.IO;

namespace MediaOrganizer
{
	public class VideoFile : BindableBase
	{
		#region Variables
		private string rootPath;

		private FileInfo currentPath;
		private string currentPathDisplay;

		private FileInfo destinationPath;
		private string newPathDisplay;

		private FileStatus status;

		public FileInfo Current
		{
			get => currentPath;
			set => SetProperty(ref currentPath, value);
		}
		public string OldPathDisplay
		{
			get => currentPathDisplay;
			set => SetProperty(ref currentPathDisplay, value);
		}

		public FileInfo Destination
		{
			get => destinationPath;
			set
			{
				SetProperty(ref destinationPath, value);
				SetProperty(ref newPathDisplay, Path.Combine("~", destinationPath.FullName.Replace(rootPath, "")), "newPathDisplay");
			}
		}
		public string NewPathDisplay
		{
			get => newPathDisplay;
			set => SetProperty(ref newPathDisplay, value);
		}

		public FileStatus Status
		{
			get => status;
			set => SetProperty(ref status, value);
		}
		#endregion

		public VideoFile(string rootParent, string fullPath)
		{
			rootPath = rootParent;
			status = FileStatus.Ready;

			currentPath = new FileInfo(fullPath);
			OldPathDisplay = Path.Combine("~", currentPath.FullName.Replace(rootPath, ""));
		}

		public void SetDestination(string path)
		{
			Destination = new FileInfo(path);
		}

		public void SetDestination(string directory, string fileName)
		{
			Destination = new FileInfo(Path.Combine(directory, fileName));
		}
	}

	public enum FileStatus
	{
		Ready,
		Moved,
		ErrorInUse,
		Error,
		AlreadyExists
	}
}