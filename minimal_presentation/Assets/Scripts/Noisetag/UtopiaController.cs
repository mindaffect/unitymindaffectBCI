namespace nl.ma.utopia
{
    using System;
    using System.Collections;
    using System.Net.Sockets;
    using System.Collections.Generic;
    using nl.ma.utopiaserver;
    using nl.ma.utopiaserver.messages;

    // controller class to manage the interaction with the Mindaffect decoder,
    //     setting up the connection, sending and recieving messages, and firing message
    //     event handlers
    public class UtopiaController
    {

        public UtopiaClient client;

        // TODO[X] : convert to events!
        public List<UtopiaMessage> msgs;
        public delegate void MessageDegelateType(List<UtopiaMessage> msgs);
        MessageDegelateType messageHandlers = null;
        //public List<MessageDegelateType> messageHandlers;

        public PredictedTargetProb lastPrediction;
        public delegate void PredictionDegelateType(PredictedTargetProb ptp);
        PredictionDegelateType predictionHandlers = null;
        //public List<PredictionDegelateType> predictionHandlers;

        public delegate void SelectionDegelateType(int objID);
        SelectionDegelateType selectionHandlers = null;
        //public List<SelectionDegelateType> selectionHandlers;

        public SignalQuality lastSignalQuality;
        public delegate void SignalQualityDegelateType(float[] sigquality);
        SignalQualityDegelateType signalQualityHandlers = null;

        public delegate void NewTargetDegelateType();
        NewTargetDegelateType newTargetHandlers = null;

        public double selectionThreshold;

        public UtopiaController()
        {
            this.client = new UtopiaClient();
            this.msgs = new List<UtopiaMessage>();
            this.lastPrediction = null;
            // callback list for new messages
            //this.messageHandlers = new List<MessageDegelateType>();
            // call back list for new predictions
            //this.predictionHandlers = new List<PredictionDegelateType>();
            // selection stuff
            //this.selectionHandlers = new List<SelectionDegelateType>();
            this.selectionThreshold = 0.1;
        }


        public virtual void addMessageHandler(MessageDegelateType cb)
        {
            if (this.messageHandlers == null)
            {
                this.messageHandlers = cb;
            }
            else
            {
                this.messageHandlers += cb;
            }
        }


        public virtual void addPredictionHandler(PredictionDegelateType cb)
        {
            this.predictionHandlers += cb;
        }

        public virtual void addNewTargetHandler(NewTargetDegelateType cb)
        {
            this.newTargetHandlers += cb;
        }

        public virtual void addSelectionHandler(SelectionDegelateType cb)
        {
            this.selectionHandlers += cb;
        }


        public virtual void addSignalQualityHandler(SignalQualityDegelateType cb)
        {
            this.signalQualityHandlers += cb;
        }

        // get a (relative) wall-time stamp *in milliseconds*
        public virtual int getTimeStamp(int t0 = 0)
        {
            if (this.client != null)
            {
                return (int)(this.client.getTimeStamp() - t0);
            }
            return (int)(TimeStampClock.getTime() - t0);
        }

        public virtual bool autoconnect(string host = null,
                        int port = 8400,
                        int timeout_ms = 5000)
        {
            if (client == null) client = new UtopiaClient();
            if (!client.isConnected())
            {
                try
                {
                    client.connect(host, port, timeout_ms);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.WriteLine("Could not connect to server.  waiting");
                }
            }
            if (!this.client.isConnected())
            {
                Console.WriteLine("Warning:: couldnt connect to a utopia hub....");
                //this.client = null;
            }
            // subscribe to PREDICTEDTARGETPROB, MODECHANGE, SELECTION, ELECTRODEQUALITY and NEWTARGET messages only
            if (this.client != null && this.client.isConnected() )
            {
                subscribe("PMSNQ");
            }
            return isConnected();
        }

        public virtual bool isConnected()
        {
            if (this.client == null) return false;
            return this.client.isConnected();
        }
        public virtual string getHostPort()
        {
            if (this.client == null) return null;
            return this.client.getHostPort();
        }

        // Send a message to the Utopia-HUB informing of the current stimulus state
        public virtual StimulusEvent sendStimulusEvent(int[] stimulusState,
                          long timestamp = -1,
                          int targetState = -1,
                          int[] objIDs = null)
        {
            StimulusEvent stimEvent =
                this.mkStimulusEvent(stimulusState,
                         timestamp,
                         targetState,
                         objIDs);
            if (this.client != null)
            {
                this.client.sendMessage(stimEvent);
            }
            // erp injection for debugging with fakedata
            if (targetState == 0 || targetState == 1)
            {
                injectERP(targetState);
            }
            return stimEvent;
        }


        // make a valid stimulus event for the given stimulus state
        public virtual StimulusEvent mkStimulusEvent(int[] stimulusState,
                             long timestamp = -1,
                             int targetState = -1,
                             int[] objIDs = null)
        {
            if (timestamp == -1)
            {
                timestamp = this.getTimeStamp();
            }
            if (objIDs == null)
            {
                objIDs = new int[stimulusState.Length];
                for (int i = 0; i < objIDs.Length; i++)
                {
                    objIDs[i] = i + 1;
                }
            }
            else if (objIDs.Length != stimulusState.Length)
            {
                throw new System.ArgumentOutOfRangeException("ARGH! objIDs and stimulusState not same length!");
            }
            // insert extra 0 object ID if targetState given
            if (targetState >= 0)
            {
                int[] tgtobjIDs = new int[objIDs.Length + 1];
                objIDs.CopyTo(tgtobjIDs, 0);
                tgtobjIDs[tgtobjIDs.Length - 1] = 0;
                int[] tgtstimState = new int[stimulusState.Length + 1];
                stimulusState.CopyTo(tgtstimState, 0);
                tgtstimState[tgtstimState.Length - 1] = targetState;
                objIDs = tgtobjIDs;
                stimulusState = tgtstimState;
            }
            return new StimulusEvent((int)timestamp, objIDs, stimulusState);
        }

        public virtual void modeChange(string newmode)
        {
            if (this.client != null)
            {
                this.client.sendMessage(
                     new ModeChange(this.getTimeStamp(), newmode));
            }
        }

        public virtual void subscribe(string newmode)
        {
            if (this.client != null)
            {
                this.client.sendMessage(
                     new Subscribe(this.getTimeStamp(), newmode));
            }
        }

        public virtual void log(string msg)
        {
            if (this.client != null)
            {
                this.client.sendMessage(new Log(this.getTimeStamp(), msg));
            }
        }

        public virtual void newTarget()
        {
            if (this.client != null)
            {
                this.client.sendMessage(new NewTarget(this.getTimeStamp()));
            }
            if (newTargetHandlers != null)
            {
                this.newTargetHandlers.Invoke();
            }
        }

        public virtual void selection(int objID)
        {
            if (this.client != null)
            {
                this.client.sendMessage(
                  new Selection(this.getTimeStamp(), objID));
            }
            if (selectionHandlers != null)
            {
                this.selectionHandlers.Invoke(objID);
            }
            //foreach (SelectionDegelateType h in this.selectionHandlers) {
            //    h.Invoke(objID);
            //}
        }

        // get new messages from the utopia-hub, and store the list of new
        public List<UtopiaMessage> getNewMessages(int timeout_ms = 0)
        {
            if (this.client == null)
            {
                return null;
            }
            // get any messages with predictions
            this.msgs = this.client.getNewMessages();// timeout_ms);
            if (timeout_ms > 0)
            {
                Console.WriteLine("Warning: timeout not supported yet!!");
            }
            // process these messages as needed & call-callbacks
            if (this.msgs.Count > 0)
            {
                //foreach (MessageDegelateType h in this.messageHandlers) {
                //  h.Invoke(this.msgs);
                //}
                if (messageHandlers != null)
                {
                    this.messageHandlers.Invoke(this.msgs);
                }
                PredictedTargetProb newPrediction = null;
                foreach (UtopiaMessage m in this.msgs)
                {
                    if (m.msgID() == PredictedTargetProb.MSGID)
                    {
                        this.lastPrediction = (PredictedTargetProb)m;
                        // process new prediction callbacks
                        //foreach (PredictionDegelateType h in this.predictionHandlers) {
                        // h.Invoke(newPrediction);
                        //}
                        if (predictionHandlers != null)
                        {
                            this.predictionHandlers.Invoke(this.lastPrediction);
                        }
                    }
                    else if (m.msgID() == Selection.MSGID)
                    {
                        if (selectionHandlers != null)
                        {
                            this.selectionHandlers.Invoke(((Selection)m).objID);
                        }
                    }
                    else if (m.msgID() == SignalQuality.MSGID)
                    {
                        this.lastSignalQuality = (SignalQuality)m;
                        if (signalQualityHandlers != null)
                        {
                            this.signalQualityHandlers.Invoke(this.lastSignalQuality.signalQuality);
                        }
                    }
                    else if (m.msgID() == NewTarget.MSGID)
                    {
                        if (newTargetHandlers != null)
                        {
                            this.newTargetHandlers.Invoke();
                        }
                    }

                }
            }
            return this.msgs;
        }

        // check for new predictions from the utopia-decoder
        public PredictedTargetProb getLastPrediction()
        {
            // Q: should we do this here? or just return the lastPrediction?
            this.getNewMessages();
            // always return the last prediction, even if no new ones
            return this.lastPrediction;
        }

        // clear the last predicted target
        public void clearLastPrediction()
        {
            this.lastPrediction = null;
        }

        // check if any object prediction is high enough for it to be selected
        public Tuple<int, bool> getLastSelection()
        {
            this.getNewMessages();
            if (this.lastPrediction != null)
            {
                if (this.lastPrediction.Perr < this.selectionThreshold)
                {
                    // good enough to select?
                    return new Tuple<int, bool>(this.lastPrediction.Yest, true);
                }
                else
                {
                    // return predictedObjID but not-selected
                    return new Tuple<int, bool>(this.lastPrediction.Yest, false);
                }
            }
            return new Tuple<int, bool>(-1, false);
        }

        public float[] getLastSignalQuality()
        {
            this.getNewMessages();
            if (this.lastSignalQuality != null)
            {
                return this.lastSignalQuality.signalQuality;
            }
            return null;
        }

        // Inject an erp into a simulated data-stream, sliently ignore if failed, e.g. because not simulated
        public static void injectERP(int amp = 1,
                     string host = "localhost",
                     int port = 8300)
        {
            try
            {
                UdpClient triggerClient = new UdpClient();
                triggerClient.Connect(host, port);
                triggerClient.Send(new byte[] { (byte)amp }, 1);
            }
            catch (SocketException)
            {
                // ignore if no trigger..
            }
        }
    }
}
