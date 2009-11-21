using System;
using System.Collections.Generic;
using System.IO;
namespace ROMHackLib
{
    /// <summary>
    /// An encapsulation of the basic
    /// properties that a ROM object should have.
    /// </summary>
    /// <remarks>
    /// This class should never really be initiated in code.
    /// It should be descended from and system specific
    /// classes should be created.
    /// </remarks>
    public class BaseROMImage
    {
        /// <summary>
        /// Gets or sets the ROM's filename.
        /// </summary>
        public string Filename { get; set; }
        
        /// <summary>
        /// Whether the ROM has been changed.
        /// </summary>
        private bool changed;
        
        /// <summary>
        /// Whether or not to ignore changes.
        /// </summary>
        private bool ignoreChanges;
        
        /// <summary>
        /// Whether or not to track all reads in the ROM data array.
        /// </summary>
        private bool trackReads = false;

        /// <summary>
        /// Gets or sets a value indicating whether the reads to the ROM should be tracked.
        /// </summary>
        public bool TrackReads
        {
            get
            {
                return this.trackReads;
            }
            set
            {
                this.trackReads = value;
                if (value == true)
                {
                    this.InitialiseByteRead();
                }
            }
        }

        /// <summary>
        /// A byte array that is used to store the ROM in memory. Declared as public
        /// just incase a pointer is needed.
        /// </summary>
        public byte[] RawROM;

        /// <summary>
        /// An array of boolean values that represent whether a byte in memory has been
        /// read.
        /// </summary>
        private bool[] byteRead;

