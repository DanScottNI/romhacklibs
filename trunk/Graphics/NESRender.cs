using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ROMHackLib.Graphics.NES
{
    /// <summary>
    /// The available fonts for use.
    /// </summary>
    public enum NESFontType
    {
        /// <summary>
        /// A standard font.
        /// </summary>
        Standard = 0,
    }

    /// <summary>
    /// A class that draw NES 2BPP format tiles.
    /// </summary>
    public unsafe class NESRender : BaseRender
    {
        /// <summary>
        /// The colours that make up the NES's palette.
        /// </summary>
        private Color[] colourPalette;
        
        /// <summary>
        /// An array of font data.
        /// </summary>
        private byte[] fontData = new byte[704];
        
        /// <summary>
        /// The name of the font resource in this class library.
        /// </summary>
        private string fontName = "ROMClass.Graphics.Fonts.NES.Standard.chr";

        /// <summary>
        /// Gets or sets the colours that make up the NES's palette.
        /// </summary>
        public Color[] ColourPalette
        {
            get
            {
                return this.colourPalette;
            }
            
            set
            {
                this.colourPalette = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the NESRender class that loads the default palette.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        public NESRender(int width, int height)
            : base(width, height)
        {
            this.LoadDefaultPalette();
            this.LoadFontData();
        }

        /// <summary>
        /// Initializes a new instance of the NESRender class that takes a filename for the palette file.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="paletteFilename">The filename for the external palette file.</param>
        public NESRender(int width, int height, string paletteFilename)
            : base(width, height)
        {
            this.LoadPaletteFile(paletteFilename);
            this.LoadFontData();
        }

        /// <summary>
        /// Draws a NES 2BPP tile to specified co-ordinates.
        /// </summary>
        /// <param name="x">The X co-ordinate of where to draw the tile to.</param>
        /// <param name="y">The Y co-ordinate of where to draw the tile to.</param>
        /// <param name="tileData">A pointer to a byte array of tile data to draw the tile with.</param>
        /// <param name="paletteData">A pointer to a byte array of palette information to draw the tile with.</param>
        public void DrawTile(int x, int y, byte* tileData, byte* paletteData)
        {
            byte temp2;
            byte temp3;
            byte temp4;
            int tileSize = 7;

            for (int y1 = 0; y1 <= tileSize; y1++)
            {
                for (int x1 = 0; x1 <= tileSize; x1++)
                {
                    temp2 = 0;
                    temp3 = 0;

                    temp2 = Convert.ToByte((*(tileData + y1) & (0x80 >> x1)) >> (tileSize - x1));
                    temp3 = Convert.ToByte((*(tileData + y1 + 8) & (0x80 >> x1)) >> (tileSize - x1));
                    temp4 = Convert.ToByte((temp3 << 1) + temp2);

                    DrawPixel((x + x1), (y + y1), this.colourPalette[*(paletteData + temp4)]);
                }
            }
        }

        /// <summary>
        /// Draws the specified text to a set of co-ordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="text">The text to draw.</param>
        public override void DrawText(int x, int y, string text)
        {
            // Copy the characters to the character array.
            char[] characters = text.ToUpper().ToCharArray();
            byte[] pal = { 0x0F, 0x16, 0x21, 0x32 };

            fixed (byte* palpointer = pal)
            {
                fixed (byte* pointer = this.fontData)
                {
                    for (int i = 0; i < characters.Length; i++)
                    {
                        byte val = 0;
                        if (FontLookup.ContainsKey(characters[i]) == false)
                        {
                            val = FontLookup[' '];
                        }
                        else
                        {
                            val = FontLookup[characters[i]];
                        }

                        this.DrawTile(x + (i * 8), y, pointer + (val * 16), palpointer);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the specified text to a set of co-ordinates.
        /// </summary>
        /// <param name="bitmap">The Bitmap onto which the text should be rendered.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="text">The text to draw.</param>
        public void DrawText(ref Bitmap bitmap, int x, int y, string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads in a NES palette file.
        /// </summary>
        /// <param name="filename">The filename of the NES palette file.</param>
        private void LoadPaletteFile(string filename)
        {
            this.colourPalette = new Color[64];
            FileStream imageStream;

            byte[] rgb = new byte[3];

            imageStream = new FileStream(filename, System.IO.FileMode.Open);

            for (int i = 0; i < 64; i++)
            {
                imageStream.Read(rgb, 0, 3);
                this.colourPalette[i] = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }
            imageStream.Close();
        }

        /// <summary>
        /// Loads the font data.
        /// </summary>
        private void LoadFontData()
        {
            Assembly assembly;
            Stream imageStream;

            byte[] rgb = new byte[3];

            assembly = Assembly.GetExecutingAssembly();
            imageStream = assembly.GetManifestResourceStream(this.fontName);
            for (int i = 0; i < 704; i++)
            {
                this.fontData[i] = Convert.ToByte(imageStream.ReadByte());
            }
        }

        /// <summary>
        /// Loads in a palette from the resources section in the current assembly.
        /// </summary>
        private void LoadDefaultPalette()
        {
            this.colourPalette = new Color[64];
            Assembly assembly;
            Stream imageStream;

            byte[] rgb = new byte[3];

            assembly = Assembly.GetExecutingAssembly();
            imageStream = assembly.GetManifestResourceStream("ROMClass.Graphics.matrx21f.pal");
            for (int i = 0; i < 64; i++)
            {
                imageStream.Read(rgb, 0, 3);
                this.colourPalette[i] = Color.FromArgb(rgb[0], rgb[1], rgb[2]);
            }
            imageStream.Close();
        }
    }
}