using System.Collections;
using System.IO;

namespace nl.ma.utopiaserver {
	//Implements a minimal subset of Java's ByteBuffer using C# MemoryStream
	public class ByteBuffer {
		
       protected MemoryStream stream;
       protected BinaryWriter writer; // needed to convert obj->byte[]
       protected BinaryReader reader;  // needed to convert byte[]->obj
       
		//Constructors
		public ByteBuffer(){
			stream = new MemoryStream();
         // readers and writers, ensure they leave the stream open after finish
         writer = new BinaryWriter(stream,System.Text.Encoding.UTF8,true);
         reader = new BinaryReader(stream,System.Text.Encoding.UTF8,true);
		}
		
		public ByteBuffer(int capacity){
			stream = new MemoryStream(capacity);
			stream.Capacity = capacity;
         // readers and writers, ensure they leave the stream open after finish
         writer = new BinaryWriter(stream,System.Text.Encoding.UTF8,true);
         reader = new BinaryReader(stream,System.Text.Encoding.UTF8,true);
		}
		
       public void order(int ord) {
           // Placeholder for later switching of the order used..
       }
       
		//put functions
		public ByteBuffer put(byte[] src, int offset, int length){
			this.stream.Write(src, offset, length);
			return this;
		}

       public ByteBuffer put(ByteBuffer src){
           this.stream.Write(src.getStream().GetBuffer(),(int)src.position(),(int)src.remaining());
			return this;
		}
       
		public ByteBuffer put(byte[] src){
			this.stream.Write(src, 0, src.Length);
			return this;
		}

		public ByteBuffer put(byte src){
			this.stream.WriteByte(src);
			return this;
		}
       
		public ByteBuffer putByte(byte src){
			this.stream.WriteByte(src);
			return this;
		}
		
		public ByteBuffer putString(string src){
         this.writer.Write(src);
			return this;
		}
		
		public ByteBuffer putShort(short src){
          this.writer.Write(src);
			return this;
		}
		
		public ByteBuffer putInt(int src){
          this.writer.Write(src);
			return this;
		}
		
		public ByteBuffer putLong(long src){
          this.writer.Write(src);
			return this;
		}
		
		public ByteBuffer putDouble(double src){
			this.writer.Write(src);
			return this;
		}
		
		public ByteBuffer putFloat(float src){
			this.writer.Write(src);
			return this;
		}
	
		//get functions
		public ByteBuffer get(ref byte[] bytes){
          // TODO [] check the size of the read?
         stream.Read(bytes,0,(int)bytes.Length);
			return this;
		}
		
		public byte get(){
          return (byte)stream.ReadByte();
		}
		
		public short getShort(){
          return reader.ReadInt16();
		}
		
		public int getInt(){
			return reader.ReadInt32();
		}
		
		public long getLong(){
			return reader.ReadInt64();
		}
		
		public double getDouble(){
			return reader.ReadDouble();
		}
		
		public float getFloat(){
			return reader.ReadSingle();
		}
				
		//other functions
		public static ByteBuffer allocate(int capacity){
			return new ByteBuffer(capacity);
		}
				
		public long position(){
          return stream.Position;	
		}
		
		public void position(long newposition){
			stream.Position = newposition;
		}
				
		public long remaining(){
          return stream.Length - stream.Position;
		}
		
		public long capacity(){
          return stream.Capacity;
		}
		
		public long length(){
			return stream.Length;
		}
		
		public ByteBuffer rewind(){
			stream.Position = 0;
			return this;
		}	

      public ByteBuffer flip(){
         stream.SetLength(stream.Position);
         stream.Position=0;
         return this;
      }

		public ByteBuffer clear(){
			stream.SetLength(0);
			return this;
		}

      // get the raw byte array
      public byte[] array(){
             return this.stream.ToArray();
      }

       public MemoryStream getStream(){ return this.stream; }
       
	}
}
