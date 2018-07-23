using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Irseny.Util {
	public static class ImageTools {


		public static Gdk.Pixbuf Rotate(
			Gdk.Pixbuf source, float angle, RotatedImageSize size = RotatedImageSize.Minimized,
			RotatedImageAlpha alpha = RotatedImageAlpha.Source, Gdk.Color? fillColor = null, Gdk.Pixbuf target = null) {
			if (source == null) throw new ArgumentNullException("source");
			if (source.BitsPerSample != 8) throw new ArgumentException("source: Invalid bits per sample: " + source.BitsPerSample);
			if (source.NChannels != 3 && source.NChannels != 4) throw new ArgumentException("source: Invalid number of channels: " + source.NChannels);
			if (target != null) {
				if (target.BitsPerSample != 8) throw new ArgumentException("target: Invalid bits per sample: " + target.BitsPerSample);
				if (target.NChannels != 3 && target.NChannels != 4) throw new ArgumentException("target: Invalid number of channels: " + target.NChannels);
			}
			// rotation matrix entries
			float m11 = (float)Math.Cos(-angle); // rotation from target to source
			float m21 = (float)Math.Sin(-angle);
			float m12 = -m21;
			float m22 = m11;
			int sourceWidth = source.Width;
			int sourceHeight = source.Height;
			int halfSourceWidth = sourceWidth/2;
			int halfSourceHeight = source.Height/2;
			int sourceStride = source.Rowstride;
			// determine size of target image
			int targetWidth;
			int targetHeight;
			int postRotationOffsetX;
			int postRotationOffsetY;
			switch (size) {
			case RotatedImageSize.Minimized:
				// rotate corner points to determine the target pixbuf size
				float width1 = Math.Abs(m11*sourceWidth + m12*sourceHeight);
				float height1 = Math.Abs(m21*sourceWidth + m22*sourceHeight);
				float width2 = Math.Abs(m11*sourceWidth - m12*sourceHeight);
				float height2 = Math.Abs(m21*sourceWidth - m22*sourceHeight);
				targetWidth = (int)Math.Ceiling(Math.Max(width1, width2));
				targetHeight = (int)Math.Ceiling(Math.Max(height1, height2));
			break;
			case RotatedImageSize.Maximized:
				targetWidth = (int)Math.Ceiling(Math.Sqrt(sourceWidth*sourceWidth + sourceHeight*sourceHeight));
				targetHeight = targetWidth;
			break;
			case RotatedImageSize.Unchanged:
				if (target != null) {
					targetWidth = target.Width;
					targetHeight = target.Height;
					break;
				} else {
					targetWidth = sourceWidth;
					targetHeight = sourceHeight;
				}
			break;
			case RotatedImageSize.Source:
				targetWidth = sourceWidth;
				targetHeight = sourceHeight;
			break;
			default:
				throw new ArgumentException("size: Unknown value");
			}
			postRotationOffsetX = targetWidth/2;
			postRotationOffsetY = targetHeight/2;
			int halfTargetWidth = targetWidth/2;
			int halfTargetHeight = targetHeight/2;
			// pixel format
			int sourceChannels = source.NChannels;
			int sourcePixelSize = sourceChannels*source.BitsPerSample/8;
			int targetChannels;
			switch (alpha) {
			case RotatedImageAlpha.Disabled:
				targetChannels = 3;
			break;
			case RotatedImageAlpha.Enabled:
				targetChannels = 4;
			break;
			case RotatedImageAlpha.Source:
				targetChannels = sourceChannels;
			break;
			case RotatedImageAlpha.Unchanged:
				if (target != null) {
					targetChannels = target.NChannels;
				} else {
					targetChannels = source.NChannels;
				}
			break;
			default:
				throw new ArgumentException("alpha: Unknown value");
			}

			// create target if given instance is not usable
			if (target == null || targetWidth != target.Width || targetHeight != target.Height || target.NChannels != targetChannels) {
				target = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, targetChannels > 3, 8, targetWidth, targetHeight);
			}
			int targetPixelSize = targetChannels*source.BitsPerSample/8;
			int targetStride = target.Rowstride;
			bool alphaExtend = sourceChannels < targetChannels;
			bool alphaDiscard = sourceChannels > targetChannels;
			// default color
			// only RGB is supported so every pixel consists of 3 or 4 bytes depending on whether alpha is enabled
			uint defaultColor;
			Gdk.Color packedDefaultColor = fillColor.GetValueOrDefault(new Gdk.Color(255, 255, 255));
			if (sourceChannels > 3 || alphaExtend) {
				defaultColor = unchecked(
					(uint)(packedDefaultColor.Red & 0xFF)<<24 |
				(uint)(packedDefaultColor.Green & 0xFF)<<16 |
				(uint)(packedDefaultColor.Blue & 0xFF)<<8 |
				(packedDefaultColor.Pixel & 0xFF));
			} else {
				defaultColor = unchecked(
					(uint)(packedDefaultColor.Red & 0xFF)<<16 |
				(uint)(packedDefaultColor.Green & 0xFF)<<8 |
				(uint)(packedDefaultColor.Blue%0xFF));
			}
			/*if (targetChannels == sourceChannels) { // specialized cases
				if (targetChannels == 3) {

				} else if (sourceChannels == 4) {

				}

			} else if (targetChannels < sourceChannels) {

			} else if (sourceChannels > targetChannels) {

			}*/
			// copy pixel data

			IntPtr sourcePixels = source.Pixels;
			IntPtr targetPixels = target.Pixels;
			/*for (int targetY = 0; targetY < targetHeight; targetY++) {
				int currentTargetChannels = targetY < targetHeight - 1 ? 4 : targetChannels;
				for (int targetX = 0; targetX < targetWidth; targetX++) {
					float centerX = targetX - halfTargetWidth;
					float centerY = targetY - halfTargetHeight;
					float sourceX = m11 * centerX + m12 * centerY + halfSourceWidth;
					float sourceY = m21 * centerX + m22 * centerY + halfSourceHeight;
					int sourceX1 = (int)sourceX;
					int sourceY1 = (int)sourceY;
					uint color;
					if (sourceX1 < 0 || sourceX1 >= sourceWidth || sourceY1 < 0 || sourceY1 >= sourceHeight) {
						color = defaultColor;
					} else {
						color = unchecked((uint)Marshal.ReadInt32(sourcePixels, sourceY1 * sourceStride + sourceX1 * sourcePixelSize));
					}
					if (currentTargetChannels < 4) {
						Marshal.WriteInt16(targetPixels, targetY * targetStride + targetX * targetPixelSize, unchecked((short)(color>>8)));
						Marshal.WriteByte(targetPixels, targetY * targetStride + targetX * targetPixelSize + 2, unchecked((byte)color));
					} else {
						Marshal.WriteInt32(targetPixels, targetY * targetStride + targetX * targetPixelSize, unchecked((int)color));
					}
				}
			}*/
			for (int targetY = 0; targetY < targetHeight; targetY++) {				
				for (int targetX = 0; targetX < targetWidth; targetX++) {
					float centerX = targetX - halfTargetWidth;
					float centerY = targetY - halfTargetHeight;
					float sourceX = m11*centerX + m12*centerY + halfSourceWidth;
					float sourceY = m21*centerX + m22*centerY + halfSourceHeight;
					int sourceX1 = (int)sourceX;
					int sourceY1 = (int)sourceY;
					uint color;
					if (sourceX1 < 0 || sourceX1 >= sourceWidth || sourceY1 < 0 || sourceY1 >= sourceHeight) {						
						color = defaultColor;
					} else {
						int sourceOffset = sourceY1*sourceStride + sourceX1*sourcePixelSize;
						if (sourcePixelSize < 4) {
							color = unchecked((uint)Marshal.ReadInt16(sourcePixels, sourceOffset)<<8 |
							(uint)Marshal.ReadByte(sourcePixels, sourceOffset + 2));
							if (alphaExtend) {
								color = color<<8 | 0xFF;
							}
						} else {
							color = unchecked((uint)Marshal.ReadInt32(sourcePixels, sourceY1*sourceStride + sourceX1*sourcePixelSize));
						}

					}
					int targetOffset = targetY*targetStride + targetX*targetPixelSize;
					if (targetPixelSize < 4) {
						if (alphaDiscard) {
							color = color>>8;
						}
						Marshal.WriteInt16(targetPixels, targetOffset, unchecked((short)(color>>8)));
						Marshal.WriteByte(targetPixels, targetOffset + 2, unchecked((byte)color));
					} else {						
						Marshal.WriteInt32(targetPixels, targetY*targetStride + targetX*targetPixelSize, unchecked((int)color));
					}
				}
			}
			return target;
		}
		public enum RotatedImageSize {
			Source,
			Unchanged,
			Minimized,
			Maximized
		}

		public enum RotatedImageAlpha {
			Source,
			Unchanged,
			Enabled,
			Disabled
		}

	}
}