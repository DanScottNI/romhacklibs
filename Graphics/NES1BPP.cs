using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace ROMClass.Graphics.NES
{
    /// <summary>
    /// Summary description for NES1BPP.
    /// </summary>
    public unsafe class NES1BPP
    {
        private Bitmap bitmap;
        private int width;
        private BitmapData bitmapData = null;
        private Byte* pBase = null;

        /// <summary>
        /// Constructor that takes a bitmap to draw the tiles onto.
        /// </summary>
        public NES1BPP(Bitmap Bitmap)
        {
            this.bitmap = Bitmap;
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
        /// Draws a NES 1BPP tile to specified co-ordinates.
        /// </summary>
        /// <param name="x">The X co-ordinate of where the tile is to be drawn.</param>
        /// <param name="y">The Y co-ordinate of where the tile is to be drawn.</param>
        /// <param name="tileData">A pointer to the tile data.</param>
        /// <param name="paletteData">A pointer to the palette data.</param>
        public void DrawTile(int x, int y, byte* tileData, byte* paletteData)
        {
            Point pixsize = PixelSize;

            LockBitmap();

            byte temp2;

            int psize = 7;

            for (int y1 = 0; y1 <= psize; y1++)
            {
                for (int x1 = 0; x1 <= psize; x1++)
                {
                    temp2 = 0;


                    // TODO: Write the actual code for the format.
                    temp2 = (byte)((((*(tileData + y1)) & (0x80 >> x1)) >> (psize - x1)));

                    PixelData* pPixel = PixelAt((x + x1), (y + y1));

                    switch (temp2)
                    {
                        case 0:
                            pPixel->Red = Color.White.R;
                            pPixel->Green = Color.White.G;
                            pPixel->Blue = Color.White.B;
                            break;
                        case 1:
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
            return (PixelData*)(this.pBase + y * this.width + x * sizeof(PixelData));
        }

        /// <summary>
        /// Unlocks the bitmap's pixels in memory.
        /// </summary>
        public void UnlockBitmap()
        {
            bitmap.UnlockBits(this.bitmapData);
            this.bitmapData = null;
            this.pBase = null;
        }
    }
}