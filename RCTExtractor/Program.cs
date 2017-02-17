using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RCTExtractor
{
    class Program
    {
        private const string rctPath = @"D:\SteamLibrary\steamapps\common\RollerCoaster Tycoon Deluxe";
        private const string extracted = "extracted";

        //music
        private static readonly Dictionary<string, string> songNames = new Dictionary<string, string> {
            { "CSS2", "people" },
            { "Css3", "autodrom" },
            { "Css4", "carousel0" },
            { "Css5", "carousel1" },
            { "Css6", "carousel2" },
            { "Css7", "carousel3" },
            { "Css8", "carousel4" },
            { "Css9", "carousel5" },
            { "Css11", "carousel6" },
            { "Css13", "carousel7" },
            { "Css14", "carousel8" },
            { "Css15", "carousel9" },
            { "css17", "intro" },
            //TODO: many more
        };

        //sounds
        private static readonly byte[] riff = new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' };
        private static readonly byte[] wave = new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' };
        private static readonly byte[] fmt = new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' };
        private const uint formatSize = 2 + 2 + 4 + 4 + 2 + 2;
        private static readonly byte[] data = new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' };
        private static readonly string[] names = new string[] { "lift 1", "straight running", "lift 2",
            "scream 1", "click 1", "click 2", "place item", "scream 2", "scream 3", "scream 4",
            "scream 5", "scream 6", "lift 3", "cash sound", "crash", "laying out water", "water 1",
            "water 2", "train whistle", "train chugging", "water splash", "hammering", "ride launch 1",
            "ride launch 2", "cough 1", "cough 2", "cough 3", "cough 4", "rain 1", "thunder 1",
            "thunder 2", "rain 2", "rain 3", "tink 1", "tink 2", "scream 7", "toilet flush", "click 3",
            "quack", "message sound", "dialog box sound", "person 1", "person 2", "person 3", "clapping",
            "haunted house 1", "haunted house 2", "haunted house 3" };

        static void Main(string[] args)
        {
            if (Directory.Exists(extracted))
                Directory.Delete(extracted, true);

            Directory.CreateDirectory(extracted);

            ExtractMusic();
            ExtractSounds();
        }

        private static void ExtractMusic()
        {
            var musicDir = Path.Combine(extracted, "music");
            Directory.CreateDirectory(musicDir);

            foreach (var file in Directory.GetFiles(Path.Combine(rctPath, "Data"), "*.dat"))
            {
                bool validMusic;
                using (var fs = File.OpenRead(file))
                {
                    if (fs.Length <= 4)
                        continue;

                    byte[] buf = new byte[riff.Length];
                    var count = fs.Read(buf, 0, riff.Length);
                    if (count != riff.Length)
                        throw new Exception("did not read enough!");

                    validMusic = riff.SequenceEqual(buf);
                }

                if (validMusic)
                {
                    var baseName = Path.GetFileNameWithoutExtension(file);
                        File.Copy(file, Path.Combine(musicDir, (songNames.ContainsKey(baseName) ? songNames[baseName] : baseName) + ".wav"));
                }
                    
            }
        }

        private static void ExtractSounds()
        {
            var soundsPath = Path.Combine(extracted, "sounds");
            Directory.CreateDirectory(soundsPath);

            using (var fs = File.OpenRead(Path.Combine(rctPath, "Data", "Css1.dat")))
            using (var br = new BinaryReader(fs))
            {
                var positions = new uint[br.ReadUInt32()];

                for (int i = 0; i < positions.Length; i++)
                    positions[i] = br.ReadUInt32();

                for (int i = 0; i < positions.Length; i++)
                {
                    fs.Position = positions[i];

                    var size = br.ReadUInt32() - formatSize;

                    using (var sfs = File.OpenWrite(Path.Combine(soundsPath, names[i] + ".wav")))
                    using (var bw = new BinaryWriter(sfs))
                    {
                        bw.Write(riff);
                        bw.Write((uint)(wave.Length + fmt.Length + 4 + formatSize + data.Length + 4 + size));
                        bw.Write(wave);
                        bw.Write(fmt);
                        bw.Write(formatSize);
                        CopyStream(fs, sfs, (int)formatSize);
                        bw.Write(data);
                        bw.Write(size);
                        CopyStream(fs, sfs, (int)size);
                    }
                }
            }
        }

        private static void CopyStream(Stream input, Stream output, int bytes)
        {
            byte[] buffer = new byte[32768];
            int read;
            while (bytes > 0 &&
                   (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
            {
                output.Write(buffer, 0, read);
                bytes -= read;
            }
        }
    }
}