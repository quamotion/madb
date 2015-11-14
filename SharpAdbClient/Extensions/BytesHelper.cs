// <copyright file="BytesHelper.cs" company="The Android Open Source Project, Ryan Conrad, Quamotion">
// Copyright (c) The Android Open Source Project, Ryan Conrad, Quamotion. All rights reserved.
// </copyright>

namespace SharpAdbClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static partial class ManagedAdbExtenstions
    {
        /// <summary>
        /// Ints the reverse for raw image.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        public static void IntReverseForRawImage(this byte[] source, Action<byte[]> action)
        {
            var step = 4;
            for (int i = 0; i < source.Count(); i += step)
            {
                var b = new byte[step];
                for (int x = b.Length - 1; x >= 0; --x)
                {
                    b[(step - 1) - x] = source[i + x];
                }

                b[2] = source[i + 0];
                b[1] = source[i + 1];
                b[0] = source[i + 2];
                b[3] = source[i + 3];

                action(b);
            }
        }
    }
}
