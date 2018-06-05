namespace MediaOrganizer
{
	public static class CommonMethods
	{
		public static string ReplacePeriodsWithSpacesAndTrim(string str)
		{
			return str.Replace(".", " ").Trim();
		}

		public static string TitleCase(string s)
		{
			s = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());

			// De-capitalize specific words
			foreach (string word in MainWindow.wordsToNotCapitalize)
			{
				s = s.Replace(word, word.ToLower());
			}

			return s;
		}

		public static string GetListedName(string s)
		{
			if (s.ToLower().Substring(0, 3) == "the")
			{
				return s.Substring(4) + ", The";
			}

			return s;
		}
	}
}
