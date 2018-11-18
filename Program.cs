using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassImageOverlay
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("Please specify all directories, first the overlayed directory, the overlaying directory, and then the output directory.");
				return 1;
			}
			String[] filesOverlayed = Directory.GetFiles(args[0]);
			String[] filesOverlaying = Directory.GetFiles(args[1]);
			Dictionary<Bitmap, String> images = new Dictionary<Bitmap, string>();
			foreach (String overlayedFile in filesOverlayed)
			{
				Bitmap overlayed = new Bitmap(overlayedFile);
				foreach (String overlayingFile in filesOverlaying)
				{
					Bitmap overlaying = new Bitmap(overlayingFile);
					String combinedPath = Path.GetFileNameWithoutExtension(overlayingFile) + "_" +
										  Path.GetFileName(overlayedFile);
					images.Add(Overlay(overlayed, overlaying), combinedPath);
					Console.WriteLine("Overlayed " + Path.GetFileNameWithoutExtension(overlayedFile) + " with " +
									  Path.GetFileNameWithoutExtension(overlayingFile) + "...");
				}
			}
			if (!args[2].EndsWith(Path.DirectorySeparatorChar.ToString())) args[2] += Path.DirectorySeparatorChar;
			foreach (KeyValuePair<Bitmap, String> image in images)
			{
				image.Key.Save(args[2] + image.Value);
			}
			return 0;
		}

		static Bitmap Overlay(Bitmap overlayed, Bitmap overlaying, bool adjustSize = true, bool onNonTransparentOnly = true)
		{
			Bitmap image = new Bitmap(overlayed.Width, overlayed.Height);
			if (adjustSize)
			{
				if (overlayed.Height != overlaying.Height)
				{
					overlaying = ResizeImage(overlaying, overlayed.Size);
				}
			}
			using (Graphics gr = Graphics.FromImage(image))
			{
				gr.DrawImage(overlayed, new Point(0, 0));
				gr.DrawImage(overlaying, new Point(0, 0));
			}
			return onNonTransparentOnly ? PaintIfTransparent(image, overlayed) : image;
		}

		static Bitmap PaintIfTransparent(Bitmap bitmap, Bitmap compare)
		{
			Bitmap b = new Bitmap(bitmap);
			for (int x = 0; x < b.Width; x++)
			{
				for (int y = 0; y < b.Height; y++)
				{
					if (compare.GetPixel(x, y).A < 0.75)
					{
						b.SetPixel(x, y, Color.Transparent);
					}
				}
			}
			return b;
		}

		/// <summary>
		/// Resize the image to the specified width and height.
		/// </summary>
		/// <param name="image">The image to resize.</param>
		/// <param name="width">The width to resize to.</param>
		/// <param name="height">The height to resize to.</param>
		/// <returns>The resized image.</returns>
		/// <remarks>Based on code by user mpen on StackOverflow: 
		/// https://stackoverflow.com/a/24199315/
		/// </remarks>
		public static Bitmap ResizeImage(Bitmap image, Size size)
		{
			var destRect = new Rectangle(Point.Empty, size);
			var destImage = new Bitmap(size.Width, size.Height);

			destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

			using (var graphics = Graphics.FromImage(destImage))
			{
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

				using (var wrapMode = new ImageAttributes())
				{
					wrapMode.SetWrapMode(WrapMode.TileFlipXY);
					graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
				}
			}

			return destImage;
		}
	}
}
