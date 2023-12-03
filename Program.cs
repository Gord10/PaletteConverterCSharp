/*
 * 
Copyright 2023 Ahmet Kamil Keles

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 * */

using System.Diagnostics;
using System.Drawing;
using System.IO;

class Sample
{
    //Filename of the palette file that will be generated
    static readonly string PALETTE_FILENAME = "palette.png";

    //Examples: player.png will be converted as player_converted.png, if user is converting a single image. "/Sprites/" folder will be "/Sprites_converted/", if user is converting whole png images in a folder.
    static readonly string CONVERTED_NAME_POSTFIX = "_converted";

    static System.Drawing.Imaging.PixelFormat PIXEL_FORMAT = System.Drawing.Imaging.PixelFormat.Format32bppPArgb;

    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            string argument = args[0]; //This is supposed to be a file or folder name. Program will halt if neither, because the purpose of this program is to generate output based on files/folders.
            //string argument = "example_c64.png";

            bool isFile = File.Exists(argument);
            bool isFolder = Directory.Exists(argument);

            if (!isFile && !isFolder)
            {
                Console.WriteLine("Invalid file or folder name: " + argument);
                return;
            }
            bool willQuit = false;

            if (isFolder) //We can safely assume that user wants to convert the png images within a folder. We don't need to show them a main screen that prompts what they want to do.
                          //Because they can't generate palette from multiple images in this program, anyway (yet).
            {
                ConvertImagesInDirectories(args); //Using the array of arguments, so the user can convert multiple directories at one time.
                                                  //Assumption is that the arguments are all directories, but the validity of each directory path will still be checked in that function
                willQuit = true;
            }

            while (!willQuit)
            {
                willQuit = ShowMainScreen(argument);
            } 

