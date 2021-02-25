//  ******************************************************
//
//  Dsize       Calculate directory size 
//  2021 02 24  Ronny Solheim
//
//  ******************************************************
using System;
using System.Threading.Tasks;

namespace DSize
{ 
    class Program
    {
        static string[] Folders;
        static string[] ShortSizeFolder;
        static string[] LongSizeFolder;

        static string Original = "";

        /// <summary>
        /// Startup. 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static int Main(string[] args)
        {
            // Test if input arguments were supplied.
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter directory to examine.");
                Console.WriteLine("Usage: Ex.  Dsize . For present directory.");
                Console.WriteLine("Usage: Ex.  Dsize C:\\temp");
                return -1;
            }

            string path = args[0];

            Original = GetFolders(path);
            Folders = Original.Split('\n');
            LongSizeFolder = Original.Split('\n');
            ShortSizeFolder = Original.Split('\n');

            GetFolderSize();
            return 0;
        }

        /// <summary>
        /// Goes thrue all folders.
        /// </summary>
        static void GetFolderSize()
        {
            long Size;
            long Total = 0;
            int Pos = 0;

            Console.Clear();
            foreach (var line in Folders)
            {
                if (line.Length > 2)
                {
                    System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(line);
                    Size = CalculateFolderSize(di);
                    if (Size != -1)         // -1 = Directory locked or faulty.
                    {
                        Total += Size;
                    }
                    Console.WriteLine(PositionSize(Size, false) + "  " + line );
                    ShortSizeFolder[Pos] = PositionSize(Size, true) + "  " + ShortSizeFolder[Pos];
                    LongSizeFolder[Pos] = PositionSize(Size, false) + "  " + LongSizeFolder[Pos];
                    Pos += 1;
                }
            }

            string Tmp;
            Console.WriteLine('\n' + PositionSize(Total, true) + "  Total.");  
            Tmp = PositionSize(Total, false) + "  Total.";  //"Total " + PositionSize(Total, false);
            LongSizeFolder[Pos] = Tmp;
            System.Array.Sort(LongSizeFolder);
            CreateSortedShortSizeFolder();
            ShowFolder(true);  // Show sorted short number.
        }

        /// <summary>
        /// Display after all folders calculated.
        /// </summary>
        static void CreateSortedShortSizeFolder()
        {
            long Size;
            int Pos = 0;

            foreach (var line in LongSizeFolder)
            {
                try
                {
                    Size = Convert.ToInt64(Left(line, 20));
                    ShortSizeFolder[Pos] = PositionSize(Size, true) + Right(line, line.Length - 23);
                }
                catch (Exception)
                {
                    Size = 0;
                    ShortSizeFolder[Pos] = PositionSize(Size, true) + " *" + Right(line, line.Length - 21); 
                }
                Pos++;
            }
        }

        static void ShowFolder(bool ShortOrLong)
        {
            Console.Clear();

            // ShortSizeFolder
            if (ShortOrLong)
            {
                foreach (var line in ShortSizeFolder)
                {
                    if (line.Length > 2)
                        Console.WriteLine(line);
                }
            }
            else
                foreach (var line in LongSizeFolder)
                {
                    if (line.Length > 2)
                        Console.WriteLine(line);
                }
        }

        static string LongFill  = "                       ";
        static string ShortFill = "             ";
        static string PositionSize(long Size, bool LS)
        {
            string s = GbKbBt(Size, LS);

            if (LS)
            {
                s = Left(ShortFill, ShortFill.Length - s.Length) + s;
                return s;
            }
            s = Left(LongFill, LongFill.Length - s.Length) + s;
            return s;
        }

        static string GbKbBt(long Size, bool LS)
        {
            if (!LS)
            {
                return Size.ToString() + " --";
            }

            return SizeSuffix(Size, 2);
        }


        static string Left(string s, int pos)
        {
            return s.Substring(0, pos);
        }

        static string Right(string s, int pos)
        {
            return s.Substring(s.Length - pos, pos);
        }

        private static readonly string[] SizeSuffixes =
           { "b.", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        /// <summary>
        ///             Smart way to find size suffix.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        static string SizeSuffix(long value, int decimalPlaces = 2)
        {
            if (value == -1) { return string.Format("{0:n" + decimalPlaces + "} **", 0); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} b.", 0); }
            int mag = (int)Math.Log(value, 1024);  
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }
            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        /// <summary>
        ///             Recursive function for calculating childe folders.
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        static long CalculateFolderSize(System.IO.DirectoryInfo directoryInfo, bool recursive = true)
        {

            var startDirectorySize = default(long);

            if (directoryInfo == null || !directoryInfo.Exists)
            {
                return startDirectorySize; //Return 0 while Directory does not exist.
            }

            try
            {
                foreach (var fileInfo in directoryInfo.GetFiles())
                    System.Threading.Interlocked.Add(ref startDirectorySize, fileInfo.Length);
            }
            catch (Exception)
            {
                return -1; // startDirectorySize; //Return 0 Directory has security lock. 
            }

            if (recursive) //Loop on Sub Direcotries in the Current Directory and Calculate it's files size.
                Parallel.ForEach(directoryInfo.GetDirectories(), (subDirectory) =>
                System.Threading.Interlocked.Add(ref startDirectorySize, CalculateFolderSize(subDirectory, recursive)));

            return startDirectorySize;  //Return full Size of this Directory.
        }


        static string GetFolders(string Path)
        {
            string reply = "";

            try
            {
                foreach (string dir in System.IO.Directory.GetDirectories(Path))
                    reply = reply + dir.ToString() + '\n';
                return reply;
            }
            catch (Exception)
            {
                return "";
            }

        }
    }
}
