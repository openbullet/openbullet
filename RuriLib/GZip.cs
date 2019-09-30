using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib
{
    /*
     * Taken from https://stackoverflow.com/questions/7343465/compression-decompression-string-with-c-sharp
     * */

    /// <summary>
    /// GZip utilities class.
    /// </summary>
    public static class GZip
    {
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

        /// <summary>
        /// Zips a string.
        /// </summary>
        /// <param name="str">The string to zip</param>
        /// <returns>The zipped byte array.</returns>
        public static byte[] Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }

        /// <summary>
        /// Unzips a byte array.
        /// </summary>
        /// <param name="bytes">The byte array to unzip</param>
        /// <returns>The unzipped string.</returns>
        public static string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

    }
}
