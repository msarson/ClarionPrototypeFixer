using System.IO;

namespace ClarionPrototypeFixer
{
    internal class UserSettings
    {
        public static string SolutionName { get; internal set; }
        public static string FilesFolder { get; internal set; }
        public static string SolutionPath()
        {
           
                return Path.GetDirectoryName(SolutionName);
           
        }
        public static string SaveToDirectory()
        {
            return SolutionPath() + "\\ConvertedAPV"; 
        }
    }
}