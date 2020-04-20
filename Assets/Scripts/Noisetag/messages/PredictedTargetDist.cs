namespace nl.ma.utopiaserver.messages {
public class PredictedTargetDist : UtopiaMessage {

    public static int MSGID = ((int)('F'));

    public static string MSGNAME = "PredictedTargetDist";

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

    public float[] pTgt;

    public PredictedTargetDist
(int timeStamp, int objIDs, float pTgt) {
        this.timeStamp = timeStamp;
        this.objIDs = new int[1];
        this.objIDs[0] = objIDs;
        this.pTgt = new float[1];
        this.pTgt[0] = pTgt;
    }

    public PredictedTargetDist
(int timeStamp, int[] objIDs, float[] pTgt) {
        this.timeStamp = timeStamp;
        int nObj = objIDs.Length;
        if ((objIDs.Length != pTgt.Length)) {
            System.Console.WriteLine("objIDs and pTgt have different Lengths");
        }

        if (nObj < pTgt.Length) {
            nObj = pTgt.Length;
        }

        this.objIDs = new int[nObj];
        System.Array.Copy(objIDs, 0, this.objIDs, 0, nObj);
        this.pTgt = new float[nObj];
        System.Array.Copy(pTgt, 0, this.pTgt, 0, nObj);
    }

    //  deserialize and create stimlus event
    public static PredictedTargetDist
 deserialize(ByteBuffer buffer) {
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
        float[] pTgt = new float[nObjects];
        for (int i = 0; (i < nObjects); i++) {
            objIDs[i] = ((int)(buffer.get()));
            pTgt[i] = ((float)(buffer.getFloat()));
        }

        return new PredictedTargetDist
    
(timeStamp, objIDs, pTgt);
    }

    public void serialize(ByteBuffer buf) {
        //buf.order(UTOPIABYTEORDER);
        buf.putInt(this.timeStamp);
        buf.put(((byte)(this.objIDs.Length)));
        for (int i = 0; (i < this.objIDs.Length); i++) {
            buf.put(((byte)(this.objIDs[i])));
            buf.putFloat(this.pTgt[i]);
        }

    }

    public override string ToString() {
        string str = "t:" + this.msgName() + " ts:" + this.timeStamp;
        str = str + " [" + this.objIDs.Length + "] ";
        for (int i = 0; i < this.objIDs.Length; i++) {
            str = str + "{" + this.objIDs[i] + "," + this.pTgt[i] + "}";
        }

        return str;
    }
}
}
