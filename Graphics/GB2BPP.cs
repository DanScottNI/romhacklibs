using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ROMClass.Graphics.GB
{
    /// <summary>
    /// Summary description for GB2BPP.
    /// </summary>
    public unsafe class GB2BPP
    {

        System.Drawing.Bitmap bitmap;
        int width;
        BitmapData bitmapData = null;
        Byte* pBase = null;

        /// <summary>
        /// Constructor that takes a bitmap to draw the tiles onto.
        /// </summary>
        public GB2BPP(Bitmap Bitmap)
        {
            this.bitmap = Bitmap;
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
        /// Draws a GB 2BPP tile to specified co-ordinates.
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="TileData"></param>
        /// <param name="Palette"></param>
        public void DrawTile(int X, int Y, byte* TileData, byte* Palette)
        {
            Point pixsize = PixelSize;

            LockBitmap();

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

                    Temp2 = (byte)((((*(TileData + (y1 * 2))) & (0x80 >> x1)) >> (psize - x1)));
                    Temp3 = (byte)(((*(TileData + (y1 * 2) + 1)) & (0x80 >> x1)) >> (psize - x1));
                    Temp4 = (byte)((Temp3 << 1) + Temp2);

                    PixelData* pPixel = PixelAt((X + x1), (Y + y1));

                    switch (Temp4)
                    {
                        case 0:
                            pPixel->Red = Color.White.R;
                            pPixel->Green = Color.White.G;
                            pPixel->Blue = Color.White.B;
                            break;
                        case 1:
                            pPixel->Red = Color.LightGray.R;
                            pPixel->Green = Color.LightGray.G;
                            pPixel->Blue = Color.LightGray.B;

                            break;
                        case 2:
                            pPixel->Red = Color.DarkGray.R;
                            pPixel->Green = Color.DarkGray.G;
                            pPixel->Blue = Color.DarkGray.B;

                            break;
                        case 3:
                            pPixel->Red = Color.Black.R;
                            pPixel->Green = Color.Black.G;
                            pPixel->Blue = Color.Black.B;
                            break;
                    }

                }

            }

            UnlockBitmap();
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

            pBase = (Byte*)bitmapData.Scan0.ToPointer();
        }

        /// <summary>
        /// Returns the pixel data for a specified co-ordinate in the bitmap.
        /// </summary>
        /// <param name="x">The x co-ordinate of the pixel.</param>
        /// <param name="y">The y co-ordinate of the pixel.</param>
        /// <returns>A PixelData struct for the specified pixel.</returns>
        public PixelData* PixelAt(int x, int y)
        {
            return (PixelData*)(pBase + y * width + x * sizeof(PixelData));
        }

        /// <summary>
        /// Unlocks the bitmap's pixels in memory.
        /// </summary>
        public void UnlockBitmap()
        {
            bitmap.UnlockBits(bitmapData);
            bitmapData = null;
            pBase = null;
        }

    }
}
