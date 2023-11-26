using MediaInfo.DotNetWrapper.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using MediaInfo;
using MediaInfo.DotNetWrapper;
using System.Runtime.CompilerServices;

namespace WhatsappMassStickers
{
    internal class Program
    {

        public static string RootDir = "C:/Users/Florian Wahl/Desktop/WhatsappStickers";
        public static string ImportDir = "C:/Users/Florian Wahl/Desktop/WhatsappStickers/WhatsApp Stickers";
        public static string Title = "Bubu & Dudu";
        public static string Author = "Vani & Flo";

        static void Main(string[] args)
        {
            Run();
            Console.WriteLine("Complete - press any key to exit");
            Console.ReadLine();
        }

        public static void Run()
        {
            Console.WriteLine("Starting");
            List<string> files = Directory.GetFiles(ImportDir).ToList();
            var fileSegments = SplitListIntoSegments(files, 30);

            Console.WriteLine("Splitting into segments");
            foreach (var segment in fileSegments)
            {
                Console.WriteLine($"Creating pack for segment {fileSegments.IndexOf(segment) + 1}:");
                var folderName = ProcessSegment(segment, fileSegments.IndexOf(segment));

                var folderPath = Path.Combine(RootDir, folderName);
                var zipPath = Path.Combine(RootDir, folderName + ".wastickers");

                Console.WriteLine("Zipping...");
                ZipFolder(folderPath, zipPath);
                Console.WriteLine("Done");
            }
        }

        public static List<List<string>> SplitListIntoSegments(List<string> list, int segmentSize)
        {
            var segments = new List<List<string>>();
            for (int i = 0; i < list.Count; i += segmentSize)
            {
                segments.Add(list.GetRange(i, Math.Min(segmentSize, list.Count - i)));
            }
            return segments;
        }

        public static string ProcessSegment(List<string> items, int no)
        {

            Console.WriteLine("Creating folder...");
            //variables
            string folderName = Title + " " + no;
            string folderPath = Path.Combine(RootDir, folderName);

            Directory.CreateDirectory(folderPath);
            Console.WriteLine("Done");


            Console.WriteLine("Copying files...");
            //copy files
            foreach (var item in items)
            {
                string target = Path.Combine(folderPath, Path.GetFileName(item));
                File.Copy(item, target, true );
            }
            Console.WriteLine("Done");

            //meta data
            Console.WriteLine("Writing metadata...");
            string authorPath = Path.Combine(folderPath, "author.txt");
            string titlePath = Path.Combine(folderPath, "title.txt");
            File.WriteAllText(authorPath, Author);
            File.WriteAllText(titlePath, Title + " " + no + 1);
            Console.WriteLine("Done");

            //tray.png
            Console.WriteLine("Creating tray icon...");
            string trayPath = Path.Combine(folderPath, "tray.png");

            var tray = items.Where(x => !IsWebMAnimated(x) && IsFileBelow50KB(x)).FirstOrDefault();
            if (tray == null)
            {
                Console.WriteLine("Using fallback icon");
                tray = "C:/Users/Florian Wahl/Desktop/WhatsappStickers/tray.png";
            }

            File.Copy(tray, trayPath, true );
            Console.WriteLine("Done");

            return folderName;
        }

        public static void ZipFolder(string sourceFolder, string destinationZipFilePath)
        {
            if (Directory.Exists(sourceFolder))
            {
                ZipFile.CreateFromDirectory(sourceFolder, destinationZipFilePath);
            }
            else
            {
                throw new DirectoryNotFoundException("Source folder not found.");
            }
        }

        public static bool IsWebMAnimated(string filePath)
        {
            using (var mediaInfo = new MediaInfo.MediaInfo())
            {
                mediaInfo.Open(filePath);

                // Get the number of video tracks in the file
                int videoTracks = mediaInfo.CountGet(MediaInfo.StreamKind.Video);

                if (videoTracks > 0)
                {
                    // Retrieve the frame count of the first video track
                    string frameCountStr = mediaInfo.Get(MediaInfo.StreamKind.Video, 0, "FrameCount");

                    if (int.TryParse(frameCountStr, out int frameCount))
                    {
                        // Check if more than one frame is present
                        return frameCount > 1;
                    }
                }

                return false;
            }
        }

        public static bool IsFileBelow50KB(string filePath)
        {
            const int maxFileSizeInBytes = 50 * 1024; // 50KB in bytes
            FileInfo fileInfo = new FileInfo(filePath);

            return fileInfo.Length < maxFileSizeInBytes;
        }
    }
}
