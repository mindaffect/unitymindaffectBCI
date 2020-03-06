using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

/**
 * TODO [] : finish the implementation and test
 */
public class SSDPDiscovery
{
  /// <summary>
  /// Device search request
  /// </summary>
  private const string searchRequest = "M-SEARCH * HTTP/1.1\r\nHOST: {0}:{1}\r\nMAN: \"ssdp:discover\"\r\nMX: {2}\r\nST: {3}\r\n";

  /// <summary>
  /// Advertisement multicast address
  /// </summary>
  private const string multicastIP = "239.255.255.250";

  /// <summary>
  /// Advertisement multicast port
  /// </summary>
  private const int multicastPort=1900;

    string serviceType;
    byte[] msearchMessage;
    UdpClient udpClient=null;
    float querytime=-1;

    public SSDPDiscovery(string serviceType,int timeout=5000){
	this.serviceType=serviceType;
	string request = string.Format(searchRequest, multicastIP, multicastPort,timeout,serviceType);
	this.msearchMessage = System.Text.Encoding.UTF8.GetBytes(request);
	// BODGE: make sure last query time was long ago...
	this.querytime=getAbsTime_ms()-100000;
    }
    
    public UdpClient initSocket(){
	var address = IPAddress.Parse(multicastIP);
	var ipEndPoint = new IPEndPoint(address, multicastPort);
	udpClient = new UdpClient(AddressFamily.InterNetwork);
	udpClient.JoinMulticastGroup(address);
	return udpClient;
    }
    
    public List<Dictionary<string,string>> discover(int timeout=1000)
    {
	var devices = new List<Dictionary<string,string>>();
	if ( udpClient == null ) {
	    initSocket();
	}
    
	// send the request
	if( querytime + timeout < getAbsTime_ms() ) {
	    Console.WriteLine("Sending query: ");      
	    IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse(multicastIP), multicastPort);
	    udpClient.Send(msearchMessage, msearchMessage.Length, sendEndPoint);
	    querytime = getAbsTime_ms();

	}
	// When the initial multicast is done, get ready to receive responses
	IPEndPoint recieveEndPoint = new IPEndPoint(IPAddress.Any, 0);
	Console.WriteLine("Awaiting responses:");
	udpClient.Client.ReceiveTimeout=timeout;
	var data = udpClient.Receive(ref recieveEndPoint);
	// Got a response, so decode it
	string result = Encoding.UTF8.GetString(data);
	Console.WriteLine("Got response: " + result.ToString());
	if (result.StartsWith("HTTP/1.1 200 OK", StringComparison.InvariantCultureIgnoreCase))
	    {
		//parse device
		Dictionary<string,string> resp=ParseSSDPResponse(result);
		// check for match
		if ( ( resp.ContainsKey("st") && resp["st"].Contains(serviceType) ) ||
		     (resp.ContainsKey("server") && resp["server"].Contains(serviceType)) ) {
		    // add to response list
		    devices.Add(resp);          
		}
            }
        else
	    {
		//Debug.WriteLine("INVALID SEARCH RESPONSE");
	    }
	return devices;
    }
  
  public static long getAbsTime_ms(){
      return (long)(Stopwatch.GetTimestamp()*1000.0/Stopwatch.Frequency);
  }
  

    public static List<Dictionary<string,string>> ssdpDiscover(string servicetype, int timeout){
	SSDPDiscovery dis=new SSDPDiscovery(servicetype);
	var tend=getAbsTime_ms()+timeout;
	int ttg =timeout;
	while ( ttg>0 ) {
	    var devices = dis.discover(ttg);
	    if( devices.Count>0 ) {
		// TODO: accumulate the devices lists
		return devices;
	    }
	    ttg=(int)(tend-getAbsTime_ms());
	}
	return null;
    }
  
  // Probably not exactly compliant with RFC 2616 but good enough for now
  private Dictionary<string, string> ParseSSDPResponse(string response)
    {
      StringReader reader = new StringReader(response);
      
      string line = reader.ReadLine();
      if (line != "HTTP/1.1 200 OK")
        return null;
      
      Dictionary<string, string> result = new Dictionary<string, string>();
      
      while ( true ) 
        {
          line = reader.ReadLine();
          if (line == null)
            break;
          if (line != "")
            {
              int colon = line.IndexOf(':');
              if (colon < 1)
                {
                  return null;
                }
              string name = line.Substring(0, colon).Trim();
              string value = line.Substring(colon + 1).Trim();
              if (string.IsNullOrEmpty(name))
                {
                  return null;
                }
              result[name.ToLowerInvariant()] = value;
            }
        }
      return result;
    }
  
  public static void Main(string[] argv) {
      SSDPDiscovery dis=new SSDPDiscovery("ssdp:all");
      while ( true ){
	  List<Dictionary<string,string>> devices=dis.discover();
	  foreach ( var dev in devices ) {
	      Console.WriteLine("Device");
	      foreach ( KeyValuePair<string,string> kvp in dev ) {
		  Console.WriteLine("{0}:{1}",kvp.Key,kvp.Value);
	      }
	  }
      }
  }
};

