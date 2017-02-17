using System;
using System.Collections.Generic;
using System.IO;

namespace RCTExtractor
{
    class Program
    {
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
            if (Directory.Exists("sounds"))
                Directory.Delete("sounds", true);

            Directory.CreateDirectory("sounds");

            using (var fs = File.OpenRead(@"D:\SteamLibrary\steamapps\common\RollerCoaster Tycoon Deluxe\Data\Css1.dat"))
            using (var br = new BinaryReader(fs))
            {
                var positions = new uint[br.ReadUInt32()];

                for (int i = 0; i < positions.Length; i++)
                    positions[i] = br.ReadUInt32();

                for (int i = 0; i < positions.Length; i++)
                {
                    fs.Position = positions[i];

                    var size = br.ReadUInt32() - formatSize;

                    using (var sfs = File.OpenWrite(Path.Combine("sounds", names[i] + ".wav")))
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