using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;
using System.Linq;

namespace PtzJoystickControl.Gui.TrayIcon
{
    public class IconFactory
    {

        public const int MaxIconWidth = 256;

        public const int MaxIconHeight = 256;

        private const ushort HeaderReserved = 0;
        private const ushort HeaderIconType = 1;
        private const byte HeaderLength = 6;

        private const byte EntryReserved = 0;
        private const byte EntryLength = 16;

        private const byte PngColorsInPalette = 0;
        private const ushort PngColorPlanes = 1;

        public static WindowIcon PngsToMultiSizeIcon(IEnumerable<Bitmap> images)
        {
            if (images == null)
                throw new ArgumentNullException("images");


            // validates the pngs
            //ThrowForInvalidPngs(images);

            Bitmap[] orderedImages = images.OrderBy(i => i.Size.Width)
                                           .ThenBy(i => i.Size.Height)
                                           .ToArray();

            MemoryStream ms = new MemoryStream();
            using (var writer = new BinaryWriter(ms))
            {

                // write the header
                writer.Write(HeaderReserved);
                writer.Write(HeaderIconType);
                writer.Write((ushort)orderedImages.Length);

                // save the image buffers and offsets
                Dictionary<uint, byte[]> buffers = new Dictionary<uint, byte[]>();

                // tracks the length of the buffers as the iterations occur
                // and adds that to the offset of the entries
                uint lengthSum = 0;
                uint baseOffset = (uint)(HeaderLength + EntryLength * orderedImages.Length);

                for (int i = 0; i < orderedImages.Length; i++)
                {
                    Bitmap image = orderedImages[i];

                    // creates a byte array from an image
                    byte[] buffer = CreateImageBuffer(image);

                    // calculates what the offset of this image will be
                    // in the stream
                    uint offset = (baseOffset + lengthSum);

                    // writes the image entry
                    writer.Write(GetIconWidth(image));
                    writer.Write(GetIconHeight(image));
                    writer.Write(PngColorsInPalette);
                    writer.Write(EntryReserved);
                    writer.Write(PngColorPlanes);
                    writer.Write((ushort)PixelFormat.Rgba8888);
                    writer.Write((uint)buffer.Length);
                    writer.Write(offset);

                    lengthSum += (uint)buffer.Length;

                    // adds the buffer to be written at the offset
                    buffers.Add(offset, buffer);
                }

                // writes the buffers for each image
                foreach (var kvp in buffers)
                {

                    // seeks to the specified offset required for the image buffer
                    writer.BaseStream.Seek(kvp.Key, SeekOrigin.Begin);

                    // writes the buffer
                    writer.Write(kvp.Value);
                }

                writer.BaseStream.Position = 0;

                return new WindowIcon(writer.BaseStream);
            }

        }

        private static void ThrowForInvalidPngs(IEnumerable<Bitmap> images)
        {
            foreach (var image in images)
            {
                if (image.Size.Width > MaxIconWidth ||
                    image.Size.Height > MaxIconHeight)
                {
                    throw new InvalidOperationException
                        (string.Format("Dimensions must be less than or equal to {0}x{1}",
                                       MaxIconWidth,
                                       MaxIconHeight));
                }
            }
        }

        private static byte GetIconHeight(Bitmap image)
        {
            if (image.Size.Height == MaxIconHeight)
                return 0;

            return (byte)image.Size.Height;
        }

        private static byte GetIconWidth(Bitmap image)
        {
            if (image.Size.Width == MaxIconWidth)
                return 0;

            return (byte)image.Size.Width;
        }

        private static byte[] CreateImageBuffer(Bitmap image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream);

                return stream.ToArray();
            }
        }

    }
}
