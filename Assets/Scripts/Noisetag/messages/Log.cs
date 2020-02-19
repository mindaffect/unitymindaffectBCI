namespace nl.ma.utopiaserver.messages{
/*
 * Copyright (c) MindAffect B.V. 2018
 * For internal use only.  Distribution prohibited.
 */

/**
 * the MODECHANGE utopia message class, which has a time-stamp and a mode-string.
 */
public class Log : UtopiaMessage {
    public static  int MSGID         =(int)'L';
    public static  string MSGNAME    ="LOG";

    /**
     * get the unique message ID for this message type
     */
    public int msgID(){ return MSGID; }
    /**
     * get the unique message name, i.e. human readable name, for this message type
     */
    public string msgName(){ return MSGNAME; }
    public int timeStamp;
    /**
     * get the time-stamp for this message 
     */
    public int getTimeStamp(){return this.timeStamp;}
    /**
     * set the time-stamp information for this message.
     */
    public void setTimeStamp(int ts){ this.timeStamp=ts; }
    /**
     * get the version of this message
     */
    public int getVersion(){  return 0; }
    
    public string logmsg;

    public Log( int timeStamp, string logmsg){
        this.timeStamp=timeStamp;
        this.logmsg  =logmsg;
    }

    /**
     * deserialize a byte-stream to create an instance of this class 
     */ 
    public static Log deserialize( ByteBuffer buffer) {
        //buffer.order(UTOPIABYTEORDER);
        // get the timestamp
         int timeStamp = buffer.getInt();
        // get the new mode string -- N.B. assumed UTF-8 encoded        
         string logmsg = System.Text.Encoding.UTF8.GetString(buffer.array(),(int)buffer.position(),(int)buffer.remaining());
        return new Log(timeStamp,logmsg);
    }
    /**
     * serialize this instance into a byte-stream in accordance with the message spec. 
     */
    public void serialize( ByteBuffer buf) {
        //buf.order(Constants.UTOPIABYTEORDER);
        buf.putInt(timeStamp);
        // send the string
        buf.put(System.Text.Encoding.UTF8.GetBytes(logmsg));
    }    
	public override string ToString() {
		 string str= "t:" + msgName() + " ts:" + timeStamp + " msg:" + logmsg;
		 return str;
	}
}
}
