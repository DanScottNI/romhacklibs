using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace ROMHackLib.NES
{
    /// <summary>
    /// An enumerated type for the different types of mirrorings in NES ROMs.
    /// </summary>
    public enum INESMirroring
    {
        /// <summary>
        /// Horizontal mirroring.
        /// </summary>
        Horizontal,

        /// <summary>
        /// Vertical mirroring.
        /// </summary>
        Vertical
    }

    /// <summary>
    /// A class that handles iNES format NES ROMs.
    /// </summary>
    public class INESROMImage : BaseROMImage
    {
        /// <summary>
        /// An array containing the backup data for a bank.
        /// </summary>
        private byte[] backupBank = new byte[0x4000];

        /// <summary>
        /// An array of the names of the mappers.
        /// </summary>
        private string[] mapperNames = new string[256];

        /// <summary>
        /// Initializes a new instance of the INESROMImage class. Calls the ancestor
        /// classes's constructor, and loads the mapper names into memory.
        /// </summary>
        /// <param name="filename">The filename for a ROM.</param>
        public INESROMImage(string filename)
            : base(filename)
        {
            if (this.ValidImage() == false)
            {
                throw new NotValidiNESImage();
            }
            this.LoadMapperNames();
        }

        /// <summary>
        /// An overloaded version of PointerToOffset that provides a default bank of the bank that
        /// the pointer resides in, a default Memory Address of 0x8000 and a default bank size of 0x4000.
        /// </summary>
        /// <param name="offset">The ROM offset that the pointer is located at.</param>
        /// <returns>An offset for the specified pointer.</returns>
        public int PointerToOffset(int offset)
        {
            return this.PointerToOffset(offset, 0, 0x8000, 0x4000);
        }

        /// <summary>
        /// An overloaded version of PointerToOffset that provides a default Memory Address of 0x8000
        /// and a default bank size of 0x4000.
        /// </summary>
        /// <param name="offset">The ROM offset that the pointer is located at.</param>
        /// <param name="bank">The bank that contains the data that the pointer points to.</param>
        /// <returns>An offset for the specified pointer.</returns>
        public int PointerToOffset(int offset, int bank)
        {
            return this.PointerToOffset(offset, bank, 0x8000, 0x4000);
        }

        /// <summary>
        /// An overloaded version of PointerToOffset that provides a default banksize of 0x4000.
        /// </summary>
        /// <param name="offset">The ROM offset that the pointer is located at.</param>
        /// <param name="bank">The bank that contains the data that the pointer points to.</param>
        /// <param name="memoryAddress">The memory address for the bank that the pointer points to.</param>
        /// <returns>An offset for the specified pointer.</returns>
        public int PointerToOffset(int offset, int bank, int memoryAddress)
        {
            return this.PointerToOffset(offset, bank, memoryAddress, 0x4000);
        }

        /// <summary>
        /// Takes an offset for a pointer, and 
        /// works out the offset for it.
        /// </summary>
        /// <param name="offset">The ROM offset that the pointer is located at.</param> 
        /// <param name="bank">The bank that contains the data that the pointer points to.</param>
        /// <param name="memoryAddress">The memory address for the bank that the pointer points to.</param>
        /// <param name="bankSize">The size of a bank.</param>
        /// <returns>An offset for the specified pointer.</returns>
        public int PointerToOffset(int offset, int bank, int memoryAddress, int bankSize)
        {
            int tempPointer;
            int tempBank;

            // Now we work out where the pointer leads to.
            tempPointer = (RawROM[offset + 1] << 8) + RawROM[offset];

            if (bank == 0x0)
            {
                tempBank = offset;
            }
            else
            {
                tempBank = bank;
            }

            tempBank = (((tempBank - 0x10) / bankSize) * bankSize) + 0x10;

            return (tempPointer - memoryAddress) + tempBank;
        }

        /// <summary>
        /// An overloaded version of OffsetToPointer that sets the MemAddress
        /// to $8000.
        /// </summary>
        /// <param name="offset">The offset to convert into a pointer.</param>
        /// <returns>The address at which the pointer points to.</returns>
        public int OffsetToPointer(int offset)
        {
            return this.OffsetToPointer(offset, 0x8000);
        }

        /// <summary>
        /// An overloaded version of OffsetToPointer. Passes a banksize of 0x4000
        /// to the full version of OffsetToPointer.
        /// </summary>
        /// <param name="offset">The offset to convert into a pointer.</param>
        /// <param name="memoryAddress">The memory address of the bank</param>
        /// <returns>An integer containing the pointer.</returns>
        public int OffsetToPointer(int offset, int memoryAddress)
        {
            return this.OffsetToPointer(offset, memoryAddress, 0x4000);
        }

        /// <summary>
        /// A function that converts a raw offset to a standard pointer.
        /// </summary>
        /// <param name="offset">The offset to convert to a standard pointer.</param>
        /// <param name="memoryAddress">The memory address of the bank.</param>
        /// <param name="bankSize">The size of the PRG bank.</param>
        /// <returns>An integer containing the pointer.</returns>
        public int OffsetToPointer(int offset, int memoryAddress, int bankSize)
        {
            return (offset - (((offset / bankSize) * bankSize) + 0x10)) + memoryAddress;
        }

        /// <summary>
        /// Takes an offset, converts it to a pointer, and stores it 
        /// in a location in the ROM.
        /// </summary>
        /// <param name="offsetPointer">The offset to convert into a pointer.</param>
        /// <param name="storeOffset">The offset at which the newly converted pointer gets stored.</param>
        /// <param name="memoryAddress">The memory address of the bank where the offset is located.</param>
        public void StorePointerOffset(int offsetPointer, int storeOffset, int memoryAddress)
        {
            string temp;
            temp = this.OffsetToPointer(offsetPointer, memoryAddress).ToString("X");

            RawROM[storeOffset] = Convert.ToByte("0x" + temp[3] + temp[4]);
            RawROM[storeOffset + 1] = Convert.ToByte("0x" + temp[1] + temp[2]);
        }

        /// <summary>
        /// Takes a ROM address of a CHR pointer, and works out the offset in
        /// the ROM that the pointer points to.
        /// </summary>
        /// <param name="offset">Offset requires the offset of the pointer.</param>
        /// <param name="bankNumber">Bank requires the index of the CHR bank that the pointer points to.</param>
        /// <returns>An integer containing the offset that the pointer points to.</returns>
        public int CHRPointerToOffset(int offset, int bankNumber)
        {
            return (RawROM[offset + 1] << 8) + RawROM[offset] + (this.PRG * 0x4000) + 0x10 + (bankNumber * 0x1000);
        }

        /// <summary>
        /// Takes a specific PRG bank in the NES ROM, and stores it in a temporary
        /// array, for backup purposes.
        /// </summary>
        /// <param name="bankNumber">The bank number to temporarily back up.</param>
        public void BackupBank(int bankNumber)
        {
            int offset = ((bankNumber * 0x4000) + 0x10);
            for (int i = 0; i <= 0x4000; i++)
            {
                this.backupBank[i] = RawROM[offset + i];
            }
        }

        /// <summary>
        /// Restores the bank that is currently backed up (from calling BackupBank)
        /// It can also be used to copy a entire bank from one location to another.
        /// </summary>
        /// <param name="bankNumber">The bank number to restore the bank into.</param>
        public void RestoreBank(int bankNumber)
        {
            int offset = ((bankNumber * 0x4000) + 0x10);
            for (int i = 0; i <= 0x4000; i++)
            {
                RawROM[offset + i] = this.backupBank[i];
            }
        }

        /// <summary>
        /// Loads a list of mapper names from a resource in the current assembly.
        /// </summary>
        public void LoadMapperNames()
        {
            StreamReader textStreamReader;
            Assembly assembly;
            assembly = Assembly.GetExecutingAssembly();
            textStreamReader = new StreamReader(assembly.GetManifestResourceStream("ROMClass.NES.MapperNames.txt"));
            for (int i = 0; i < 256; i++)
            {
                this.mapperNames[i] = textStreamReader.ReadLine();
            }
        }

        /// <summary>
        /// Gets or sets the number of PRG banks that the ROM contains, according to the iNES header.
        /// </summary>
        public byte PRG
        {
            get
            {
                return RawROM[0x4];
            }

            set
            {
                RawROM[0x4] = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of CHR banks that the ROM contains, according to the iNES header.
        /// </summary>
        public byte CHR
        {
            get
            {
                return RawROM[0x5];
            }

            set
            {
                RawROM[0x5] = value;
            }
        }

        /// <summary>
        /// Looks up a string array for the name of a memory mapper specified by Index.
        /// </summary>
        /// <returns>The name of the memory mapper.</returns>
        public string MapperName()
        {
            return this.mapperNames[this.MapperNumber];
        }

        /// <summary>
        /// Determines whether the ROM has the DiskDude! text in the 
        /// header, which could throw off any attempts to read the mapper number.
        /// </summary>
        /// <returns>Whether the ROM has the DiskDude! text corrupting the header.</returns>
        public bool DiskDude()
        {
            if (BitConverter.ToString(RawROM, 0x7, 9) == "DiskDude!")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ROM uses vertical, horizontal, or four-screen mirroring.
        /// </summary>
        public INESMirroring Mirroring
        {
            get
            {
                return (INESMirroring)(RawROM[0x6] & 0x1);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ROM uses SRAM.
        /// </summary>
        public bool SRAM
        {
            get
            {
                if ((RawROM[0x6] & 0x2) == 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ROM uses a four-screen VRAM layout
        /// </summary>
        public bool FourScreen
        {
            get
            {
                if ((RawROM[0x6] & 0x8) == 8)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether whether a trainer is present.
        /// </summary>
        public bool Trainer
        {
            get
            {
                if ((RawROM[0x6] & 0x4) == 4)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ROM is a VS System cartridge.
        /// </summary>
        public bool VSSystemCartridge
        {
            get
            {
                if ((RawROM[0x7] & 0x1) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the mapper number based on the values in the iNES
        /// header, taking into the account the DiskDude text.
        /// </summary>
        public int MapperNumber
        {
            get
            {
                if (this.DiskDude())
                {
                    return (RawROM[0x6] >> 4) & 0xF;
                }
                else
                {
                    return (RawROM[0x7] & 0xF0) + ((RawROM[0x6] >> 4) & 0xF);
                }
            }

            set
            {
                if (this.DiskDude())
                {
                    this.EraseDiskdude();
                }

                byte temp1 = (byte)(RawROM[0x7] & 0x0F);
                byte temp2 = (byte)(RawROM[0x6] & 0x0F);

                RawROM[0x7] = (byte)((value & 0xF0) + temp1);
                RawROM[0x6] = (byte)(((value & 0x0F) << 4) + temp2);
            }
        }

        /// <summary>
        /// Erases the diskdude text in the header if it exists.
        /// </summary>
        public void EraseDiskdude()
        {
            if (this.DiskDude())
            {
                for (int i = 7; i < 0x10; i++)
                {
                    RawROM[i] = 0x00;
                }
            }
        }

        /// <summary>
        /// Tests for the existence of the NES text that iNES ROMs are
        /// prefixed with.
        /// </summary>
        /// <returns>Whether the ROM is a valid iNES image.</returns>
        public bool ValidImage()
        {
            if ((BitConverter.ToString(RawROM, 0x0, 3) == "4E-45-53") && (RawROM[0x3] == 0x1A))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This function exports NES tile data in an uncompressed format.
        /// </summary>
        /// <param name="offset">The offset from where in the ROM to export.</param>
        /// <param name="destArray">The byte array that the graphics are going to.</param>
        public void ExportTile(int offset, ref byte[] destArray)
        {
            ////ExportTile(Offset, ref destArray);
        }

        /// <summary>
        /// This function exports NES tile data in an uncompressed format, with
        /// the ability to specify a size.
        /// </summary>
        /// <param name="offset">The offset from where in the ROM to export</param>
        /// <param name="destArray">The byte array that the graphics are going to.</param>
        /// <param name="size">The amount of tiles to export.</param>
        public void ExportTile(int offset, ref byte[] destArray, int size)
        {
        }

        /// <summary>
        /// Returns the size of a trainer, if present.
        /// </summary>
        /// <returns>The size of trainer. If one isn't present, return 0.</returns>
        public int TrainerSize()
        {
            if (this.Trainer == true)
            {
                return 0x200;
            }
            else
            {
                return 0x0;
            }
        }

        /// <summary>
        /// Returns the start of the CHR-ROM, if present.
        /// </summary>
        /// <returns>The start offset of CHR-ROM.</returns>
        public int CHRROMStartAddress()
        {
            if (this.CHR == 0)
            {
                throw new CHRROMNotPresent();
            }

            return (this.PRG * 0x4000) + 0x10 + this.TrainerSize();
        }

        /// <summary>
        /// Draws the palette used.
        /// </summary>
        /// <param name="bitmap">The bitmap to render the palette to.</param>
        /// <param name="pal">The NESRender class for which to retrieve the palette from.</param>
        public void RenderPalette(ref Bitmap bitmap, ref ROMHackLib.Graphics.NES.NESRender pal)
        {
            System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            for (int i = 0; i < 64; i++)
            {
                g.FillRectangle(new SolidBrush(pal.ColourPalette[i]), (i % 16) * 16, (i / 16) * 16, 16, 16);
            }
            g.Dispose();
        }
    }
}