        /// <summary>
        /// Gets or sets an array of boolean values that represent whether a byte in memory has been
        /// read.
        /// </summary>
        public bool[] ByteRead
        {
            get
            {
                return this.byteRead;
            }

            set
            {
                this.byteRead = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the BaseROMImage class. Stores a local copy of the
        /// Filename parameter, loads the ROM, and initialises some variables.
        /// </summary>
        /// <param name="filename">Takes the filename for a ROM image.</param>
        public BaseROMImage(string filename)
        {
            this.Filename = filename;
            this.LoadROM(this.Filename);
            this.changed = false;
            this.ignoreChanges = false;
        }

        /// <summary>
        /// Saves the ROM to the filename that was used to load the ROM from.
        /// </summary>
        public void Save()
        {
            this.SaveROM(this.Filename);
        }

        /// <summary>
        /// Sets the length of the ROM.
        /// </summary>
        /// <param name="size">The size in bytes to set the ROM to.</param>
        public void SetROMLength(int size)
        {
            byte[] tempBuffer;
            tempBuffer = new byte[this.RawROM.Length];
            this.RawROM.CopyTo(tempBuffer, 0);
            this.RawROM = new byte[size];
            tempBuffer.CopyTo(this.RawROM, 0);
        }

        /// <summary>
        /// Saves the ROM to a given filename.
        /// </summary>
        /// <param name="filename">The filename that the ROM gets saved as.</param>
        public void SaveAs(string filename)
        {
            this.SaveROM(filename);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ROM has had any bytes modified.
        /// </summary>
        public bool IsChanged
        {
            get
            {
                return this.changed;
            }

            set
            {
                this.changed = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether writes to the ROM's bytes
        /// effect the Changed property.
        /// </summary>
        public bool IgnoreChanges
        {
            get
            {
                return this.ignoreChanges;
            }
            set
            {
                this.ignoreChanges = value;
            }
        }

        /// <summary>
        /// Applies an IPS patch to the ROM.
        /// </summary>
        /// <param name="filename">The filename of the IPS patch to apply.</param>
        /// <returns>A Boolean value depending on whether the patch is successful.</returns>
        public bool ApplyIPSPatch(string filename)
        {
            int offset, ipsPosition;
            int dataSize, rleSize;
            byte rleVal;
            byte[] ipsByte;

            // First, it is best to load the IPS into memory. For
            // speed. The actual file that is being patched isn't
            // loaded into memory, as it needs to be dynamic-resized.
            // (Probably).
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                // Resize the ROM array to hold the entire file in memory.
                ipsByte = new byte[fs.Length];

                // Load the ROM file into the array ROM.
                fs.Read(ipsByte, 0, ipsByte.Length);

                // Close the file streams.
                fs.Close();
            }

            // Next, we check if the first five bytes of the
            // IPS file are 'PATCH'. If they are not display a messagebox
            // and exit the subroutine.
            if ((ipsByte[0] != 80) || (ipsByte[1] != 65) || (ipsByte[2] != 84) || (ipsByte[3] != 67) || (ipsByte[4] != 72))
            {
                return false;
            }

            // If we are here, the patch has a valid PATCH header.
            // Next, we need to load in the offset for the first patch
            // To do this, we set the IPSCounter value to
            ipsPosition = 5;

            while (ipsPosition < ipsByte.Length)
            {
                // If we are at the end of the patch, there should be an EOF text.
                // if it exists, break out of the while loop.
                if ((ipsByte[ipsPosition] == 69) && (ipsByte[ipsPosition + 1] == 79) && (ipsByte[ipsPosition + 2] == 70))
                {
                    break;
                }

                // First load in the offset (3-bytes)
                offset = (ipsByte[ipsPosition] << 16) + (ipsByte[ipsPosition + 1] << 8) + ipsByte[ipsPosition + 2];
                ipsPosition = ipsPosition + 3;

                // Next load in the size of the bytes (2-bytes)
                dataSize = (ipsByte[ipsPosition] << 16) + ipsByte[ipsPosition + 1];
                ipsPosition = ipsPosition + 2;

                // Now we need to check if the size value is 0.
                // if it is, it's using RLE.
                if (dataSize == 0)
                {
                    rleSize = (ipsByte[ipsPosition] << 8) + ipsByte[ipsPosition + 1];
                    ipsPosition = ipsPosition + 2;

                    rleVal = ipsByte[ipsPosition];
                    ipsPosition++;

                    for (int i = 0; i < rleSize; i++)
                    {
                        this.RawROM[offset + i] = rleVal;
                    }
                }
                else if (dataSize > 0)
                {
                    for (int i = ipsPosition; i < (ipsPosition + dataSize); i++)
                    {
                        this.RawROM[offset + (i - ipsPosition)] = ipsByte[i];
                    }
                    ipsPosition += dataSize;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Byte"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the ROM array.</param>
        public byte this[int index]
        {
            get
            {
                if (index < this.RawROM.Length)
                {
                    if (this.trackReads == true)
                    {
                        this.byteRead[index] = true;
                    }

                    return this.RawROM[index];
                }
                else
                {
                    throw new OffsetOutOfRange();
                }
            }

            set
            {
                if (index < this.RawROM.Length)
                {
                    if ((this.ignoreChanges == false) && (this.RawROM[index] != value))
                    {
                        this.changed = true;
                    }

                    this.RawROM[index] = value;
                }
                else
                {
                    throw new OffsetOutOfRange();
                }
            }
        }

        /// <summary>
        /// Reads a colon separated value file into a Dictionary. 
        /// </summary>
        /// <remarks>
        /// The format of the file is usually a hexadecimal value representing the index, 
        /// followed by the colon character, then the name or information about the item.
        /// </remarks>
        /// <param name="filename">The filename of the colon-separated file.</param>
        /// <returns>A Dictionary of type int, and string, containing the elements from the file.</returns>
        public Dictionary<int, string> ReadColonSeparatedFile(string filename)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(filename))
            {
                string line;

                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    // If the line starts with a double-slash (//) then skip it.
                    if (!line.StartsWith("//"))
                    {
                        string[] items = line.Split(':');
                        if (items.Length == 2)
                        {
                            dict.Add(items[0].HexValueToInt(), items[1]);
                        }
                    }
                }
            }
            return dict;
        }

        /// <summary>
        /// Loads the ROM into memory.
        /// </summary>
        /// <param name="filename">The filename to load the ROM from.</param>
        protected void LoadROM(string filename)
        {
            // Create a filestream object to load in the ROM.
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                // Resize the ROM array to hold the entire file in memory.
                this.RawROM = new byte[fs.Length];

                // Load the ROM file into the array ROM.
                fs.Read(this.RawROM, 0, this.RawROM.Length);

                // Close the file streams.
                fs.Close();
            }
        }

        /// <summary>
        /// Saves the ROM.
        /// </summary>
        /// <param name="filename">The filename to save the ROM to.</param>
        protected void SaveROM(string filename)
        {
            // Create a filestream object to load in the ROM.
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                fs.Write(this.RawROM, 0, this.RawROM.Length);
                fs.Close();

                // Reset the changed variable to false;
                this.changed = false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ROM is readonly.
        /// </summary>
        protected bool ReadOnly
        {
            get
            {
                if ((File.GetAttributes(this.Filename) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if ((File.GetAttributes(this.Filename) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                {
                    File.SetAttributes(this.Filename, File.GetAttributes(this.Filename) | FileAttributes.ReadOnly);
                }
            }
        }

        /// <summary>
        /// Returns the size of the ROM.
        /// </summary>
        /// <returns>Size of the ROM.</returns>
        protected int GetROMSize()
        {
            return this.RawROM.Length;
        }

        /// <summary>
        /// Initialises the read tracking.
        /// </summary>
        private void InitialiseByteRead()
        {
            this.byteRead = new bool[this.RawROM.Length];
        }
    }
}
