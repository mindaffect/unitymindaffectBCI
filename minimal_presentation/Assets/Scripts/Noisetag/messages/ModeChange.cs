namespace nl.ma.utopiaserver.messages {
public class ModeChange : UtopiaMessage {

    public static int MSGID = ((int)('M'));

    public static string MSGNAME = "MODECHANGE";

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


    public string newmode;

    public ModeChange(int timeStamp, string newmode) {
        this.timeStamp = timeStamp;
        this.newmode = newmode;
    }

    //  deserialize and create stimlus event
    public static ModeChange deserialize(ByteBuffer buffer) {
        //buffer.order(UTOPIABYTEORDER);
        //  get the timestamp
        int timeStamp = buffer.getInt();
        //  get the new mode string -- N.B. assumed UTF-8 encoded
        string newmode = System.Text.Encoding.UTF8.GetString(buffer.array(),(int)buffer.position(),(int)buffer.remaining());
        return new ModeChange(timeStamp, newmode);
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
        //  send the string
        buf.put(System.Text.Encoding.UTF8.GetBytes(newmode));
    }

    public override string ToString() {
        string str = "t:" + this.msgName() + " ts:" + this.timeStamp + " mode:" + this.newmode;
        return str;
    }
}
}