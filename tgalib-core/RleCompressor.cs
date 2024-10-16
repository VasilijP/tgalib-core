namespace tgalib_core;

//auxiliary class for RLE compression
public class RleCompressor(Stream stream)
{ 
	private const int buflen = 65536;
	private const int bufcritical = 65532;
	private RlePixel[] buf = new RlePixel[buflen];
	private int bufindex = 0;//index of first free place			

	private bool match(RlePixel arg1, RlePixel arg2)
	{
		if (arg1.c.Length != arg2.c.Length) return(false);
		for (int i = 0; i < arg1.c.Length; i++) if (arg1.c[i] != arg2.c[i]) return(false);
		return(true);
	}
	
	public void write(byte arg) { write([arg]); }
	
	//add arg to FIFO
	public void write(byte[] arg)
	{ 
		RlePixel argpix = new RlePixel(arg);
		if ((bufindex > 0)&&match(buf[bufindex-1],argpix)){
			if (buf[bufindex-1].rep < 128){
				buf[bufindex-1].rep++;
				return;
			}
		}
		buf[bufindex]=argpix;
		bufindex++;			
		if (bufindex > bufcritical) forceWrite();
	}
	
	//write one copy or run packet
	private void writePacket(int from, int to)
	{
		byte[] header = new byte[1];
		if (to - from  > 0) //create copy packet
		{
			header[0] = (byte)(to - from);//number of copied pixels - 1
		} else { //write run packet
			header[0] = (byte)(128 | (buf[from].rep-1));//number of repetition -1 and highest bit is 1				
		}
		try
		{
			stream.Write(header);
			for (int rpc = from; rpc <= to; rpc++) // this should run only once for run packet
			{
				stream.Write(buf[rpc].c);
			}
		} catch (IOException e) { Console.WriteLine(e); }
	}
	
	//forced write of entire buffer to output (end of image or full buffer)
	public void forceWrite()
	{	
		int from = 0;
		while (from < bufindex)
		{					
				int to = from;
				while ((to < bufindex-1)&&(buf[to].rep == 1)&&(to-from < 127)) to++;
				if ((to - from +1 > 1)&&(buf[to].rep > 1)) to--;
				writePacket(from, to);	
				from = to+1;								
				//writePacket(from, from);
				//from++;
		}
		bufindex = 0;//empty buffer
	}
}