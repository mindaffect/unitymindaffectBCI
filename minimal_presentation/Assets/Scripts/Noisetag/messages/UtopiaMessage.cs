namespace nl.ma.utopiaserver.messages {
//  General interface for all utopia Messages -- minimum methods they must provide
//  Also location for general information about the message format
    public class Config {
        public static int UTOPIABYTEORDER=0;
    }
public interface UtopiaMessage {
    int msgID();

    string msgName();

    void serialize(ByteBuffer buf);

    // static UtopiaMessage deserialize(java.nio.ByteBuffer buf);
    //  time-stamp getter/setter
    int getTimeStamp();
    void setTimeStamp(int ts);
    int getVersion();
    //string ToString();
}
}
