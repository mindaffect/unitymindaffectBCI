namespace nl.ma.utopiaserver.messages{
public class PredictedTargetProb : UtopiaMessage {

    public static int MSGID = ((int)('P'));

    public static string MSGNAME = "PREDICTEDTARGETPROB";

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
        return 0;
    }

    public int Yest;

    public float Perr;

    public PredictedTargetProb(int timeStamp, int Yest, float Perr) {
        this.timeStamp = timeStamp;
        this.Yest = Yest;
        this.Perr = Perr;
    }

    public static PredictedTargetProb deserialize(ByteBuffer buffer, int version) {
        //buffer.order(Constants.UTOPIABYTEORDER);
        //  get the timestamp
        int timeStamp = buffer.getInt();
        //  get the targetID
        int Yest = ((int)(buffer.get()));
        //  get the target prob
        float Perr = buffer.getFloat();
        return new PredictedTargetProb(timeStamp, Yest, Perr);
    }

    public static PredictedTargetProb deserialize(ByteBuffer buffer) {
        return PredictedTargetProb.deserialize(buffer, 0);
        //  default to version 0 messages
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(Constants.UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
        buf.put(((byte)(this.Yest)));
        buf.putFloat(this.Perr);
    }

    public override string ToString() {
        string str = "t:" + this.msgName() + " ts:" + this.timeStamp + " Yest:"
            + this.Yest + " Perr:" + this.Perr;
        return str;
    }
}}
