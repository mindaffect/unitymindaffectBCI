using System;
using System.Collections.Generic;
using nl.ma.utopiaserver;
using nl.ma.utopiaserver.messages;

//namespace utopia2output {
    
// Example class for a utopia OUTPUT module.  Connects to the utopia server
//     and then either, depending on mode:
//         a)  listens for output which exceed it's probability threshold before
//     then printing them and using NEWTARGET to indicated the output has taken
//     place
//     
public class Utopia2Output {
        
    public UtopiaClient client;
    public delegate void SelectionDegelateType(int objID);
    public Dictionary<int,SelectionDegelateType> objectID2Action;
    public bool outputActivated;
    public float outputPressThreshold;
    public float outputReleaseThreshold;
    public int VERB;
        
    public Utopia2Output(float  outputPressThreshold = -1,
			 float outputReleaseThreshold = -1,
			 Dictionary<int,SelectionDegelateType> objectID2Action = null) {
	this.VERB = 0;
	this.outputPressThreshold = outputPressThreshold;
	if (outputReleaseThreshold > 0) {
	    this.outputReleaseThreshold = outputPressThreshold * 2;
	} else {
	    this.outputReleaseThreshold = outputReleaseThreshold;
	}
	// this dictionay contains the functions to execute for the
	// selection IDs we are responsible for. 
	this.objectID2Action = objectID2Action;
	this.client = new UtopiaClient();
    }
        
    public virtual void connect(string host = null,
				int port = -1,
				int timeout_ms = 30000) {
	Console.WriteLine("Connecting to utopia on"+ host+ ":" +port+ ","+timeout_ms);
	this.client.connect(host, port, timeout_ms);
	this.client.initClockAlign();
	if (this.outputPressThreshold <= 0) {
	    // selection mode => subscribe only to selection messages
	    this.client.sendMessage(new Subscribe(this.client.getTimeStamp(), "S"));
	    Console.WriteLine("In SelectionMode");
	} else {
	    // perr press/release mode => subscribe to probability output messages
	    this.client.sendMessage(new Subscribe(this.client.getTimeStamp(), "PS"));
	    Console.WriteLine("In PerrMode");
	}
    }
        
    // process a perr-message generating appropriate output.  
    //            To avoid 'key-bounce' we use a press-release semantics, where the output is 'Pressed' 
    //            so the output is generated when the Perr < outputPressThreshold, then further
    //            output is inhibited until Perr > outputReleaseThreshold.
    public virtual void perrModeOutput(List<UtopiaMessage> msgs) {
	foreach (UtopiaMessage m in msgs) {
	    if (!(m.msgID() == PredictedTargetProb.MSGID)) {
		continue;
	    }
	    PredictedTargetProb ptp = (PredictedTargetProb)m;
	    //print('OutputnMode:',ptp)
	    if (ptp.Perr < this.outputPressThreshold && !this.outputActivated) {
		// low enough error, not already activated
		this.client.sendMessage(new Selection(this.client.getTimeStamp(), ptp.Yest));
		this.outputActivated = true;
		this.doOutput(ptp.Yest);
	    } else if (ptp.Perr > this.outputReleaseThreshold && this.outputActivated) {
		// high-enough error, already activated
		this.outputActivated = false;
	    }
	}
    }
        
    //  Process selection message to generate output.  
    //             Basically generate output if the messages objectID is one of the ones
    //             we are tasked with generating output for
    public virtual void selectionModeOutput(List<UtopiaMessage> msgs) {
	foreach (UtopiaMessage msg in msgs) {
	    if (!(msg.msgID() == Selection.MSGID)) {
		continue;
	    }
	    Console.WriteLine("SelnMode:", msg);
	    this.doOutput(((Selection)msg).objID);
	}
    }
        
    // mainloop of utopia-to-output mapping
    //         runs an infinite loop, waiting for new messages from utopia, filtering out 
    //         those mesages which contain an output prediction (i.e. PREDICTEDTARGETPROB message)
    //         and if the output prediction is sufficiently confident forwarding this to the output
    //         device and sending a NEWTARGET to the recogniser to indicate the output was sent
    //         
    public virtual void run(int timeout_ms = 3000) {
	if (!this.client.isConnected()) {
	    this.connect();
	}
	Console.WriteLine("Waiting for messages");
	this.outputActivated = false;
	while (true) {
	    List<UtopiaMessage> newmsgs = this.client.getNewMessages();
	    if (this.outputPressThreshold>0) {
		// Perr output mode
		this.perrModeOutput(newmsgs);
	    } else if (this.outputPressThreshold <= 0) {
		// Perr output mode
		this.selectionModeOutput(newmsgs);
	    }
	    Console.Write(".");
	    System.Threading.Thread.Sleep(500); 
	}
    }
        
    // This function is run when objID has sufficiently low error to mean that 
    //         and output should be generated for this objID. 
    //         N.B. Override/Replace this function with your specific output method.
    public virtual void doOutput(int objID) {
	if (this.objectID2Action == null) {
	    Console.WriteLine(String.Format("Generated output for Target %d", objID));
	} else {
	    SelectionDegelateType action;
	    if( this.objectID2Action.TryGetValue(objID,out action) ){
		action.Invoke(objID);
	    }
	}
    }

    public static void main( params object[] args){
	string host = null;
	int port=-1;
	float outputthreshold = .1f;
	Utopia2Output u2o = new Utopia2Output(outputthreshold);
	u2o.connect(host, port);	    
	u2o.run();
    }
	
}
//}
