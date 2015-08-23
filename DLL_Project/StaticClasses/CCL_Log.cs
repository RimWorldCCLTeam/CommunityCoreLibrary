using System;
using System.Text;

namespace CommunityCoreLibrary
{
	internal static class CCL_Log
	{
		/// <summary>
		/// Write a log => Community Core Library :: category(nullable) :: content
		/// </summary>
		public static void Message(string category, string content)
		{
			var builder = new StringBuilder();
			builder.Append("Community Core Library:: ");

			if (category != null)
				builder.Append(category).Append(" :: ");

			builder.Append(content);

			Verse.Log.Message(builder.ToString());
		}

		/// <summary>
		/// Write an error => Community Core Library :: category(nullable) :: content
		/// </summary>
		public static void Error(string category, string content)
		{
			var builder = new StringBuilder();
			builder.Append("Community Core Library:: ");

			if (category != null)
				builder.Append(category).Append(" :: ");

			builder.Append(content);

			Verse.Log.Error(builder.ToString());
		}
	}
}
