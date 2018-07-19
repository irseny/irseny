using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Irseny.Util {
	public static class ImageTools {


		public static Gdk.Pixbuf Rotate(Gdk.Pixbuf source, float angle, bool crop = false) {
			return Rotate(source, angle, new Gdk.Color(0, 0, 0), crop);
		}
		public static Gdk.Pixbuf Rotate(Gdk.Pixbuf source, float angle, Gdk.Color fillColor, bool crop = false) {
			float m11 = (float)Math.Cos(-angle); // rotation from target to source
			float m21 = (float)Math.Sin(-angle);
			float m12 = -m21;
			float m22 = m11;
			int sourceWidth = source.Width;
			int sourceHeight = source.Height;
			int halfSourceWidth = sourceWidth/2;
			int halfSourceHeight = source.Height/2;
			int sourceStride = source.Rowstride;
			int targetWidth;
			int targetHeight;
			int postRotationOffsetX;
			int postRotationOffsetY;
			{
				// rotate corner points to determine the target pixbuf size
				float width1 = Math.Abs(m11*sourceWidth + m12*sourceHeight);
				float height1 = Math.Abs(m21*sourceWidth + m22*sourceHeight);
				float width2 = Math.Abs(m11*sourceWidth - m12*sourceHeight);
				float height2 = Math.Abs(m21*sourceWidth - m22*sourceHeight);
				targetWidth = (int)Math.Ceiling(Math.Max(width1, width2));
				targetHeight = (int)Math.Ceiling(Math.Max(height1, height2));
				postRotationOffsetX = targetWidth/2;
				postRotationOffsetY = targetHeight/2;
			}
			if (crop) {
				targetWidth = sourceWidth;
				targetHeight = sourceHeight;
			}
			int halfTargetWidth = targetWidth/2;
			int halfTargetHeight = targetHeight/2;
			int pixelSize = 3;
			// get access to pixel data
			// only RGB is supported so every pixel consists of 3 bytes
			int defaultColor = (fillColor.Red & 0xFF)<<16 | (fillColor.Green & 0xFF)<<8 | (fillColor.Blue & 0xFF);
			var target = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, targetWidth, targetHeight);
			int targetStride = target.Rowstride; // can differ from width*3;
			IntPtr sourcePixels = source.Pixels;
			IntPtr targetPixels = target.Pixels;
			for (int targetY = 0; targetY < targetHeight; targetY++) {
				for (int targetX = 0; targetX < targetWidth; targetX++) {
					float centerX = targetX - halfTargetWidth;
					float centerY = targetY - halfTargetHeight;
					float sourceX = m11*centerX + m12*centerY + halfSourceWidth;
					float sourceY = m21*centerX + m22*centerY + halfSourceHeight;
					int sourceX1 = (int)sourceX;
					int sourceY1 = (int)sourceY;
					if (sourceX1 < 0 || sourceX1 >= sourceWidth || sourceY1 < 0 || sourceY1 >= sourceHeight) {
						if (targetY + 1 < targetHeight || targetX + 1 < targetWidth) { // last pixel outside bounds
							Marshal.WriteInt32(targetPixels, targetY*targetStride + targetX*pixelSize, defaultColor);
						}

					} else {						
						int color = Marshal.ReadInt32(sourcePixels, sourceY1*sourceStride + sourceX1*pixelSize);
						Marshal.WriteInt32(targetPixels, targetY*targetStride + targetX*pixelSize, color);
					}
				}
			}
			return target;
		}
	}
}