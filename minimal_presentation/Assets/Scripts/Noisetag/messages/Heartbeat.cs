namespace nl.ma.utopiaserver.messages {
public class Heartbeat : UtopiaMessage {

    public static int MSGID = ((int)('H'));

    public static string MSGNAME = "HEARTBEAT";

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

    public int getVersion() {
        if (this.statemessage == null) {
            return 0;
        }
        else {
            return 1;
        }

    }

    public string statemessage;

    public Heartbeat(int timeStamp, string statemessage) {
        this.timeStamp = timeStamp;
        this.statemessage = statemessage;
    }

    public Heartbeat(int timeStamp) {
        this.timeStamp = timeStamp;
        this.statemessage = null;
    }

    public static Heartbeat deserialize(ByteBuffer buffer, int version) {
        //buffer.order(UTOPIABYTEORDER);
        //  get the timestamp
        int timeStamp = buffer.getInt();
        string statemessage = null;
        if (version > 0) {
            //  decode the new mode info
            statemessage = System.Text.Encoding.UTF8.GetString(buffer.array(),(int)buffer.position(),(int)buffer.remaining());
        }
        return new Heartbeat(timeStamp, statemessage);
    }

    public static Heartbeat deserialize(ByteBuffer buffer) {
        return Heartbeat.deserialize(buffer, 0);
        //  default to version 0 messages
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
        if (this.statemessage != null) {
            //  include the statemessage
            System.Console.WriteLine("Putting message: " + this.statemessage);
            buf.put(System.Text.Encoding.UTF8.GetBytes(this.statemessage));
        }
    }

    public override string ToString() {
        string str = "t:" + this.msgName() + " ts:" + this.timeStamp;
        if (this.statemessage == null) {
            str = str + " v:" + "NULL";
        }
        else {
            str = str + " v:" + this.statemessage;
        }
        return str;
    }
}
}
