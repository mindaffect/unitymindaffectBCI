namespace nl.ma.utopiaserver.messages{
    public class Reset : UtopiaMessage {

        public static int MSGID = ((int)('R'));

        public static string MSGNAME = "RESET";

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

        public Reset(int timeStamp) {
            this.timeStamp = timeStamp;
        }

        public static Reset deserialize(ByteBuffer buffer, int version) {
            //buffer.order(Constants.UTOPIABYTEORDER);
            //  get the timestamp
            int timeStamp = buffer.getInt();
            return new Reset(timeStamp);
        }

        public static Reset deserialize(ByteBuffer buffer) {
            return Reset.deserialize(buffer, 0);
        }

        public void serialize(ByteBuffer buf) {
            //buf.order(Constants.UTOPIABYTEORDER);
            buf.putInt(this.timeStamp);
        }

        public override string ToString() {
            string str = "t:" + this.msgName() + " ts:" + this.timeStamp;
            return str;
        }
    }
}
