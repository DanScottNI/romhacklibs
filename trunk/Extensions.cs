using System;
using System.IO;

namespace ROMHackLib
{
    /// <summary>
    /// Class that provides easy conversion from hexadecimal values, to integers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts a hexadecimal value to an integer.
        /// </summary>
        /// <param name="hexValue">The hexadecimal value to convert.</param>
        /// <returns>An Integer value representing the hexadecimal input value.</returns>
        public static int HexValueToInt(this string hexValue)
        {
            return Convert.ToInt32(hexValue, 16);
        }

        /// <summary>
        /// Converts a hexadecimal value to an byte.
        /// </summary>
        /// <param name="hexValue">The hexadecimal value to convert.</param>
        /// <returns>A Byte value representing the hexadecimal input value.</returns>
        public static byte HexValueToByte(this string hexValue)
        {
            return Convert.ToByte(hexValue, 16);
        }

        /// <summary>
        /// Converts an integer into a hexadecimal string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A String containing the input number as a hexadecimal.</returns>
        public static string ToHex(this int value)
        {
            string temp = Convert.ToString(value, 16);

            if (temp.Length == 1)
            {
                temp = "0" + temp;
            }

            return temp;
        }

        /// <summary>
        /// Converts a byte into a hexadecimal string.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <returns>A String containing the input number as a hexadecimal.</returns>
        public static string ToHex(this byte value)
        {
            string temp = Convert.ToString(value, 16);

            if (temp.Length == 1)
            {
                temp = "0" + temp;
            }

            return temp;
        }

        /// <summary>
        /// Converts the byte array into a string of hexadecimal values, representing the elements in the array.
        /// </summary>
        /// <param name="value">The byte array to convert.</param>
        /// <returns>A string of hexadecimal values.</returns>
        public static string ToFormattedHex(this byte[] value)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                sb.Append("$" + value[i].ToHex() + " ");
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Converts the string into an ASCII encoded byte array.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A byte[] containing the string in ASCII format.</returns>
        public static byte[] ToByteArray(this string value)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetBytes(value);
        }

        /// <summary>
        /// Shortens a pathname for display purposes.
        /// </summary>
        /// <param name="pathname">The pathname to shorten.</param>
        /// <param name="maxLength">The maximum number of characters to be displayed.</param>
        /// <remarks>Shortens a pathname by either removing consecutive components of a path
        /// and/or by removing characters from the end of the filename and replacing
        /// then with three elipses (...)
        /// <para>In all cases, the root of the passed path will be preserved in it's entirety.</para>
        /// <para>If a UNC path is used or the pathname and maxLength are particularly short,
        /// the resulting path may be longer than maxLength.</para>
        /// <para>This method expects fully resolved pathnames to be passed to it.
        /// (Use Path.GetFullPath() to obtain this.)</para>
        /// </remarks>
        /// <returns>A string containing the pathname, shortened.</returns>
        public static string ShortenPathname(this string pathname, int maxLength)
        {
            if (pathname.Length <= maxLength)
            {
                return pathname;
            }

            string root = Path.GetPathRoot(pathname);
            
            if (root.Length > 3)
            {
                root += Path.DirectorySeparatorChar;
            }

            string[] elements = pathname.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            int filenameIndex = elements.GetLength(0) - 1;

            // pathname is just a root and filename
            if (elements.GetLength(0) == 1) 
            {
                // long enough to shorten
                if (elements[0].Length > 5) 
                {
                    // if path is a UNC path, root may be rather long
                    if (root.Length + 6 >= maxLength)
                    {
                        return root + elements[0].Substring(0, 3) + "...";
                    }
                    else
                    {
                        return pathname.Substring(0, maxLength - 3) + "...";
                    }
                }
            }
            else if ((root.Length + 4 + elements[filenameIndex].Length) > maxLength) 
            {
                // pathname is just a root and filename
                root += "...\\";

                int len = elements[filenameIndex].Length;
                if (len < 6)
                {
                    return root + elements[filenameIndex];
                }

                if ((root.Length + 6) >= maxLength)
                {
                    len = 3;
                }
                else
                {
                    len = maxLength - root.Length - 3;
                }
                return root + elements[filenameIndex].Substring(0, len) + "...";
            }
            else if (elements.GetLength(0) == 2)
            {
                return root + "...\\" + elements[1];
            }
            else
            {
                int len = 0;
                int begin = 0;

                for (int i = 0; i < filenameIndex; i++)
                {
                    if (elements[i].Length > len)
                    {
                        begin = i;
                        len = elements[i].Length;
                    }
                }

                int totalLength = pathname.Length - len + 3;
                int end = begin + 1;

                while (totalLength > maxLength)
                {
                    if (begin > 0)
                    {
                        totalLength -= elements[--begin].Length - 1;
                    }

                    if (totalLength <= maxLength)
                    {
                        break;
                    }

                    if (end < filenameIndex)
                    {
                        totalLength -= elements[++end].Length - 1;
                    }

                    if (begin == 0 && end == filenameIndex)
                    {
                        break;
                    }
                }

                // assemble final string
                for (int i = 0; i < begin; i++)
                {
                    root += elements[i] + '\\';
                }

                root += "...\\";

                for (int i = end; i < filenameIndex; i++)
                {
                    root += elements[i] + '\\';
                }

                return root + elements[filenameIndex];
            }
            return pathname;
        }
    }
}