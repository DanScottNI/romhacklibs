using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ROMHackLib.Graphics
{
    /// <summary>
    /// Base render class for fast graphics rendering. This class cannot be instantiated, use one 
    /// of its descendants instead.
    /// </summary>
    public abstract unsafe class BaseRender
    {
        /// <summary>
        /// The byte array containing the image data.
        /// </summary>
        protected byte[] imageData;

        /// <summary>
        /// The size of a row in _ImageData
        /// </summary>
        private int rowWidth;

        /// <summary>
        /// The width of the image.
        /// </summary>
        private int width;

        /// <summary>
        /// The height of the image.
        /// </summary>
        private int height;

        /// <summary>
        /// A dictionary of values to use when looking up characters.
        /// </summary>
        private Dictionary<char, byte> fontLookup;

        /// <summary>
        /// Gets the height of the data array.
        /// </summary>
        public int Height
        {
            get
            {
                return this.height;
            }
        }

        /// <summary>
        /// Gets the width of the data array.
        /// </summary>
        public int Width
        {
            get
            {
                return this.width;
            }
        }

        /// <summary>
        /// Gets or sets the size of a row in the imageData array
        /// </summary>
        protected int RowWidth
        {
            get
            {
                return this.rowWidth;
            }

            set
            {
                this.rowWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets a dictionary of values to use when looking up characters.
        /// </summary>
        protected Dictionary<char, byte> FontLookup
        {
            get
            {
                return this.fontLookup;
            }

            set
            {
                this.fontLookup = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the BaseRender class. 
        /// Sets up the array of data for the image, and the font lookup table.
        /// </summary>
        /// <param name="imagewidth">The width of the image.</param>
        /// <param name="imageheight">The height of the image.</param>
        public BaseRender(int imagewidth, int imageheight)
        {
            this.width = imagewidth;
            this.height = imageheight;
            this.SetupArray();
            this.SetupFontLookup();
        }

        /// <summary>
        /// Draws the entire tile area to a bitmap.
        /// </summary>
        /// <param name="bitmap">The Bitmap object to draw the tile data to.</param>
        public void DrawBitmap(ref Bitmap bitmap)
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF boundsF = bitmap.GetBounds(ref unit);
            Rectangle bounds = new Rectangle((int)boundsF.X, (int)boundsF.Y, (int)boundsF.Width, (int)boundsF.Height);
            BitmapData bitmapData = null;

            bitmapData = bitmap.LockBits(bounds, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            ////Marshal.Copy(bitmapData.Scan0, this.DestData, 0, DestData.Length);
            Marshal.Copy(this.imageData, 0, bitmapData.Scan0, this.imageData.Length);
            bitmap.UnlockBits(bitmapData);

            bitmapData = null;
        }

        /// <summary>
        /// Draws portions of the tile area to a bitmap.
        /// </summary>
        /// <param name="bitmap">The Bitmap object to draw the tile data to.</param>
        /// <param name="sourceRect">A Rectangle object which defines the area on the data array from which to copy the tile data.</param>
        /// <param name="destRect">A Rectangle object which defines where to draw the sourceRect to on the bitmap parameter.</param>
        public void DrawBitmap(ref Bitmap bitmap, Rectangle sourceRect, Rectangle destRect)
        {
            BitmapData bitmapData = null;

            // Lock the source portion of the bitmap.
            bitmapData = bitmap.LockBits(destRect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            // Retrieve the width of each row in the destination bitmap.
            int destWidth = bitmapData.Stride;

            if (sourceRect.Height > 0)
            {
                // Calculate the X position of the row
                int sourceXPosition = (sourceRect.X * sizeof(PixelData));
                int sourceXPositionEnd = sourceXPosition + (sourceRect.Width * sizeof(PixelData));
                int sourceLength = sourceXPositionEnd - sourceXPosition;

                int destXPosition = (destRect.X * sizeof(PixelData));

                // Get the address of the first line.
                byte* ptr = (byte*)bitmapData.Scan0;

                for (int i = sourceRect.Y; i < (sourceRect.Height + sourceRect.Y); i++)
                {
                    int sourceYPos = (i * this.rowWidth);
                    int destYPos = (i * destWidth);
                    int sourceArrayIndex = sourceYPos + sourceXPosition;
                    int destArrayIndex = destYPos + destXPosition;

                    for (int x = 0; x < sourceLength; x++)
                    {
                        *(ptr + x) = this.imageData[sourceArrayIndex + x];
                    }
                    ptr += destWidth;
                }
            }

            bitmap.UnlockBits(bitmapData);
            bitmapData = null;
        }

        /// <summary>
        /// A method to draw the text. Descendants of this class implement this method.
        /// </summary>
        /// <param name="x">The X co-ordinate to draw the text to.</param>
        /// <param name="y">The Y co-ordinate to draw the text to.</param>
        /// <param name="text">The text to draw.</param>
        public abstract void DrawText(int x, int y, string text);

        /// <summary>
        /// A method to draw a rectangle.
        /// </summary>
        /// <param name="rect">A Rectangle object representing the area to draw the rectangle in.</param>
        /// <param name="colour">The colour to render the bitmap with.</param>
        public void DrawRectangle(Rectangle rect, Color colour)
        {
            for (int x = rect.Y; x < rect.Bottom; x++)
            {
                for (int i = rect.X; i < rect.Right; i++)
                {
                    this.DrawPixel(i, x, colour);
                }
            }
        }

        /// <summary>
        /// Sets a pixel at the specified co-ordinates, with the specific colour.
        /// </summary>
        /// <param name="x">The X co-ordinate at which to set the pixel.</param>
        /// <param name="y">The Y co-ordinate at which to set the pixel.</param>
        /// <param name="palette">The color to set the pixel to.</param>
        protected void DrawPixel(int x, int y, Color palette)
        {
            int loc;

            if (x > this.rowWidth)
            {
                throw new Exception("The X co-ordinate is greater than the row's width.");
            }

            loc = (y * this.rowWidth) + (x * sizeof(PixelData));

            this.imageData[loc + 3] = palette.A;
            this.imageData[loc + 2] = palette.R;
            this.imageData[loc + 1] = palette.G;
            this.imageData[loc] = palette.B;
        }

        /// <summary>
        /// Setups the array for the image data.
        /// </summary>
        private void SetupArray()
        {
            Rectangle bounds = new Rectangle(0, 0, this.Width, this.Height);

            // Figure out the number of bytes in a row
            // This is rounded up to be a multiple of 4
            // bytes, since a scan line in an image must always be a multiple of 4 bytes
            // in length. 
            this.rowWidth = this.GetRowWidth(bounds.Width);

            this.imageData = new byte[bounds.Width * bounds.Height * sizeof(PixelData)];
        }

        /// <summary>
        /// Gets the width of the row of the image.
        /// </summary>
        /// <param name="imageWidth">Width of the image.</param>
        /// <returns>An integer representing the width of the row.</returns>
        private int GetRowWidth(int imageWidth)
        {
            int tempwidth;
            tempwidth = imageWidth * sizeof(PixelData);
            if (tempwidth % 4 != 0)
            {
                tempwidth = 4 * ((tempwidth / 4) + 1);
            }

            return tempwidth;
        }

        /// <summary>
        /// Setups the font lookup table.
        /// </summary>
        private void SetupFontLookup()
        {
            this.fontLookup = new Dictionary<char, byte>();
            this.fontLookup.Add('A', 0);
            this.fontLookup.Add('B', 1);
            this.fontLookup.Add('C', 2);
            this.fontLookup.Add('D', 3);
            this.fontLookup.Add('E', 4);
            this.fontLookup.Add('F', 5);
            this.fontLookup.Add('G', 6);
            this.fontLookup.Add('H', 7);
            this.fontLookup.Add('I', 8);
            this.fontLookup.Add('J', 9);
            this.fontLookup.Add('K', 10);
            this.fontLookup.Add('L', 11);
            this.fontLookup.Add('M', 12);
            this.fontLookup.Add('N', 13);
            this.fontLookup.Add('O', 14);
            this.fontLookup.Add('P', 15);
            this.fontLookup.Add('Q', 16);
            this.fontLookup.Add('R', 17);
            this.fontLookup.Add('S', 18);
            this.fontLookup.Add('T', 19);
            this.fontLookup.Add('U', 20);
            this.fontLookup.Add('V', 21);
            this.fontLookup.Add('W', 22);
            this.fontLookup.Add('X', 23);
            this.fontLookup.Add('Y', 24);
            this.fontLookup.Add('Z', 25);
            this.fontLookup.Add('0', 26);
            this.fontLookup.Add('1', 27);
            this.fontLookup.Add('2', 28);
            this.fontLookup.Add('3', 29);
            this.fontLookup.Add('4', 30);
            this.fontLookup.Add('5', 31);
            this.fontLookup.Add('6', 32);
            this.fontLookup.Add('7', 33);
            this.fontLookup.Add('8', 34);
            this.fontLookup.Add('9', 35);
            this.fontLookup.Add('!', 36);
            this.fontLookup.Add('?', 37);
            this.fontLookup.Add('"', 38);
            this.fontLookup.Add(',', 39);
            this.fontLookup.Add('.', 40);
            this.fontLookup.Add('\'', 41);
            this.fontLookup.Add('-', 42);
            this.fontLookup.Add(' ', 43);
        }
    }
}