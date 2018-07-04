using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ClarionPrototypeFixer
{
    class Program
    {
      

        [STAThread]
        static void Main(string[] args)
        {
            if (GetSolution())
            {
               
                if (Directory.Exists(UserSettings.SaveToDirectory()))
                    Directory.Delete(UserSettings.SaveToDirectory(), true);

                var filesToProcess = Directory.GetFiles(UserSettings.FilesFolder,
                                                   "*.apv",
                                                   SearchOption.AllDirectories);
                foreach (var file in filesToProcess)
                {
                    Console.WriteLine("Processing file: " + file);
                    var source = GetSourceFile(file);
                    var newSource = source;
                    var isRootAPV = !file.Contains("_");
                    newSource = (new PrototypeFixer(newSource, isRootAPV)).Source;
                    source = newSource;
                    SaveProcessedFile(file, source);


                }
            }

        }
        private static async void SaveProcessedFile(string file, string source)
        {
            var filename = file.Replace(UserSettings.FilesFolder,
                                        UserSettings.SaveToDirectory());
            Console.WriteLine("Saving File: " + filename);
            CreateDirectoryIfNecessary(filename);
            if (source != "")
                using (var writer = new StreamWriter(new FileStream(filename,
                                                                   FileMode.OpenOrCreate,
                                                                   FileAccess.ReadWrite),
                                                    Encoding.Default))
                {
                    await writer.WriteAsync(source);
                    writer.Close();
                }
            else
                await CopyFileAsync(file, filename);
            // File.Copy(file, filename);
        }
        private static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using (var sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            using (var destinationStream = new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
                await sourceStream.CopyToAsync(destinationStream);
        }
        private static void CreateDirectoryIfNecessary(string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
        private static string GetSourceFile(string file)
        {
            var source = string.Empty;
            using (var currentFileReader = new StreamReader(file,
                                                           Encoding.Default))
            {
                source = currentFileReader.ReadToEnd();
                currentFileReader.Close();
            }
            return source;
        }

        private static bool GetSolution()
        {
            var solutionFileDialog = new OpenFileDialog
            {
                Title = "Select Clarion Solution",
                FileName = string.Empty,
                Filter = "Clarion Solution Files|*.sln"
            };
            var dr = solutionFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                UserSettings.SolutionName = solutionFileDialog.FileName;

                var versionControlINI = Path.GetDirectoryName(solutionFileDialog.FileName) +
                                    "\\up_vcSettings.ini";
                var iniFile = new Ini(versionControlINI);
                UserSettings.FilesFolder = $"{Path.GetDirectoryName(versionControlINI)}\\{iniFile.GetValue("OutputFolder", "VC_InterFace").Remove(0, 2)}";
                return true;
            }
            return false;
        }
    }
}
