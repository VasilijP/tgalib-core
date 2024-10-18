namespace tgalib_core;

//auxiliary class for RLE compression
public class RleCompressor(Stream stream)
{ 
	public const int Buflen = 8*1024;
	private readonly RlePixel[] buf = new RlePixel[Buflen];
	private int bufindex = 0;//index of first free place			

	public void Write(byte arg) { Write([arg]); }
	
	//add arg to FIFO
	public void Write(byte[] arg)
	{
		if (bufindex > 0 && buf[bufindex-1].Rep < 127 && buf[bufindex-1].C.SequenceEqual(arg)) { buf[bufindex-1].Rep++; return; } // increment repetition counter
		buf[bufindex++] = new RlePixel(arg);
		if (bufindex ==  Buflen) { ForceWrite(); }
	}
	
	private static readonly byte[] Header = new byte[1];
	
	//write one copy or run packet
	private void WritePacket(int from, int to)
	{
		if (to - from  > 0) { Header[0] = (byte)(to - from); } // create copy packet, number of copied pixels - 1 
		else { Header[0] = (byte)(128 | buf[from].Rep); }      // write run packet, number of repetition -1 and highest bit is 1
		stream.Write(Header);
		for (int rpc = from; rpc <= to; rpc++) { stream.Write(buf[rpc].C); } // this should run only once for run packet
	}
	
	//forced write of entire buffer to output (end of image or full buffer)
	public void ForceWrite()
	{
		int from = 0;
		while (from < bufindex)
		{
			int to = from;
			while (buf[to].Rep == 0 && to+1 < bufindex && to-from < 127) { to++; }
			if (to - from > 0 && buf[to].Rep > 0) { to--; }
			WritePacket(from, to);
			from = to+1;
		}
		bufindex = 0; //empty buffer
	}
}
