namespace tgalib_core;

/**
 *   <B>TGA file format (persistence of 2D raster graphics data).</B>
 *
 *   All Targa formats are identified by a Data Type field, which is a one
 *    byte binary integer located in byte three of the file.  The various
 *    file types specified by this field are as follows:<BR>
 *
 *      0  -  No image data included.
 *      1  -  Uncompressed, color-mapped images.
 *      2  -  Uncompressed, RGB images.
 *      3  -  Uncompressed, black and white images.
 *      9  -  Runlength encoded color-mapped images.
 *     10  -  Runlength encoded RGB images.
 *     11  -  Compressed, black and white images.
 *     32  -  Compressed color-mapped data, using Huffman, Delta, and
 *                   runlength encoding.
 *     33  -  Compressed color-mapped data, using Huffman, Delta, and
 *                   runlength encoding.  4-pass quadtree-type process.
 *
 * Extra documentation - Header of TGA
 *  
 * header[0]  = 0; 	no characters in identification field
 * header[1]  = 0; 	no color map present in this image
 * header[2]  = 2; 	Image Type Code.
 * header[3]  = 0; 	ignored fields (colormap info)
 * header[4]  = 0; 	ignored fields (colormap info)
 * header[5]  = 0; 	ignored fields (colormap info)
 * header[6]  = 0; 	ignored fields (colormap info)
 * header[7]  = 0; 	ignored fields (colormap info)
 * header[8]  = 0;
 * header[9]  = 0; 	X Origin of Image. (2 byte integer)
 * header[10] = 0;
 * header[11] = 0; 	Y Origin of Image. (2 byte integer)
 * header[12] = lo(width); 		LSB first
 * header[13] = hi(width); 		Width of Image.
 * header[14] = lo(height); 	LSB first
 * header[15] = hi(height); 	Height of Image.
 * header[16] = 24; 24 bits per pixel (3 byte RGB)
 * header[17] = 0;	0 bits for Alpha Channel, non-interleaved, Origin in lower left-hand corner
 * 
 * @author  Peter Truchly   */
public static class TgaFileFormat
{
	// Common save routine.
	public static void CommonSave(TgaMode curTgaMode, Stream stream, IImage g)
	{
		// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
		switch (curTgaMode)
		{
			case TgaMode.Rgb24Unc : TgaRgb24UncSave(stream, g); break; //RGB 24 UNCOMPRESSED
			case TgaMode.Rgb24Rle : TgaRgb24RleSave(stream, g); break; //RGB 24 RUN LENGTH COMPRESSED
			case TgaMode.Pal8Unc : TgaPal8UncSave(stream, g); break; //PALETTED 8 bit UNCOMPRESSED
			case TgaMode.Pal8Rle : TgaPal8RleSave(stream, g); break; //PALETTED 8 bit RUN LENGTH COMPRESSED
			default: throw new ArgumentOutOfRangeException(nameof(curTgaMode), curTgaMode, "Unsupported image format."); 
		}
	}

	// TGA RGB 24 RLE save code.
	private static void TgaRgb24RleSave(Stream tgaFile, IImage image)
	{
		int width = image.Width;
		int height = image.Height;
		
		//                1| 2| 3| 4| 5| 6| 7| 8| 9|10|11|12|               13|                    14|                15|                     16|17|18		    
		byte[] header = [ 0, 0,10, 0, 0, 0, 0, 0, 0, 0, 0, 0,(byte)(width&255),(byte)((width>>8)&255),(byte)(height&255),(byte)((height>>8)&255),24, 0];		
		tgaFile.Write(header);
		
		RleCompressor rlec = new(tgaFile);
		for (int y = 0; y < height; ++y) // TODO: according to v2 spec, there should not be a run overlapping 2 scanlines
			for (int x = 0; x < width; ++x)
			{ 
				image.GetPixelRgba(x, y, out int r, out int g, out int b, out int a);
				rlec.Write([(byte)(b & 255), (byte)(g & 255), (byte)(r & 255)]);
			}
			
		rlec.ForceWrite();
		tgaFile.Close();
	}
	
