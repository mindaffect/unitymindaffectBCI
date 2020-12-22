namespace nl.ma.utopiaserver.messages {
public class StimulusEvent : UtopiaMessage {

    public static int MSGID = ((int)('E'));

    public static string MSGNAME = "STIMULUSEVENT";

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

    public int[] objIDs;

    public int[] objState;

    public StimulusEvent(int timeStamp, int objIDs, int objState) {
        this.timeStamp = timeStamp;
        this.objIDs = new int[1];
        this.objIDs[0] = objIDs;
        this.objState = new int[1];
        this.objState[0] = objState;
    }

    public StimulusEvent(int timeStamp, int[] objIDs, int[] objState) {
        this.timeStamp = timeStamp;
        int nObj = objIDs.Length;
        if ((objIDs.Length != objState.Length)) {
            System.Console.WriteLine("objIDs and objState have different Lengths");
        }

        if (nObj < objState.Length) {
            nObj = objState.Length;
        }

        this.objIDs = new int[nObj];
        System.Array.Copy(objIDs, 0, this.objIDs, 0, nObj);
        this.objState = new int[nObj];
        System.Array.Copy(objState, 0, this.objState, 0, nObj);
    }

    //  deserialize and create stimlus event
    public static StimulusEvent deserialize(ByteBuffer buffer) {
        //buffer.order(UTOPIABYTEORDER);
        //  get the timestamp
        int timeStamp = buffer.getInt();
        //  Get number of objects
        int nObjects = ((int)(buffer.get()));
        // Console.WriteLine("ts:"+timeStamp+" ["+nObjects+"]");
        int size = (nObjects * 2);
        //  Check if size and the number of bytes in the buffer match
        if ((buffer.remaining() < size)) {
            //  BODGE: allow for over-long message payloads...
            throw new ClientException("Defined size of data and actual size do not match.");
        }

        //  extract into 2 arrays, 1 for the objIDs and one for the state
        //  Transfer bytes from the buffer into a nSamples*nChans*nBytes array;
        int[] objIDs = new int[nObjects];
        int[] objState = new int[nObjects];
        for (int i = 0; (i < nObjects); i++) {
            objIDs[i] = ((int)(buffer.get()));
            objState[i] = ((int)(buffer.get()));
        }

        return new StimulusEvent(timeStamp, objIDs, objState);
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
        buf.put(((byte)(this.objIDs.Length)));
        for (int i = 0; (i < this.objIDs.Length); i++) {
            buf.put(((byte)(this.objIDs[i])));
            buf.put(((byte)(this.objState[i])));
        }

    }

    public override string ToString() {
        string str = "t:" + this.msgName() + " ts:" + this.timeStamp;
        str = str + " [" + this.objIDs.Length + "] ";
        for (int i = 0; i < this.objIDs.Length; i++) {
            str = str + "{" + this.objIDs[i] + "," + this.objState[i] + "}";
        }

        return str;
    }
}
}
