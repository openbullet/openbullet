/*****************
 * Author: Boris Dongarov (ideafixxxer)
 *  
 * http://www.codeproject.com/Members/ideafixxxer
 * 
******************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenBullet
{
    /// <summary>
    /// Sorts lines in huge text files
    /// </summary>
    public class HugeFileSort
    {
        #region Fields

        long maxFileSize = 100 * 1024 * 1024;
        //This collection maps string corresponding to the beginning of the lines in the input file 
        //to temp files containing sorted lines starting from this string, 
        //and to unsorted files containing only this strings 
        SortedDictionary<string, ChunkInfo> chunks;
        static int fileCounter;

        #endregion

        #region Properties

        /// <summary>
        /// Comparer that is used to sort strings in the file
        /// </summary>
        public StringComparer Comparer { get; set; }

        /// <summary>
        /// Input file encoding
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Maximum size of a file that can be loaded into memory for processing
        /// </summary>
        public long MaxFileSize { get { return maxFileSize; } set { maxFileSize = value; } }

        #endregion

        #region Nested Types

        /// <summary>
        /// Contains information related to beginning of lines in the file
        /// </summary>
        class ChunkInfo
        {
            StreamWriter noSortWriter;
            string noSortFileName;

            /// <summary>
            /// The name of the file, containing sorted lines starting with the given string
            /// </summary>
            public string FileName;

            /// <summary>
            /// Adds a small string to file <see cref="NoSortFileName"/>
            /// </summary>
            /// <param name="str"></param>
            public void AddSmallString(string str, Encoding encoding)
            {
                if (noSortWriter == null)
                {
                    noSortFileName = GenerateFileName();
                    noSortWriter = new StreamWriter(noSortFileName, false, encoding);
                }
                noSortWriter.WriteLine(str);
            }

            /// <summary>
            /// Closes opened stream
            /// </summary>
            public void Close()
            {
                if (noSortWriter != null) noSortWriter.Close();
            }

            /// <summary>
            /// The name of the file, containing only the given string
            /// </summary>
            public string NoSortFileName { get { return noSortFileName; } }

            private string GenerateFileName()
            {
                return "tmp\\n" + fileCounter++ + ".txt";
            }
        }

        /// <summary>
        /// Contains information about file chunk
        /// </summary>
        class FileChunk
        {
            StreamWriter writer;
            long size;
            string fileName;

            public FileChunk(Encoding encoding)
            {
                fileName = GenerateFileName();
                writer = new StreamWriter(fileName, false, encoding);
            }

            private string GenerateFileName()
            {
                return "tmp\\s" + fileCounter++ + ".txt";
            }

            /// <summary>
            /// Appends the line to the file
            /// </summary>
            /// <param name="entry">Text line to append</param>
            public void Append(string entry, Encoding encoding)
            {
                writer.WriteLine(entry);
                size += encoding.GetByteCount(entry) + encoding.GetByteCount(Environment.NewLine);
            }

            /// <summary>
            /// File size in bytes
            /// </summary>
            public long Size { get { return size; } }

            /// <summary>
            /// File name
            /// </summary>
            public string FileName { get { return fileName; } }

            /// <summary>
            /// Closes file stream
            /// </summary>
            public void Close()
            {
                writer.Close();
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="HugeFileSort"/>
        /// </summary>
        public HugeFileSort()
        {
            Comparer = StringComparer.CurrentCulture;
            Encoding = Encoding.UTF8;
        }

        /// <summary>
        /// Performs sorting
        /// </summary>
        /// <param name="inputFileName">The name of the file that needs to be sorted</param>
        /// <param name="outputFileName">The name of the file in which sorted lines from the <paramref name="inputFileName"/> should be saved</param>
        public void Sort(string inputFileName, string outputFileName)
        {
            chunks = new SortedDictionary<string, ChunkInfo>(Comparer);
            var info = new FileInfo(inputFileName);
            //If file size is less than maxFileSize simply sort its content in memory.
            if (info.Length < maxFileSize)
                SortFile(inputFileName, outputFileName);
            //Otherwise create temp directory and split file into chunks, saving each chunk as a new file into this directory
            else
            {
                var dir = new DirectoryInfo("tmp");
                if (dir.Exists)
                    dir.Delete(true);
                dir.Create();
                SplitFile(inputFileName, 1);
                Merge(outputFileName);
            }
        }

        /// <summary>
        /// Merges all chunks into one file
        /// </summary>
        private void Merge(string outputFileName)
        {
            using (var output = File.Create(outputFileName))
            {
                foreach (var name in chunks)
                {
                    name.Value.Close();
                    if (name.Value.NoSortFileName != null)
                    {
                        CopyFile(name.Value.NoSortFileName, output);
                        //File.Delete(name.Value.NoSortFileName);
                    }
                    if (name.Value.FileName != null)
                    {
                        CopyFile(name.Value.FileName, output);
                        //File.Delete(name.Value.FileName);
                    }
                }
            }
        }

        /// <summary>
        /// Copy content of the file into stream
        /// </summary>
        /// <param name="fileName">The name of the file to copy from</param>
        /// <param name="output">Output file stream</param>
        private void CopyFile(string fileName, FileStream output)
        {
            using (var file = File.OpenRead(fileName))
            {
                file.CopyTo(output);
            }
        }

        /// <summary>
        /// Splits big file into some chunks by matching starting characters in each line
        /// </summary>
        /// <param name="inputFileName">Big file name</param>
        /// <param name="chars">Number of starting characters to split by</param>
        private void SplitFile(string inputFileName, int chars)
        {
            var files = new Dictionary<string, FileChunk>(Comparer);
            using (var sr = new StreamReader(inputFileName, Encoding))
            {
                while (sr.Peek() >= 0)
                {
                    string entry = sr.ReadLine();
                    //The length of the line is less than the current number of characters we split by
                    //In this cases we add the line to the non-sorted file
                    if (entry.Length < chars)
                    {
                        ChunkInfo nameInfo;
                        if (!chunks.TryGetValue(entry, out nameInfo))
                            chunks.Add(entry, nameInfo = new ChunkInfo());
                        nameInfo.AddSmallString(entry, Encoding);
                    }
                    //Otherwise we add the line to the file corresponding to the first char characters of the line
                    else
                    {
                        string start = entry.Substring(0, chars);
                        FileChunk sfi;
                        if (!files.TryGetValue(start, out sfi))
                        {
                            sfi = new FileChunk(Encoding);
                            files.Add(start, sfi);
                        }
                        sfi.Append(entry, Encoding);
                    }
                }
            }
            //For each of the chunk we check if size of the chunk is still greater than the maxFileSize
            foreach (var file in files)
            {
                file.Value.Close();
                //If it is - split to smaller chunks
                if (file.Value.Size > maxFileSize)
                {
                    SplitFile(file.Value.FileName, chars + 1);
                    File.Delete(file.Value.FileName);
                }
                //Otherwise save it to the dictionary
                else
                {
                    SortFile(file.Value.FileName, file.Value.FileName);
                    ChunkInfo nameInfo;
                    if (!chunks.TryGetValue(file.Key, out nameInfo))
                        chunks.Add(file.Key, nameInfo = new ChunkInfo());
                    nameInfo.FileName = file.Value.FileName;
                }
            }
        }

        /// <summary>
        /// Sorts content of the specified file
        /// </summary>
        /// <param name="inputFileName">File to sort</param>
        /// <param name="outputFileName">File to write results to</param>
        private void SortFile(string inputFileName, string outputFileName)
        {
            var info = new FileInfo(inputFileName);

            var entries = new List<string>((int)(info.Length / 4L));
            using (var sr = new StreamReader(inputFileName, Encoding))
            {
                while (sr.Peek() >= 0)
                    entries.Add(sr.ReadLine());
            }
            entries.Sort(Comparer);
            using (var sw = new StreamWriter(outputFileName, false, Encoding))
            {
                foreach (string entry in entries)
                {
                    sw.WriteLine(entry);
                }
            }
        }
    }
}
