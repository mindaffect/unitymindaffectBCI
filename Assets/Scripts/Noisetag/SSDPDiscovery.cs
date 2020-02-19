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

  static public List<Dictionary<string,string>> ssdpDiscover(string serviceType, int timeout)
  {
    var devices = new List<Dictionary<string,string>>();
    string request = string.Format(searchRequest, multicastIP, multicastPort,timeout,serviceType);

    // send the request
    using (var udpClient = new UdpClient(AddressFamily.InterNetwork))
    {
      var address = IPAddress.Parse(multicastIP);
      var ipEndPoint = new IPEndPoint(address, multicastPort);
      udpClient.JoinMulticastGroup(address);
      byte[] req = System.Text.Encoding.UTF8.GetBytes(request);
      Console.WriteLine("Sending query: " + request);      
      udpClient.Send(req, req.Length, ipEndPoint);
      
      // When the initial multicast is done, get ready to receive responses
      ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
      var t0=getAbsTime();
      Console.WriteLine("Awaiting responses:");
      while ( getAbsTime()-t0 < timeout ) {
        try {
          int timetogo = timeout - (int)(getAbsTime()-t0);
          udpClient.Client.ReceiveTimeout=timetogo;
          var data = udpClient.Receive(ref ipEndPoint);
          // Got a response, so decode it
          string result = Encoding.UTF8.GetString(data);
          Console.WriteLine("Got response: " + result.ToString());
          if (result.StartsWith("HTTP/1.1 200 OK", StringComparison.InvariantCultureIgnoreCase))
            {
              //parse device
              Dictionary<string,string> resp=ParseSSDPResponse(result);
              //Console.WriteLine("Parsed Response");
              //foreach ( KeyValuePair<string,string> kvp in resp ) {
              //  Console.WriteLine("{0}:{1}",kvp.Key,kvp.Value);
              //}
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
        } catch ( SocketException ) {
          break;
        }
      }
      udpClient.Close();
    }
    return devices;
  }
  
  public static long getAbsTime(){
    return Stopwatch.GetTimestamp()/Stopwatch.Frequency;
  }
  
  
  // Probably not exactly compliant with RFC 2616 but good enough for now
  private static Dictionary<string, string> ParseSSDPResponse(string response)
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
    var devices = SSDPDiscovery.ssdpDiscover("ssdp:all",10000);
    foreach ( var dev in devices ) {
      Console.WriteLine("Device");
      foreach ( KeyValuePair<string,string> kvp in dev ) {
        Console.WriteLine("{0}:{1}",kvp.Key,kvp.Value);
      }
    }
  }
};

