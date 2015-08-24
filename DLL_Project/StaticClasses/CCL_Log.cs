using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    internal static class CCL_Log
    {
        /// <summary>
        /// Write a log => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  Message(string content, string category = null)
        {
            var builder = new StringBuilder();
            builder.Append("Community Core Library :: ");

            if (category != null)
                builder.Append(category).Append(" :: ");

            builder.Append(content);

            Verse.Log.Message(builder.ToString());
        }

        /// <summary>
        /// Write a log(verbose only) => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  MessageVerbose(string content, string category = null)
        {
            if (!Prefs.LogVerbose)
                return;

            var builder = new StringBuilder();
            builder.Append("Community Core Library :: ");

            if (category != null)
                builder.Append(category).Append(" :: ");

            builder.Append(content);

            Verse.Log.Message(builder.ToString());
        }

        /// <summary>
        /// Write an error => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  Error(string content, string category = null)
        {
            var builder = new StringBuilder();
            builder.Append("Community Core Library :: ");

            if (category != null)
                builder.Append(category).Append(" :: ");

            builder.Append(content);

            Verse.Log.Error(builder.ToString());
        }
    }
}