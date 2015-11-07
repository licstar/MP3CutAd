using System;
using System.Diagnostics;
using System.IO;

namespace MP3CutAd.Core {
    class FFMpeg {
        private static string path = Path.Combine(Environment.CurrentDirectory, "../ffmpeg/ffmpeg.exe");

        private static void run(string argument) {
            Process p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = argument;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();

            p.StandardError.ReadToEnd();
            p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }

        public static void Mp3toWav(string mp3, string wav) {
            run(string.Format("-i \"{0}\" -ac 1 -ar 22050 -f wav \"{1}\"", mp3, wav));
        }

        public static void Split(string mp3, string output, double from, double to) {
            run(string.Format("-i \"{0}\" -c copy -ss {2} -to {3} \"{1}\"",
                mp3, output, from, to));
        }

        public static void Concat(string list, string output) {
            run(string.Format("-f concat -i \"{0}\" -c copy \"{1}\"", list, output));
        }
    }
}
