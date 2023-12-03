# PaletteConverterCSharp
You can get the color palette of an image with this program, and then apply this palette to another image easily.
![example_c64](https://github.com/Gord10/PaletteConverterCSharp/assets/9501683/13839c71-6551-4c71-bdc7-29b4fa4f888a)

Original

![example_c64_converted](https://github.com/Gord10/PaletteConverterCSharp/assets/9501683/32dea220-5be6-426d-a74a-cd54e830c7ef)

After converting to [NES palette](https://en.wikipedia.org/wiki/File:NES_palette_sample_image.png)
Note that this program is able to create palette files simply by reading reference images

# How to use?
1. Drag the image whose palette you want to extract on PaletteConverter.exe. (example_NES.png, for example).
2. Choose "Create a palette from this reference image". This will create palette.png, which contains the colors of the reference image.
3. Drag the image whose colors need to be changed on PaletteConverter.exe. (example_c64.png, for example).
4. Choose "Convert this image's colors according to the palette". This will open the new file.

Alternatively, you can drag folder(s) to the exe to convert multiple images according to the palette you created in prior.

# Dependencies
.NET 6.0