            Console.WriteLine("Press any key to quit...");
            Console.Read();
        }
        else
        {
            Console.WriteLine("Please drag an image or folder on the exe file, so the tool can process it.\n(Press any key to exit)");
            //ShowHelp();
            
            Console.ReadKey();
        }
    }

    //Returns whether the application will quit
    static bool ShowMainScreen(string argument)
    {
        bool willQuit = false;
        Console.WriteLine(argument);

        Console.WriteLine("1: Create a palette from this reference image");
        Console.WriteLine("2: Convert this image's colors according to the palette");
        
        Console.WriteLine("3: Help");
        Console.WriteLine("(Press 1, 2 or 3...)");
        ConsoleKeyInfo key = Console.ReadKey();

        switch(key.Key)
        {
            case ConsoleKey.D1:
                CreatePalette(argument);
                willQuit = true;
                break;

            case ConsoleKey.D2:
                string outputFileName = Path.GetFileNameWithoutExtension(argument);
                outputFileName += CONVERTED_NAME_POSTFIX + ".png";
                List<Color> palette = GetPaletteFromFile(PALETTE_FILENAME);

                ConvertImage(imageToConvertFilePath: argument, outputImageFilePath: outputFileName, palette, willOpenConvertedFile: true);
                willQuit = true;
                break;

            case ConsoleKey.D3:
                Console.WriteLine("\n");
                ShowHelp();
                willQuit = false;
                break;

            default:
                Console.WriteLine("Invalid key...");
                Console.Read();
                willQuit = false;
                break;
        }

        return willQuit;
    }

    static void ConvertImagesInDirectories(string[] directories)
    {
        List<Color> palette = GetPaletteFromFile(PALETTE_FILENAME);

        if(palette.Count == 0) //Invalid palette
        {
            return;
        }

        foreach (string directory in directories)
        {
            if (Directory.Exists(directory))
            {
                ConvertImagesInDirectory(directory, palette);
            }
        }
    }

    //Converts the colors of all png images within this directory
    static void ConvertImagesInDirectory(string orgDirectoryName, List<Color> palette)
    {
        Console.WriteLine("Folder: " + orgDirectoryName);
        string outputFolderName = orgDirectoryName + CONVERTED_NAME_POSTFIX;
        Directory.CreateDirectory(outputFolderName);

        string[] filePathsInDirectory = Directory.GetFileSystemEntries(orgDirectoryName, "*.png", SearchOption.AllDirectories);

        foreach (string filePath in filePathsInDirectory)
        {
            string relativePath = Path.GetRelativePath(orgDirectoryName, filePath);
            string outputFilePath = outputFolderName + Path.DirectorySeparatorChar + relativePath;

            string outputFileDirectory = Path.GetDirectoryName(outputFilePath);
            Directory.CreateDirectory(outputFileDirectory);

            //There will probably be too many output files. So let's not automatically open them in order not to spam user.
            ConvertImage(imageToConvertFilePath: filePath, outputImageFilePath: outputFilePath, palette, willOpenConvertedFile: false);
        }
    }

    static void ShowHelp()
    {
        Console.WriteLine("You can get the color palette of an image with this program, and then apply this palette to another image easily.\n\n");
        Console.WriteLine("1. Drag the image whose palette you want to extract on PaletteConverter.exe. (example_NES.png, for example)");
        Console.WriteLine("2. Choose \"Create a palette from this reference image\". This will create palette.png, which contains the colors of the reference image.");
        Console.WriteLine("3. Drag the image whose colors need to be changed on PaletteConverter.exe. (example_c64.png, for example)");
        Console.WriteLine("4. Choose \"Convert this image's colors according to the palette\". This will open the new file.");
        Console.WriteLine("\n\nAlternatively, you can drag folder(s) to the exe to convert multiple images according to the palette you created in prior.");
    }

    //We need to make sure the bitmap we read from file is in a format we consistently use in this program
    static Bitmap GetBitmapFromFile(string filePath)
    {
        Bitmap bmpTmp = null;
        try
        {
            bmpTmp = new Bitmap(filePath);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return null;
        }

        Bitmap bitmap = bmpTmp.Clone(new Rectangle(0, 0, bmpTmp.Width, bmpTmp.Height), PIXEL_FORMAT);
        bmpTmp.Dispose();
        return bitmap;
    }

    //Finds the unique colors in the reference image, then creates a png file (palette.png, as default) that consists of those colors.
    //This palette file will be a row of pixels that each represent a color.
    static void CreatePalette(string referenceImageFileName)
    {
        //Find unique colors
        List<Color> colorsInPalette = new List<Color>();
        Console.WriteLine("Create palette");

        Bitmap referenceImage = GetBitmapFromFile(referenceImageFileName);
        for (int i = 0; i < referenceImage.Width; i++)
        {
            for (int j = 0; j < referenceImage.Height; j++)
            {
                Color pixel = referenceImage.GetPixel(i, j);
                if (!colorsInPalette.Contains(pixel)) //Ignore the transparent pixels
                {
                    colorsInPalette.Add(pixel);
                }
            }
        }

        referenceImage.Dispose();

        if(colorsInPalette.Count == 0)
        {
            Console.WriteLine("No colors could be found in the image. Is it completely transparent?");
            return;
        }

        //Creates the png that consists of the unique colors
        Bitmap paletteImage = new Bitmap(colorsInPalette.Count, 1, PIXEL_FORMAT);

        for (int i = 0; i < colorsInPalette.Count - 1; i++)
        {
            paletteImage.SetPixel(i, 0, colorsInPalette[i]);
        }

        try
        {
            paletteImage.Save(PALETTE_FILENAME, System.Drawing.Imaging.ImageFormat.Png);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return;
        }
        
        paletteImage.Dispose();
        Console.WriteLine("\n" + PALETTE_FILENAME + " is written");
        Console.WriteLine(colorsInPalette.Count + " colors");
    }

    //Converts the colors of the image to closest colors in the palette, then creates an image
    static void ConvertImage(string imageToConvertFilePath, string outputImageFilePath, List<Color> palette, bool willOpenConvertedFile)
    {
        Bitmap imgOriginal = GetBitmapFromFile(imageToConvertFilePath);
        if(imgOriginal == null)
        {
            Console.WriteLine("Halting...");
            return;
        }

        Bitmap imgConverted = new Bitmap(imgOriginal);

        if(palette.Count == 0)
        {
            Console.WriteLine("Halting");
            imgOriginal.Dispose();
            imgConverted.Dispose();
            return;
        }

        int i, j;
        for (i = 0; i < imgOriginal.Width;i++)
        {
            for(j=0; j < imgOriginal.Height; j++)
            {
                Color color = imgOriginal.GetPixel(i, j);
                color = ConvertColor(color, palette);
                imgConverted.SetPixel(i, j, color);
            }
        }

        imgOriginal.Dispose();

        try
        {
            imgConverted.Save(outputImageFilePath, System.Drawing.Imaging.ImageFormat.Png);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return;
        }
        
        imgConverted.Dispose();
        Console.WriteLine("\n" + outputImageFilePath + " is written");

        if(willOpenConvertedFile)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(@outputImageFilePath)
                {
                    UseShellExecute = true
                }
            }.Start();
        }
    }

    //Finds the closest color to orgColor in the palette
    static Color ConvertColor(Color orgColor, List<Color> palette)
    {
        if(orgColor.A == 0) //This pixel is transparent, we can simply return transparency
        {
            return Color.Transparent;
        }

        int i;
        int minDistance = int.MaxValue;
        Color closestColor = Color.White;

        for(i = 0; i < palette.Count; i++)
        {
            int distance = GetColorDistance(orgColor, palette[i]);
            if(distance < minDistance)
            {
                minDistance = distance;
                closestColor = palette[i];
            }
        }

        return closestColor;
    }

    //Returns the list of colors that the palette image contains. We assume the palette image height is 1, and each color is a pixel in this row. 
    static List<Color> GetPaletteFromFile(string paletteFileName)
    {
        Bitmap paletteImage;
        List<Color> palette = new();
        try
        {
            paletteImage = new(paletteFileName);
        }
        catch(Exception ex)
        {
            Console.WriteLine("Are you sure " + paletteFileName + " exists? You need a palette first.");
            return palette;
        }

        int i;
        for(i = 0; i < paletteImage.Width;i++)
        {
            Color color = paletteImage.GetPixel(i, 0);
            palette.Add(color);
        }

        return palette;
    }

    static int GetColorDistance(Color a, Color b)
    {
        int rDistance = a.R - b.R;
        int gDistance = a.G - b.G;
        int bDistance = a.B - b.B;

        rDistance = Math.Abs(rDistance);
        gDistance = Math.Abs(gDistance);
        bDistance = Math.Abs(bDistance);

        return rDistance + gDistance + bDistance;
    }
}
