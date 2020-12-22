namespace nl.ma.utopiaserver.messages
{
    /*
 * Copyright (c) MindAffect B.V. 2018
 * For internal use only.  Distribution prohibited.
 */

    /**
     * The NEWTARGET utopia message class
     */
    public class Selection : UtopiaMessage
    {
        public static int MSGID = ((int)'S');
        public static string MSGNAME = "SELECTION";
        /**
         * get the unique message ID for this message type
         */
        public int msgID() { return MSGID; }
        /**
         * get the unique message name, i.e. human readable name, for this message type
         */
        public string msgName() { return MSGNAME; }
        public int timeStamp;
        /**
         * get the time-stamp for this message 
         */
        public int getTimeStamp() { return this.timeStamp; }
        /**
         * set the time-stamp information for this message.
         */
        public void setTimeStamp(int ts) { this.timeStamp = ts; }
        /**
         * get the version of this message
         */
        public int getVersion() { return 0; }

        public int objID;
        public Selection(int timeStamp, int objID)
        {
            this.timeStamp = timeStamp;
            this.objID = objID;
        }

        /**
         * deserialize a byte-stream to create a NEWTARGET instance
         */
        public static Selection deserialize(ByteBuffer buffer, int version)
        {
            // get the timestamp
            int timeStamp = buffer.getInt();
            int objID = (int)buffer.get();
            return new Selection(timeStamp, objID);
        }
        public static Selection deserialize(ByteBuffer buffer)
        {
            return deserialize(buffer, 0);
        }
        /**
     * serialize this instance into a byte-stream in accordance with the message spec. 
     */
        public void serialize(ByteBuffer buf)
        {
            buf.putInt(timeStamp);
            buf.put((byte)objID);
        }

        public override string ToString()
        {
            return "t:" + msgName() + " ts:" + timeStamp + " id:" + objID;
        }
    }
}
