using RuriLib.ViewModels;
using System;
using System.Linq;

namespace OpenBullet.ViewModels
{
    class ListGeneratorViewModel : ViewModelBase
    {
        private bool onlyLuhn = false;
        public bool OnlyLuhn { get { return onlyLuhn; } set { onlyLuhn = value; OnPropertyChanged("OnlyLuhn"); OnPropertyChanged("OutputLines"); OnPropertyChanged("OutputSize"); } }

        private bool autoImport = false;
        public bool AutoImport { get { return autoImport; } set { autoImport = value; OnPropertyChanged("AutoImport"); } }

        private string mask = "657438923467423847****:**";
        public string Mask { get { return mask; } set { mask = value; OnPropertyChanged("Mask"); OnPropertyChanged("OutputLines"); OnPropertyChanged("OutputSize"); } }

        private string allowedCharacters = "0123456789";
        public string AllowedCharacters { get { return allowedCharacters; } set { allowedCharacters = value; OnPropertyChanged("AllowedCharacters"); OnPropertyChanged("OutputLines"); OnPropertyChanged("OutputSize"); } }

        
        public int OutputLines { get
            {
                var splitMask = Mask.Split(':')[0].Replace("*","");
                var varCount = Mask.ToCharArray().Where(c => c == '*').Count();
                var lines = (int)Math.Pow(AllowedCharacters.Length, varCount);
                var allNum = !(splitMask.ToCharArray().Any(c => !Char.IsDigit(c)) || AllowedCharacters.ToCharArray().Any(c => !Char.IsDigit(c)));
                return allNum && OnlyLuhn ? lines / 10 : lines;
            }
        }

        
        public string OutputSize { get
            {
                return SizeSuffix(sizeof(char) * Mask.Length * OutputLines, 0);
            }
        }

        
        static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        
        static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }
}
