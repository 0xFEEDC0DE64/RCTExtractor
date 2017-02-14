using System;
using System.Collections.Generic;
using System.IO;

namespace RCTExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Directory.Exists("sounds"))
                Directory.Delete("sounds", true);

            Directory.CreateDirectory("sounds");

            using (var fs = File.OpenRead(@"D:\SteamLibrary\steamapps\common\RollerCoaster Tycoon Deluxe\Data\Css1.dat"))
            using (var br = new BinaryReader(fs))
            {
                var positions = new uint[br.ReadUInt32()];

                Console.WriteLine("{0} sounds", positions.Length);

                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = br.ReadUInt32();
                    Console.WriteLine("sound {0} at offset {1} {2}", i, positions[i], i > 0 ? "+" + (positions[i] - positions[i - 1]) : "");
                }

                var names = new string[] { "lift 1", "straight running", "lift 2", "scream 1",
                    "click 1", "click 2", "place item", "scream 2", "scream 3", "scream 4",
                    "scream 5", "scream 6", "lift 3", "cash sound", "crash", "laying out water",
                    "water 1", "water 2", "train whistle", "train chugging", "water splash",
                    "hammering", "ride launch 1", "ride launch 2", "cough 1", "cough 2",
                    "cough 3", "cough 4", "rain 1", "thunder 1", "thunder 2", "rain 2", "rain 3",
                    "tink 1", "tink 2", "scream 7", "toilet flush", "click 3", "quack",
                    "message sound", "dialog box sound", "person 1", "person 2", "person 3",
                    "clapping", "haunted house 1", "haunted house 2", "haunted house 3" };

                for (int i = 0; i < positions.Length; i++)
                {
                    fs.Position = positions[i];

                    var size = br.ReadUInt32();

                    var wFormatTag = br.ReadUInt16();
                    var wChannels = br.ReadUInt16();
                    var dwSamplesPerSec = br.ReadUInt32();
                    var dwAvgBytesPerSec = br.ReadUInt32();
                    var wBlockAlign = br.ReadUInt16();
                    var wBitsPerSample = br.ReadUInt16();

                    Console.WriteLine("Exporting sound {0} (size: {1})", i, size);
                    Console.WriteLine("  wFormatTag {0}", wFormatTag);
                    Console.WriteLine("  wChannels {0}", wChannels);
                    Console.WriteLine("  dwSamplesPerSec {0}", dwSamplesPerSec);
                    Console.WriteLine("  dwAvgBytesPerSec {0}", dwAvgBytesPerSec);
                    Console.WriteLine("  wBlockAlign {0}", wBlockAlign);
                    Console.WriteLine("  wBitsPerSample {0}", wBitsPerSample);

                    using (var sfs = File.OpenWrite(Path.Combine("sounds", names[i] + ".wav")))
                    using (var bw = new BinaryWriter(sfs))
                    {
                        bw.Write(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' });
                        bw.Write(4 + 4 + 4 + 4 + 4 + size);
                        bw.Write(new byte[] { (byte)'W', (byte)'A', (byte)'V', (byte)'E' });
                        bw.Write(new byte[] { (byte)'f', (byte)'m', (byte)'t', (byte)' ' });
                        bw.Write((uint)(2 + 2 + 4 + 4 + 2 + 2));
                        bw.Write(wFormatTag);
                        bw.Write(wChannels);
                        bw.Write(dwSamplesPerSec);
                        bw.Write(dwAvgBytesPerSec);
                        bw.Write(wBlockAlign);
                        bw.Write(wBitsPerSample);
                        bw.Write(new byte[] { (byte)'d', (byte)'a', (byte)'t', (byte)'a' });
                        bw.Write(size - 2 - 2 - 4 - 4 - 2 - 2);
                        bw.Write(br.ReadBytes((int)(size - 2 - 2 - 4 - 4 - 2 - 2)));
                    }
                }
            }
        }
    }
}
