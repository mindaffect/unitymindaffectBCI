namespace nl.ma.utopiaserver.messages {
public class Subscribe : UtopiaMessage {

    public static int MSGID = ((int)('B'));

    public static string MSGNAME = "SUBSCRIBE";

    public int msgID() {
        return MSGID;
    }

    public string msgName() {
        return MSGNAME;
    }

    public int timeStamp;

    public int getTimeStamp() {
        return this.timeStamp;
    }

    public void setTimeStamp(int ts) {
        this.timeStamp = ts;
    }
    /**
     * get the version of this message
     */
    public int getVersion(){ return 0 ; }


    public string messageIDs;

    public Subscribe(int timeStamp, string messageIDs) {
        this.timeStamp = timeStamp;
        this.messageIDs = messageIDs;
    }

    //  deserialize and create stimlus event
    public static Subscribe deserialize(ByteBuffer buffer) {
        //buffer.order(UTOPIABYTEORDER);
        //  get the timestamp
        int timeStamp = buffer.getInt();
        //  get the new mode string -- N.B. assumed UTF-8 encoded
        string messageIDs = System.Text.Encoding.UTF8.GetString(buffer.array(),(int)buffer.position(),(int)buffer.remaining());
        return new Subscribe(timeStamp, messageIDs);
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
        //  send the string
        buf.put(System.Text.Encoding.UTF8.GetBytes(messageIDs));
    }

    public override string ToString() {
        string str = "t:" + this.msgName() + " ts:" + this.timeStamp + " messageIDs:" + this.messageIDs;
        return str;
    }
}
}
