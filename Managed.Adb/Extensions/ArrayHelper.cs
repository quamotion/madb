// <copyright file="ArrayHelper.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace Managed.Adb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <ignore>true</ignore>
    public static partial class ManagedAdbExtenstions
    {
        /// <summary>
        /// Swaps an unsigned value around, and puts the result in an array that can be sent to a device.
        /// </summary>
        /// <param name="value">The value to swap.</param>
        /// <param name="dest">the destination array</param>
        /// <param name="offset">offset the offset in the array where to put the swapped value.</param>
        /// <remarks>Array length must be at least offset + 4</remarks>
        public static void Swap32bitsToArray(this int value, byte[] dest, int offset)
        {
            dest[offset] = (byte)(value & 0x000000FF);
            dest[offset + 1] = (byte)((value & 0x0000FF00) >> 8);
            dest[offset + 2] = (byte)((value & 0x00FF0000) >> 16);
            dest[offset + 3] = (byte)((value & 0xFF000000) >> 24);
        }

        /// <summary>
        /// Reads a signed 32 bit integer from an array coming from a device.
        /// </summary>
        /// <param name="value">the array containing the int</param>
        /// <param name="offset">the offset in the array at which the int starts</param>
        /// <returns>the integer read from the array</returns>
        public static int Swap32bitFromArray(this byte[] value, int offset)
        {
            int v = 0;
            v |= ((int)value[offset]) & 0x000000FF;
            v |= (((int)value[offset + 1]) & 0x000000FF) << 8;
            v |= (((int)value[offset + 2]) & 0x000000FF) << 16;
            v |= (((int)value[offset + 3]) & 0x000000FF) << 24;

            return v;
        }

        /// <summary>
        /// Reads an unsigned 16 bit integer from an array coming from a device,
        /// and returns it as an 'int'
        /// </summary>
        /// <param name="value">the array containing the 16 bit int (2 byte).</param>
        /// <param name="offset">the offset in the array at which the int starts</param>
        /// <remarks>Array length must be at least offset + 2</remarks>
        /// <returns>the integer read from the array.</returns>
        public static int SwapU16bitFromArray(this byte[] value, int offset)
        {
            int v = 0;
            v |= ((int)value[offset]) & 0x000000FF;
            v |= (((int)value[offset + 1]) & 0x000000FF) << 8;

            return v;
        }
    }
}
