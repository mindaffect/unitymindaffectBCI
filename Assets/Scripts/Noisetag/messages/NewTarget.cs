namespace nl.ma.utopiaserver.messages {
public class NewTarget : UtopiaMessage {

    public static int MSGID = ((int)('N'));

    public static string MSGNAME = "NEWTARGET";

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

    public NewTarget(int timeStamp) {
        this.timeStamp = timeStamp;
    }

    //  deserialize and create stimlus event
    public static NewTarget deserialize(ByteBuffer buffer) {
        //buffer.order(UTOPIABYTEORDER);
        //  get the timestamp
        int timeStamp = buffer.getInt();
        return new NewTarget(timeStamp);
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
    }

    public override string ToString() {
        return "t:" + this.msgName() + " ts:" + this.timeStamp;
    }

    //  Field-trip buffer serialization
    public string getType() {
        return this.msgName();
    }

    public void getValue(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
    }
}
}