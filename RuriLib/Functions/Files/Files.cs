using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RuriLib.Functions.Files
{
    /// <summary>
    /// Provides methods to work with files.
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// Saves a Selenium screenshot to a file with automatically generated name.
        /// </summary>
        /// <param name="screenshot">The Selenium screenshot</param>
        /// <param name="data">The BotData used for path creation</param>
        public static void SaveScreenshot(OpenQA.Selenium.Screenshot screenshot, BotData data)
        {
            var path = MakeScreenshotPath(data);
            data.Screenshots.Add(path);
            screenshot.SaveAsFile(path);
        }

        /// <summary>
        /// Saves a screenshot to a file with automatically generated name.
        /// </summary>
        /// <param name="screenshot">The bitmap image</param>
        /// <param name="data">The BotData used for path creation</param>
        public static void SaveScreenshot(Bitmap screenshot, BotData data)
        {
            var path = MakeScreenshotPath(data);
            data.Screenshots.Add(path);
            screenshot.Save(path);
        }

        /// <summary>
        /// Saves a stream to a file with automatically generated name.
        /// </summary>
        /// <param name="stream">The input stream</param>
        /// <param name="data">The BotData used for path creation</param>
        public static void SaveScreenshot(MemoryStream stream, BotData data)
        {
            var path = MakeScreenshotPath(data);
            using (var fileStream = File.Create(path)) { stream.CopyTo(fileStream); }
            data.Screenshots.Add(path);
        }

        /// <summary>
        /// Builds the path for the screenshot file.
        /// </summary>
        /// <param name="data">The BotData for path creation</param>
        /// <returns>The path of the file to save the screenshot to</returns>
        private static string MakeScreenshotPath(BotData data)
        {
            var folderName = MakeValidFileName(data.ConfigSettings.Name);
            var originalFilename = MakeValidFileName(data.Data.Data);
            // Check if you have to make the folder
            if (!Directory.Exists($"Screenshots\\{folderName}")) Directory.CreateDirectory($"Screenshots\\{folderName}");

            // Save the file inside the folder
            var filename = GetFirstAvailableFileName($"Screenshots\\{folderName}\\", originalFilename, "bmp");
            return $"Screenshots\\{folderName}\\{filename}";
        }

        /// <summary>
        /// Gets the first available name in the given folder by incrementing a number at the end of the filename.
        /// </summary>
        /// <param name="basePath">The path to the folder</param>
        /// <param name="fileName">The name of the file without numbers at the end</param>
        /// <param name="extension">The extension of the file</param>
        /// <returns>The first available filename (including extension)</returns>
        public static string GetFirstAvailableFileName(string basePath, string fileName, string extension)
        {
            int i;
            for (i = 1; File.Exists(basePath + fileName + i + "." + extension); i++) { }
            return fileName + i + "." + extension;
        }

        /// <summary>
        /// Fixes the filename to be compatible with the filesystem indicization.
        /// </summary>
        /// <param name="name">The name of the file</param>
        /// <param name="underscore">Whether to replace the unallowed characters with an underscore instead of removing them</param>
        /// <returns>The valid filename ready to be saved to disk</returns>
        public static string MakeValidFileName(string name, bool underscore = true)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(name, invalidRegStr, underscore ? "_" : "");
        }

        /// <summary>
        /// Throws an UnauthorizedAccessException if the path is not part of the current working directory.
        /// </summary>
        /// <param name="path">The absolute or relative path.</param>
        public static void ThrowIfNotInCWD(string path)
        {
            if (!path.IsSubPathOf(Directory.GetCurrentDirectory()))
            {
                throw new UnauthorizedAccessException("For security reasons, you cannot interact with paths outside of the current working directory");
            }
        }

        /// <summary>
        /// Creates the folder structure that contains a certain files if it doesn't already exist.
        /// </summary>
        /// <param name="file">The absolute or relative path to the file.</param>
        public static void CreatePath(string file)
        {
            var dirName = Path.GetDirectoryName(file);

            if (!string.IsNullOrWhiteSpace(dirName) && !Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }
    }
}
