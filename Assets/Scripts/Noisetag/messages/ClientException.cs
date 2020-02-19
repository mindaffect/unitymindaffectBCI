namespace nl.ma.utopiaserver {
/** 
 * Exception class for when a client message is somehow malformed or unparseable.
 */
public class ClientException : System.Exception {
   string message;
   public ClientException(string str) : base(str) {
        message = str;
        //super(str);
	}
   public string getMessage(){ return this.Message ; }
}
}
