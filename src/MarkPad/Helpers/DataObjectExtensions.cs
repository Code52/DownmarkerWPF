using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace MarkPad.Helpers
{
    public static class DataObjectExtensions
    {
        public static List<DataImage> GetImages(this IDataObject e)
        {
            // var formats = e.GetFormats();

            var images = new List<DataImage>();
            if (e.GetDataPresent(DataFormats.Dib))
            {
                var bitmap = CreateBitmapFromDib((Stream)e.GetData(DataFormats.Dib));
                images.Add(new DataImage { Bitmap = bitmap, BitmapSource = BitmapToSource(bitmap) });
            }

            else if (e.GetDataPresent(DataFormats.Bitmap))
            {
                var bitmap = (Bitmap)e.GetData(DataFormats.Bitmap);
                images.Add(new DataImage { Bitmap = bitmap, BitmapSource = BitmapToSource(bitmap) });
            }

                //Drag and drop from files
            else if (e.GetDataPresent(DataFormats.FileDrop))
            {
                var fileNames = e.GetData(DataFormats.FileDrop, true) as string[];

                if (fileNames != null)
                    foreach (var f in fileNames)
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.UriSource = new Uri("file:///" + f.Replace("\\", "/"));
                        bmp.EndInit();

                        images.Add(new DataImage { Bitmap = BitmapSourceToBitmap(bmp), BitmapSource = bmp });
                    }
            }

            return images;
        }

        // the CreateBitmapFromDib function was taken from 
        // http://www.codeproject.com/KB/GDI-plus/DIBtoBitmap.aspx?display=PrintAll&fid=355741&df=90&mpp=25&noise=3&sort=Position&view=Quick&select=2227947 
        private static Bitmap CreateBitmapFromDib(Stream dib)
        {
            // We create a new Bitmap File in memory. 
            // This is the easiest way to convert a DIB to Bitmap. 
            // No PInvoke needed. 
            var reader = new BinaryReader(dib);

            int headerSize = reader.ReadInt32();
            int pixelSize = (int)dib.Length - headerSize;
            int fileSize = 14 + headerSize + pixelSize;

            var bmp = new MemoryStream(fileSize);
            var writer = new BinaryWriter(bmp);

            // 1. Write Bitmap File Header:              
            writer.Write((byte)'B');
            writer.Write((byte)'M');
            writer.Write(fileSize);
            writer.Write(0);
            writer.Write(14 + headerSize);

            // 2. Copy the DIB  
            dib.Position = 0;
            var data = new byte[(int)dib.Length];
            dib.Read(data, 0, (int)dib.Length);
            writer.Write(data, 0, data.Length);

            // 3. Create a new Bitmap from our new stream: 
            bmp.Position = 0;
            return new Bitmap(bmp);
        }

        private static BitmapSource BitmapToSource(Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();
            var sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            var destination = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
            destination.Freeze();
            return destination;
        }

        private static Bitmap BitmapSourceToBitmap(BitmapSource source)
        {
            var bmp = new Bitmap(source.PixelWidth, source.PixelHeight, PixelFormat.Format32bppPArgb);

            var data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size),
                                    ImageLockMode.WriteOnly,
                                    PixelFormat.Format32bppPArgb);

            source.CopyPixels(Int32Rect.Empty,
                              data.Scan0,
                              data.Height * data.Stride,
                              data.Stride);

            bmp.UnlockBits(data);
            return bmp;
        }
    }
}