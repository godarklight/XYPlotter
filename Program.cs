using System;
using System.IO;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;


namespace BadApple;
class Program
{
    static void Main(string[] args)
    {
        Directory.CreateDirectory("frame");
        Random random = new Random();
        byte[] data = new byte[3 * 1920 * 1080];
        Stopwatch sw = Stopwatch.StartNew();
        using (FileStream fs = new FileStream("in.wav", FileMode.Open))
        {
            int frameNo = 0;
            fs.Seek(0x4E, SeekOrigin.Begin);
            while (true)
            {
                for (int i = 0; i < 1600; i++)
                {
                    //Read S16LE
                    short left = (short)(fs.ReadByte() | fs.ReadByte() << 8);
                    short right = (short)(fs.ReadByte() | fs.ReadByte() << 8);
                    //Convert into 0-1 frame
                    double leftd = (left + short.MaxValue) / (2.0 * short.MaxValue);
                    double rightd = (right + short.MaxValue) / (2.0 * short.MaxValue);
                    //Conver to screen space
                    int xPos = (int)(1920 * leftd);
                    int yPos = (int)(1080 * rightd);
                    int dataPos = 3 * (yPos * 1920 + xPos) + 1;
                    data[dataPos] = 255;

                }
                //Save the image
                using (Image<Rgb24> img = Image.LoadPixelData<Rgb24>(data, 1920, 1080))
                {
                    string filePath = $"frame/{frameNo}.bmp";
                    File.Delete(filePath);
                    using (FileStream saveFile = new FileStream(filePath, FileMode.CreateNew))
                    {
                        img.SaveAsBmp(saveFile);
                    }
                }
                //Fade
                for (int i = 0; i < data.Length / 3; i++)
                {
                    int fadePos = i * 3 + 1;
                    data[fadePos] = (byte)(data[fadePos] / 2);
                }
                if (fs.Length - fs.Position < (1600 * 2))
                {
                    break;
                }
                frameNo++;
            }
        }
        sw.Stop();
        Console.WriteLine(sw.ElapsedMilliseconds);
    }
}
