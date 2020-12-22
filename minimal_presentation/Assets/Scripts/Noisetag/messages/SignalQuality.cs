namespace nl.ma.utopiaserver.messages{
/*
 * Copyright (c) MindAffect B.V. 2018
 * For internal use only.  Distribution prohibited.
 */

/**
 * The SIGNALQUALITY utopia message class, which sends an [0-1] signal quality measure for each connected electrode.
 */
public class SignalQuality : UtopiaMessage {
    public static  int MSGID         =(int)'Q';
    public static  string MSGNAME    ="SIGNALQUALITY";
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
    public int getVersion(){return 0;}

    /**
     * the array of per-electrode signal qualities.
     */
    public float[] signalQuality;

    public SignalQuality( int timeStamp,  float signalQuality){
        this.timeStamp=timeStamp;
        this.signalQuality = new float[1];
        this.signalQuality[0]=signalQuality;
    }
    public SignalQuality( int timeStamp,  float[] signalQuality){
        this.timeStamp=timeStamp;
        int nObj=signalQuality.Length;
        this.signalQuality   =new float[nObj];
        System.Array.Copy(signalQuality,0,this.signalQuality,0,nObj);
    }

    /**
     * deserialize a byte-stream to create an instance of this class 
     */ 
    public static SignalQuality deserialize( ByteBuffer buffer, int version){

        //buffer.order(UTOPIABYTEORDER);
        // get the timestamp
         int timeStamp = buffer.getInt();
        // Get number of objects.  TODO[] : robustify this
         int nObjects = (int)(buffer.remaining()/4);//sizeof(float);
        // int nObjects = (int) buffer.get();
        //System.out.println("ts:"+timeStamp+" ["+nObjects+"]");        
        float [] signalQuality   = new float[nObjects];        
        for (int i = 0; i < nObjects; i++) {
            signalQuality[i]    = buffer.getFloat();
        }
        return new SignalQuality(timeStamp,signalQuality);
    }
    public static SignalQuality deserialize( ByteBuffer buffer){
        return deserialize(buffer,0);
    }

    /**
     * serialize this instance into a byte-stream in accordance with the message spec. 
     */
    public void serialize( ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(timeStamp);
        for( int i=0; i<signalQuality.Length; i++) {
            buf.putFloat(signalQuality[i]);
        }
    }
    
    public override string ToString() {
        string str= "t:" + msgName() + " ts:" + timeStamp ;
        str = str + " [" + signalQuality.Length + "] ";
        for ( int i=0; i<signalQuality.Length; i++){
            str = str + signalQuality[i] + ",";
        }
        return str;
	}
};
}