	// TGA PAL 8 RLE save code.
	// Seraches image for unique colors.
	// Stops when entire image is searched or palette is full (max 256 colors)
	// Colors which are not in palette are written as color 0 (there should be no more than 256 colors in image)
	// Performs RLE compression
	private static void TgaPal8RleSave(Stream tgaFile, IImage image)
	{
		int width = image.Width;
		int height = image.Height;
		const int maxColors = 256;
		
		//palette (BGR)
		List<byte[]> pal = [];
		Dictionary<string, int> hm = new();
		
		//serialize pixels, prepare palette (stores colors of pixel to hashmap and eliminates same colors)
		for (int x = 0; x < width; ++x)
			for(int y = 0; y < height; ++y)
			{
				image.GetPixelRgba(x, y, out int r, out int g, out int b, out int a);
				string key = $"{r & 255}|{g & 255}|{b & 255}";
				if (hm.ContainsKey(key) || hm.Count >= maxColors) continue;
				hm.TryAdd(key, pal.Count); // index of a color in palette
				pal.Add([(byte)(b & 255), (byte)(g & 255), (byte)(r & 255)]); // add palette entry
			}
		
		//                1| 2| 3| 4| 5|                     6|                         7| 8| 9|10|11|12|                13|                    14|                15|                     16|17|18
		byte[] header = [ 0, 1, 9, 0, 0, (byte)(pal.Count&255),(byte)((pal.Count>>8)&255),24, 0, 0, 0, 0, (byte)(width&255),(byte)((width>>8)&255),(byte)(height&255),(byte)((height>>8)&255), 8, 0 ];		
		tgaFile.Write(header);

		// write palette to file & init hashmap
		foreach (byte[] color in pal) { tgaFile.Write(color); }
		
		// run through image and code pixels as indexes using dictionary
		RleCompressor rlec = new(tgaFile);
		for(int y = 0; y < height; ++y) 
			for (int x = 0; x < width; ++x) 
			{
				image.GetPixelRgba(x, y, out int r, out int g, out int b, out int a);
				string key = $"{r & 255}|{g & 255}|{b & 255}";
				rlec.Write(hm.TryGetValue(key, out int value)?(byte)value:(byte)0); // write index of a color in palette
			}
			
		rlec.ForceWrite();//write last pixels
		tgaFile.Close();
	}	
	
	// TGA PAL 8 UNC save code.
	// Seraches image for unique colors.
	// Stops when entire image is searched or palette is full (max 256 colors)
	//   Colors which are not in palette are written as color 0 (there should be no more than 256 colors in image)
	private static void TgaPal8UncSave(Stream tgaFile, IImage image)
	{
		//header
		int width = image.Width;
		int height = image.Height;
		const int maxColors = 256;
		
		//palette ( colors are stored as BGR in file )
		List<byte[]> pal = [];
		Dictionary<string, int> hm = new();
		
		//serialize pixels, prepare palette (stores colors of pixel to hashmap and eliminates same colors)
		for (int x = 0; x < width; ++x) 
			for(int y = 0; y < height; ++y) 
			{
				image.GetPixelRgba(x, y, out int r, out int g, out int b, out int a);
				string key = $"{r & 255}|{g & 255}|{b & 255}";
				if (hm.ContainsKey(key) || hm.Count >= maxColors) continue;
				hm.TryAdd(key, pal.Count); // index of a color in palette
				pal.Add([(byte)(b & 255), (byte)(g & 255), (byte)(r & 255)]); //palette entry
			}
		
		//                1| 2| 3| 4| 5|                       6|                           7| 8| 9|10|11|12|                13|                    14|                15|                     16|17|18
		byte[] header = [ 0, 1, 1, 0, 0, (byte)(pal.Count & 255),(byte)((pal.Count >> 8)&255),24, 0, 0, 0, 0, (byte)(width&255),(byte)((width>>8)&255),(byte)(height&255),(byte)((height>>8)&255), 8, 0 ];		
		tgaFile.Write(header);

		// write palette to file & init hashmap
		foreach (byte[] color in pal) { tgaFile.Write(color); }
					
		// run through image and code pixels as indexes using dictionary
		for(int y = 0; y < height; ++y)
			for (int x = 0; x < width; ++x)
			{
				image.GetPixelRgba(x, y, out int r, out int g, out int b, out int a);
				string key = $"{r & 255}|{g & 255}|{b & 255}";
				tgaFile.Write([hm.TryGetValue(key, out int value)?(byte)value:(byte)0]);
			}

		tgaFile.Close();
	}

	// TGA RGB 24 UNC save code.
	private static void TgaRgb24UncSave(Stream tgaFile, IImage image)
	{
		int width = image.Width;
		int height = image.Height;
		
		//               1| 2| 3| 4| 5| 6| 7| 8| 9|10|11|12|               13|                    14|                15|                     16|17|18
		byte[] header = [0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0,(byte)(width&255),(byte)((width>>8)&255),(byte)(height&255),(byte)((height>>8)&255),24, 0];		
		tgaFile.Write(header);
		
		for (int y = 0; y < height; ++y)
			for (int x = 0; x < width; ++x)
			{ 
				image.GetPixelRgba(x, y, out int r, out int g, out int b, out int a);
				tgaFile.Write([(byte)(r & 255), (byte)(g & 255), (byte)(b & 255)]);
			}

		tgaFile.Close();
	}
}