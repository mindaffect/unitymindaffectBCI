namespace nl.ma.utopiaserver {
    using nl.ma.utopiaserver.messages;

    using System;
    using System.Collections;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    //using SSDPDiscovery;

    // TODO[]: minimal ByteBuffer wrapper
    // TODO[]: convert to use MemoryStream directly so don't need ByteBuffer?
    // TODO[x]: add the heardbeat sender every 1000ms
    public class UtopiaClient {

        public static int VERBOSITY = 0;

        private TcpClient clientSocket;
        private UdpClient udpClient;
        private NetworkStream networkStream;
        private int HEARTBEATTIMEOUT = 1000;
        private long nextHeartbeatTime;
        private int HEARTBEATTIMEOUTUDP=250;
        private long nextHeartbeatTimeUDP;
        // Socket clientSocket;

        private static int MAXBUFFERSIZE = 1024*1024*16; // 16Mb is max message size

        ByteBuffer inbuffer;

        ByteBuffer msgbuffer;

        ByteBuffer tmpbuffer;

        TimeStampClock tsclock;
	private SSDPDiscovery ssdpDiscovery;

        public UtopiaClient() {
            this.tsclock = new TimeStampClock();
            this.msgbuffer = ByteBuffer.allocate(MAXBUFFERSIZE);
            this.tmpbuffer = ByteBuffer.allocate(MAXBUFFERSIZE);
            this.inbuffer = ByteBuffer.allocate(MAXBUFFERSIZE);
            this.nextHeartbeatTime = getTimeStamp();
            this.nextHeartbeatTimeUDP = getTimeStamp();
	        this.ssdpDiscovery = null;
            this.clientSocket = new TcpClient();
        }

	// TODO: make timeout work correctly
        public bool connect(string host, int port=-1, int timeout_ms=5000)
        {
	    if ( port<0 ) port=Constants.DEFAULTPORT;
            if (this.clientSocket != null && this.clientSocket.Connected)
		{
		    this.clientSocket.Close();
		    this.clientSocket = null;
		}
            if (host == null || host.Length == 0 || host == "-" )
		{
		    // auto-search for host port
		    Console.WriteLine("Trying SSDP discovery");
		    if( ssdpDiscovery==null ) {// create if needed
		    	ssdpDiscovery=new SSDPDiscovery("utopia/1.1");
		    }
		    var devices = ssdpDiscovery.discover(timeout_ms);
		    Console.WriteLine("Discovered " + devices.Count() + " devices");
		    if (devices.Any())
			{
			    var dev = devices[0];
			    if (dev.ContainsKey("location"))
				{
				    host = dev["location"];
				    port = Constants.DEFAULTPORT;
				    if (host.StartsWith("http://"))
					{
					    host = host.Substring("http://".Length);
					}
				    if (host.Contains("/"))
					{
					    host = host.Substring(0, host.IndexOf("/"));
					}
				    string[] hostport = host.Split(':');
				    host = hostport[0];
				    if (hostport.Length > 1)
					{
					    Int32.TryParse(hostport[1], out port);
					}
				}
			    Console.WriteLine("SSDP discovered: " + host + ":" + port);
			}
		}

            if (host == null || host.Length == 0) return false;
            if ( this.clientSocket == null ) this.clientSocket = new TcpClient();
            try
            {
                this.clientSocket.Connect(host, port);
            } catch ( SocketException ex)
            {

            }
            if (this.clientSocket.Connected)
            {
                this.networkStream = this.clientSocket.GetStream();
                this.clientSocket.ReceiveTimeout = 500;
                this.clientSocket.NoDelay = true; // disable Nagle?

                // udp client bound to same local port number to make matching udp/tcp messages easier
                this.udpClient = new UdpClient(((System.Net.IPEndPoint)this.clientSocket.Client.LocalEndPoint).Port);
                this.udpClient.Connect(host, port);
            }
            return this.clientSocket.Connected;
        
        }
	public bool autoconnect() { return connect(null,-1); }

	public bool isConnected() {
	    //  TODO [] : improved connection liveness detection..... try a non-blocking read
	    if ((clientSocket != null)) {
		return clientSocket.Connected;
	    } else {
		return false;
	    }
	}


	public string getHostPort(){
	    string hostport="";
	    if ( clientSocket != null ) {
		return getHost() + ":" + getPort();
	    }
	    return hostport;
	}
	public string getHost(){
	    if ( clientSocket != null ) {
		return  System.Net.IPAddress.Parse (((System.Net.IPEndPoint)this.clientSocket.Client.RemoteEndPoint).Address.ToString ()).ToString();
	    }
	    return null;
	}
	public int getPort(){
	    if ( clientSocket != null ) {
		return  ((System.Net.IPEndPoint)this.clientSocket.Client.RemoteEndPoint).Port;
	    }
	    return -1;
	}
	
	public int getTimeStamp() {
	    return ((int)TimeStampClock.getTime());
	}

	public void initClockAlign() {
	    initClockAlign(new int[] {100,100,100,100,100,100,100,100,100,100,});
	    //  1s for initial clock alignment
	}

	public void initClockAlign(int[] delays_ms) {
	    sendMessage(new Heartbeat(getTimeStamp()));
	    for (int i = 0; i < delays_ms.Length; i++) {
		//try {
		System.Threading.Thread.Sleep(delays_ms[i]);
		//}
		//catch (InterruptedException ex) {
		//    break;
		//}

		sendMessage(new Heartbeat(getTimeStamp()));
	    }

	}

	// run as a simple message logger
	public void run() {
	    Console.WriteLine("Waiting for messages");
	    while (true) {
		//  Blocking call!!!
		try {
		    List<UtopiaMessage> inmsgs=getNewMessages();
		    foreach (UtopiaMessage msg in inmsgs) {
			Console.WriteLine("Got Message: "  + msg.ToString() + " <- server");
			if( msg.msgID() == PredictedTargetProb.MSGID ) {
			    Console.WriteLine("PTP" + ((PredictedTargetProb)msg));
			}
		    }
		}
		catch (IOException ex) {
		    Console.WriteLine("Problem reading from stream");
		    Environment.Exit(-1);
		}

		Console.Write('.');
		System.Threading.Thread.Sleep(500);
		//        System.out.flush();
	    }

	}

	public List<UtopiaMessage> getNewMessages() {
	    List<UtopiaMessage> inmessageQueue = new List<UtopiaMessage>();
	    inbuffer.clear();
	    //  copy from the stream to the byte-buffer
	    //  TODO: [] check for stream close!
	    while( networkStream.DataAvailable && inbuffer.capacity()>inbuffer.position()) {
		    inbuffer.put((byte)networkStream.ReadByte());
	    }
	    inbuffer.flip();
	    if ( VERBOSITY >= 1) {
		    Console.WriteLine(inbuffer.remaining() + " to read in channel");
	    }

	    while ((inbuffer.remaining() > 0)) {
            RawMessage rawmessage = null;
		    try {
		        rawmessage = RawMessage.deserialize(inbuffer);
		    }
		    catch (ClientException ex) {
		        Console.WriteLine("Something wrong deserializing client message... skipped");
		        Console.WriteLine(ex.getMessage());
                break;
		    }
            try
            {
                UtopiaMessage msg = rawmessage.decodePayload();
                inmessageQueue.Add(msg);

            }
                catch ( ClientException ex)
                {
                    Console.WriteLine("Something wrong decoding client message... skipped");
                    Console.WriteLine(ex.getMessage());
                }
            }

	    if ((VERBOSITY >= 1)) {
		Console.WriteLine(("New message queue size: " + inmessageQueue.Count));
	    }

	    sendHeartbeatIfTimeout();

	    return inmessageQueue;
	}

	public void serializeMessage(UtopiaMessage msg)
        {
            //  serialize Event into tempory buffer (to get it's size)
            tmpbuffer.clear();
            msg.serialize(tmpbuffer);
            tmpbuffer.flip();
            // Console.WriteLine("Message Payload size" + tmpbuffer.remaining());
            //  serialize the full message with header into the msgbuffer
            msgbuffer.clear();
            RawMessage.serialize(msgbuffer, msg.msgID(), msg.getVersion(), tmpbuffer);
            msgbuffer.flip();
        }

        public void sendMessageUDP(UtopiaMessage msg) {
            if (udpClient == null) // fall back on TCP
            {
                sendMessage(msg);
                return;
            }
            //  add time-stamp information if not already there.
            if ((msg.getTimeStamp() < 0))
            {
                msg.setTimeStamp(getTimeStamp());
            }

            serializeMessage(msg);
            udpClient.Send(msgbuffer.array(), (int)msgbuffer.remaining());
        }

        public void sendMessage(UtopiaMessage msg)
        {
            //  add time-stamp information if not already there.
            if ((msg.getTimeStamp() < 0))
		{
		    msg.setTimeStamp(getTimeStamp());
		}

            serializeMessage(msg);
            networkStream.Write(msgbuffer.array(), 0, (int)msgbuffer.remaining());

            sendHeartbeatIfTimeout();
        }

        private void sendHeartbeatIfTimeout(){
            // send heartbeat message if sufficient time has passed
            long curtime = getTimeStamp();
            if( curtime > nextHeartbeatTime)
		{
		    nextHeartbeatTime = curtime + HEARTBEATTIMEOUT; // N.B. update first to avoid infinite recursion
		    sendMessage(new Heartbeat((int)curtime));
		    if ( VERBOSITY>0 ) Console.WriteLine("H");
		    //UnityEngine.Debug.Log("H");
		}
            if ( curtime > nextHeartbeatTimeUDP)
		{
		    nextHeartbeatTimeUDP = curtime + HEARTBEATTIMEOUTUDP; // N.B. update first to avoid infinite recursion
		    sendMessageUDP(new Heartbeat((int)curtime));
		    if ( VERBOSITY>0 ) Console.WriteLine("h");
		    //UnityEngine.Debug.Log("h");
		}
    
	}

	public static void Main(string[] argv) {
	    UtopiaClient utopiaClient = new UtopiaClient();
	    while (!utopiaClient.isConnected()) {
		try {
		    //utopiaClient.connect("localhost", Constants.DEFAULTPORT);
		    utopiaClient.autoconnect();
		}
		catch (System.Net.Sockets.SocketException ex) {
		    Console.WriteLine("Could not connect to server.  waiting");
		    System.Threading.Thread.Sleep(1000);
		}

	    }

	    //  initialize the time-lock
	    utopiaClient.initClockAlign();

	    // set the debug mode
	    if( argv.Length>0 ) {
		sendTestMessages(utopiaClient,0);
	    } else {
		utopiaClient.run();
	    }

	}


	private static void sendTestMessages(UtopiaClient utopiaClient, int offset){
	    //  write some test messages..
	    int[] objIDs = new int[10];
	    for (int i = 0; (i < objIDs.Length); i++) {
		objIDs[i] = i;
	    }

	    int[] objState = new int[objIDs.Length];
	    //  send some test StimulusEvents
	    for (int i = 0; (i < 5); i++) {
		for (int j = 0; (j < objState.Length); j++) {
		    objState[j] = (i + offset);
		}

		StimulusEvent e = new StimulusEvent(utopiaClient.getTimeStamp(), objIDs, objState);
		Console.WriteLine("Sending: "  + e.ToString() + " -> server");
		try {
		    utopiaClient.sendMessage(e);
		}
		catch (IOException ex) {
		    Console.WriteLine(ex);
		}

		System.Threading.Thread.Sleep(1000);
	    }

	    {
		//  PREDICTEDTARGETPROB
		PredictedTargetProb e = new PredictedTargetProb(utopiaClient.getTimeStamp(), 1, ((float)(0.99)));
		Console.WriteLine("Sending: "  + e.ToString() + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);
	    }
		
	    {
		//  PREDICTEDTARGETDIST
		PredictedTargetDist e = new PredictedTargetDist(utopiaClient.getTimeStamp(), new int[]{1,2,3}, new float[]{.1f,.2f,.3f});
		Console.WriteLine("Sending: "  + e.ToString() + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);
	    }
	    {
		//  HEARTBEAT V.2.0
		Heartbeat e = new Heartbeat(utopiaClient.getTimeStamp(), "TestMessage");
		Console.WriteLine("Sending: "  + e.ToString() + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);
	    }
            // LOG
	    {
		Log e = new Log(utopiaClient.getTimeStamp(),"ClientTest");
		Console.WriteLine("Sending: " + e.ToString()  + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);            
	    }
	    {
		//  MODECHANGE
		ModeChange e = new ModeChange(utopiaClient.getTimeStamp(), "ClientTest");
		Console.WriteLine("Sending: "  + e.ToString() + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);
	    }
	    {
		//  RESET
		Reset e = new Reset(utopiaClient.getTimeStamp());
		Console.WriteLine("Sending: " + e.ToString() + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);
	    }
	    {
		//  NEWTARGET
		NewTarget e = new NewTarget(utopiaClient.getTimeStamp());
		Console.WriteLine("Sending: " + e.ToString() + " -> server");
		utopiaClient.sendMessage(e);
		System.Threading.Thread.Sleep(1000);
	    }
	}
    }
}
