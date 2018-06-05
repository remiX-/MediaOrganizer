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
			get { return currentPath; }
			set { SetProperty(ref currentPath, value); }
		}
		public string OldPathDisplay
		{
			get { return currentPathDisplay; }
			set { SetProperty(ref currentPathDisplay, value); }
		}

		public FileInfo Destination
		{
			get { return destinationPath; }
			set
			{
				SetProperty(ref destinationPath, value);
				SetProperty(ref newPathDisplay, Path.Combine("~", destinationPath.FullName.Replace(rootPath, "")), "newPathDisplay");
			}
		}
		public string NewPathDisplay
		{
			get { return newPathDisplay; }
			set { SetProperty(ref newPathDisplay, value); }
		}

		public FileStatus Status
		{
			get { return status; }
			set { SetProperty(ref status, value); }
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