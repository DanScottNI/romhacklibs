using System;
using System.Drawing;
using System.Drawing.Imaging;
using ROMClass.Graphics;
using System.Resources;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ROMClass.Graphics.NES
{
    /// <summary>
    /// A class that draw NES 2BPP format tiles.
    /// </summary>
    public unsafe class NES2BPP
    {
        System.Drawing.Bitmap bitmap;

        int width;
        BitmapData bitmapData = null;
        byte[] DestData;

        /// <summary>
        /// The colours that make up the NES's palette.
        /// </summary>
        public Color[] ColourPalette;

        /// <summary>
        /// Constructor that takes a bitmap, and loads the default palette.
        /// </summary>
        /// <param name="Bitmap">The bitmap which the tiles will be drawn onto.</param>
        public NES2BPP(Bitmap Bitmap)
        {
            this.bitmap = Bitmap;
            this.LoadDefaultPalette();
        }

        /// <summary>
        /// Constructor that takes a bitmap, and a filename for the palette file.
        /// </summary>
        /// <param name="Bitmap">The bitmap which the tiles will be drawn onto.</param>
        /// <param name="PalFilename">The filename for the external palette file.</param>
        public NES2BPP(Bitmap Bitmap, string PalFilename)
        {
            this.bitmap = Bitmap;
            this.LoadPaletteFile(PalFilename);
        }

        /// <summary>
        /// Saves the internal bitmap to a file.
        /// </summary>
        /// <param name="filename">The filename to save the bitmap to.</param>
        public void Save(string filename)
        {
            bitmap.Save(filename, ImageFormat.Bmp);
        }

        /// <summary>
        /// Disposes of the bitmap.
        /// </summary>
        public void Dispose()
        {
            bitmap.Dispose();
        }

        /// <summary>
        /// Returns the internal bitmap.
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                return (bitmap);
            }
        }

        /// <summary>
        /// Returns the size of a pixel.
        /// </summary>
        public Point PixelSize
        {
            get
            {
                GraphicsUnit unit = GraphicsUnit.Pixel;
                RectangleF bounds = bitmap.GetBounds(ref unit);

                return new Point((int)bounds.Width, (int)bounds.Height);
            }
        }

        /// <summary>
        /// Draws a NES 2BPP tile to specified co-ordinates.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="TileData"></param>
        /// <param name="Palette"></param>
        public void DrawTile(int X, int Y, byte* TileData, byte* Palette)
        {
            Point pixsize = PixelSize;

            //LockBitmap();

            byte Temp2;
            byte Temp3;
            byte Temp4;
            int psize = 7;

            for (int y1 = 0; y1 <= psize; y1++)
            {
                for (int x1 = 0; x1 <= psize; x1++)
                {
                    Temp2 = 0;
                    Temp3 = 0;

                    Temp2 = (byte)((((*(TileData + y1)) & (0x80 >> x1)) >> (psize - x1)));
                    Temp3 = (byte)(((*(TileData + y1 + 8)) & (0x80 >> x1)) >> (psize - x1));
                    Temp4 = (byte)((Temp3 << 1) + Temp2);

                    /*PixelData* pPixel = PixelAt((X + x1),(Y + y1));
                    pPixel->red = ColourPalette[*(Palette + Temp4)].R;
                    pPixel->green = ColourPalette[*(Palette + Temp4)].G;
                    pPixel->blue = ColourPalette[*(Palette + Temp4)].B;*/
                    PixelSet((X + x1), (Y + y1), ColourPalette[*(Palette + Temp4)]);
                }

            }

            //UnlockBitmap();
        }

        /// <summary>
        /// Locks the bitmap in memory to allow low-level access.
        /// </summary>
        public void LockBitmap()
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = bitmap.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.X,
                (int)boundsF.Y,
                (int)boundsF.Width,
                (int)boundsF.Height);

            // Figure out the number of bytes in a row
            // This is rounded up to be a multiple of 4
            // bytes, since a scan line in an image must always be a multiple of 4 bytes
            // in length. 
            width = (int)boundsF.Width * sizeof(PixelData);
            if (width % 4 != 0)
            {
                width = 4 * (width / 4 + 1);
            }

            bitmapData =
                bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            this.DestData = new Byte[bounds.Width * bounds.Height * sizeof(PixelData)];

            Marshal.Copy(bitmapData.Scan0, this.DestData, 0, DestData.Length);

            //pBase = (Byte*) bitmapData.Scan0.ToPointer();
        }

        /// <summary>
        /// Returns the pixel data for a specified co-ordinate in the bitmap.
        /// </summary>
        /// <param name="x">The x co-ordinate of the pixel.</param>
        /// <param name="y">The y co-ordinate of the pixel.</param>
        /// <returns>A PixelData struct for the specified pixel.</returns>
        public PixelData* PixelAt(int x, int y)
        {

            return (PixelData*)DestData[y * width + x * sizeof(PixelData)];
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="Pal"></param>
        public void PixelSet(int x, int y, Color Pal)
        {
            int loc;

            loc = y * width + x * sizeof(PixelData);

            DestData[loc] = Pal.B;
            DestData[loc + 1] = Pal.G;
            DestData[loc + 2] = Pal.R;
        }

        /// <summary>
        /// Unlocks the bitmap's pixels in memory.
        /// </summary>
        public void UnlockBitmap()
        {
            Marshal.Copy(this.DestData, 0, bitmapData.Scan0, DestData.Length);
            bitmap.UnlockBits(bitmapData);

            bitmapData = null;
        }

        /// <summary>
        /// Loads in a NES palette file.
        /// </summary>
        /// <param name="Filename">The filename of the NES palette file.</param>
        private void LoadPaletteFile(string Filename)
        {
            ColourPalette = new Color[64];
            FileStream _imageStream;

            byte[] rgb = new byte[3];

            _imageStream = new FileStream(Filename, System.IO.FileMode.Open);

            for (int i = 0; i < 64; i++)
            {
                _imageStream.Read(rgb, 0, 3);
                ColourPalette[i] = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }
            _imageStream.Close();
        }

        /// <summary>
        /// Loads in a palette from the resources section in the current assembly.
        /// </summary>
        private void LoadDefaultPalette()
        {

            ColourPalette = new Color[64];
            Assembly _assembly;
            Stream _imageStream;

            byte[] rgb = new byte[3];

            _assembly = Assembly.GetExecutingAssembly();
            _imageStream = _assembly.GetManifestResourceStream("MegamanData.matrx21f.pal");
            for (int i = 0; i < 64; i++)
            {
                _imageStream.Read(rgb, 0, 3);
                ColourPalette[i] = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }
            _imageStream.Close();

        }

        public void Begin()
        {
            LockBitmap();
        }

        public void End()
        {
            UnlockBitmap();
        }
    }
}