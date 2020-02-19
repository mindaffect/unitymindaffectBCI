namespace nl.ma.utopia
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using nl.ma.utopiaserver;
    using nl.ma.utopiaserver.messages;
    using nl.ma.utopia;

    public class StimulusState
    {
        public int[] stimulusState;
        public int[] objIDs;
        public int targetState;
        public bool sendEvents;
        public StimulusState(int nobj)
        {
            int[] oi = new int[nobj]; for (int i = 0; i < oi.Length; i++) oi[i] = i + 1;
            int[] ss = new int[nobj];
            set(ss, oi, -1, false);
        }
        public StimulusState(int[] ss, int[] oi, int ts, bool se)
        {
            this.set(ss, oi, ts, se);
        }
        public StimulusState set(int[] ss, int[] oi, int ts, bool se)
        {
            // Q: deep copy?
            this.stimulusState = ss;
            this.objIDs = oi;
            this.targetState = ts;
            this.sendEvents = se;
            if (this.stimulusState == null && oi != null)
            { // make stim-state array if not given
                this.stimulusState = new int[oi.Length];
            }
            return this;
        }
        public override string ToString()
        {
            string str = "{";
            if (objIDs == null)
            {
                str += " null:null";
            }
            else
            {
                for (int i = 0; i < objIDs.Length; i++)
                {
                    str += objIDs[i] + ":";
                    if (stimulusState != null) str += stimulusState[i] + " ";
                    else str += "null ";
                }
            }
            str += " tgt:" + targetState + " se:" + sendEvents + "}";
            return str;
        }
    }


    //  simple finite state machine, using a generator-like pattern
    public class FSM
    {
        // update the current state, return the new state, raise StopIteration exception when done
        public virtual bool next(float t)
        {
            return false;
        }

        // get the current state
        public virtual StimulusState get()
        {
            return new StimulusState(null, null, -1, false);
        }
    }

    // Generalized state machine with stack of states
    public class GSM : FSM
    {

        public Stack<FSM> stack;
        public GSM()
        {
            this.stack = new Stack<FSM>();
        }

        public virtual void clear()
        {
            if (this.stack.Count > 0) this.stack.Clear();
            else this.stack = new Stack<FSM>();
        }
        public virtual GSM push(FSM s)
        {
            this.stack.Push(s);
            return this;
        }

        public virtual FSM pop()
        {
            return this.stack.Pop();
        }
        public FSM peek()
        {
            return this.stack.Peek();
        }

        // get the next stimulus state to shown
        public override bool next(float t)
        {
            while (this.stack.Count > 0)
            {
                bool hasNext = this.peek().next(t);
                if (hasNext)
                {
                    return true;
                }
                else
                {
                    // end of this fsm, unwind the fsm stack
                    this.pop();
                    // for pretty printing
                    Console.WriteLine();
                }
            }
            return false;
        }

        public override StimulusState get()
        {
            if (this.stack.Count > 0)
            {
                return this.peek().get();
            }
            return null;
        }
    }

    // wait for given number of frames to pass
    public class WaitFor
        : FSM
    {

        public int nframe;

        public int numframes;

        public WaitFor(int numframes)
        {
            this.numframes = numframes;
            this.nframe = 0;
            Console.WriteLine("waitFor:" + this.numframes);
        }

        public override bool next(float t)
        {
            this.nframe = this.nframe + 1;
            if (this.nframe > this.numframes)
            {
                return false;
            }
            return true;
        }

        public override StimulusState get()
        {
            return new StimulusState(null, null, -1, false);
        }
    }

    //  do a normal flicker sequence
    public class Flicker
    : FSM
    {

        public int nframe;
        public int numframes;
        public bool sendEvents;
        public StimulusState ss;
        public int[][] stimSeq;
        public int tgtidx;

        public Flicker(
            int[][] stimSeq,
            int numframes = 4 * 60,
            int tgtidx = -1,
            bool sendEvents = true)
        {
            init_ss(stimSeq);
            this.numframes = numframes;
            this.nframe = 0;
            this.tgtidx = tgtidx;
            this.sendEvents = sendEvents;
            //Console.WriteLine("Flicker:"+this.ss);
            //update_ss();
            Console.WriteLine(String.Format("flicker: {0:d} frames, tgt {0:d}", this.numframes, tgtidx));
        }

        public void init_ss(int[][] stimSeq)
        {
            this.stimSeq = stimSeq;
            int[] objIDs = null;
            // BODGE: assume objIDs start from 1?
            if (this.stimSeq != null)
            {
                objIDs = new int[this.stimSeq[0].Length];
                for (int i = 0; i < objIDs.Length; i++) objIDs[i] = i + 1;
            }
            this.ss = new StimulusState(null, objIDs, tgtidx, sendEvents);
        }

        private void update_ss()
        {
            // get index into stimulus-sequencew, with wraparound
            int ssidx = this.nframe % this.stimSeq.Length;
            // set the stimulus state
            int[] curss = this.stimSeq[ssidx];
            for (int i = 0; i < this.ss.stimulusState.Length; i++) this.ss.stimulusState[i] = curss[i];
            if (this.tgtidx >= 0)
            {
                this.ss.targetState = curss[this.tgtidx];
            }
            this.ss.sendEvents = this.sendEvents;
        }

        public override bool next(float t)
        {
            this.nframe = this.nframe + 1;
            if (this.nframe > this.numframes)
            {
                return false;
            }
            // extract the current frames stimulus state
            update_ss();
            return true;
        }

        public override StimulusState get()
        {
            return this.ss;
        }
    }

    //  do a normal flicker sequence, with early stopping selection
    public class FlickerWithSelection
        : Flicker
    {

        public UtopiaController utopiaController;

        public FlickerWithSelection(
              int[][] stimSeq,
              int numframes = 4 * 60,
              int tgtidx = -1,
              UtopiaController utopiaController = null,
              bool sendEvents = true)
            : base(stimSeq, numframes, tgtidx, sendEvents)
        {
            this.utopiaController = utopiaController;
            Console.WriteLine(" with selection");
        }

        public override bool next(float t)
        {
            bool hasNext = base.next(t);
            // check for selection and stop if found
            Tuple<int, bool> lastseln = this.utopiaController.getLastSelection();
            int objId = lastseln.Item1;
            bool selected = lastseln.Item2;
            if (selected)
            {
                if (this.sendEvents)
                {
                    // send event to say selection has occured
                    this.utopiaController.selection(objId);
                }
                hasNext = false;
            }
            return hasNext;
        }
    }

    // Highlight a single object for a number of frames
    public class HighlightObject
        : Flicker
    {
        public static int MAXOBJID = 128;
        public HighlightObject(
              int numframes = 4 * 60,
              int tgtidx = -1,
              int tgtstate = 2,
              bool sendEvents = true,
              int numblinkframes = 60 / 2)
                : base(null, numframes, tgtidx, sendEvents)
        {
            // make the stimulus sequence
            int[][] stimSeq = null;
            if (numblinkframes > 0 && tgtidx >= 0)
            {
                stimSeq = mkBlinkingSequence(MAXOBJID, numblinkframes, tgtidx, tgtstate);
            }
            else
            {
                stimSeq = new int[1][];
                stimSeq[0] = new int[MAXOBJID];
                if (tgtidx >= 0)
                {
                    stimSeq[0][tgtidx] = tgtstate;
                }
            }
            // re-set the Flicker state
            init_ss(stimSeq);
            Console.WriteLine(String.Format("highlight: tgtidx={0:d} nframes={1:d}", tgtidx, numframes));
        }


        public int[][] mkBlinkingSequence(int nobj, int numframes, int tgtidx, int tgtState = 2)
        {
            int[][] blinkSeq = new int[numframes][];
            for (int t = 0; t < blinkSeq.Length; t++)
            {
                blinkSeq[t] = new int[nobj];
                for (int o = 0; o < blinkSeq[t].Length; o++)
                {
                    blinkSeq[t][o] = 0;
                    // only 1st half is on...
                    if (t < numframes / 2)
                    {
                        if (o == tgtidx) blinkSeq[t][o] = tgtState;
                    }
                }
            }
            return blinkSeq;
        }

    }

    //  do a complete single trial with: cue->wait->flicker->feedback 
    public class SingleTrial
        : FSM
    {
        public int cueframes;
        public int feedbackframes;
        public int numframes;
        public int[] objIDs;
        public float selectionThreshold;
        public int stage;
        public int[][] stimSeq;
        public GSM stimulusStateStack;
        public int tgtidx;
        public UtopiaController utopiaController;
        public int waitframes;

        public SingleTrial(
            int[][] stimSeq,
            int tgtidx,
            UtopiaController utopiaController,
            GSM stimulusStateStack,
        params object[] args)
        {
            //Console.WriteLine("SingleTrial: args "+args.Length); foreach ( object arg in args ) Console.WriteLine(arg);
            // parse / default the variable length args portion
            float selectionThreshold = -1;
            int numframes = 400;
            int duration = 4;
            int cueduration = 1;
            int feedbackduration = 1;
            int waitduration = 1;
            int cueframes = -1;
            int feedbackframes = -1;
            int waitframes = -1;
            try
            {
                if (args.Length > 0) selectionThreshold = (float)args[0];
                if (args.Length > 1) numframes = (int)args[1];
                if (args.Length > 2) duration = (int)args[2];
                if (args.Length > 3) cueduration = (int)args[3];
                if (args.Length > 4) feedbackduration = (int)args[4];
                if (args.Length > 5) waitduration = (int)args[5];
                if (args.Length > 6) cueframes = (int)args[6];
                if (args.Length > 7) feedbackframes = (int)args[7];
                if (args.Length > 8) waitframes = (int)args[8];
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Invalid argument format...");
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Invalid argument format...");
            }
            init(stimSeq, tgtidx,
         utopiaController, stimulusStateStack,
         selectionThreshold, numframes,
         duration, cueduration, feedbackduration, waitduration,
         cueframes, feedbackframes, waitframes);
        }
        public SingleTrial(
            int[][] stimSeq,
            int tgtidx,
            UtopiaController utopiaController,
            GSM stimulusStateStack,
            float selectionThreshold = -1,
            int numframes = -1,
            int duration = 4,
            int cueduration = 1,
            int feedbackduration = 1,
            int waitduration = 1,
            int cueframes = -1,
            int feedbackframes = -1,
            int waitframes = -1)
        {
            init(stimSeq, tgtidx,
             utopiaController, stimulusStateStack,
             selectionThreshold, numframes,
             duration, cueduration, feedbackduration, waitduration,
             cueframes, feedbackframes, waitframes);
        }
        private void init(
                int[][] stimSeq,
                int tgtidx,
                UtopiaController utopiaController,
                GSM stimulusStateStack,
                float selectionThreshold = -1,
                int numframes = -1,
                int duration = 4,
                int cueduration = 1,
                int feedbackduration = 1,
                int waitduration = 1,
                int cueframes = -1,
                int feedbackframes = -1,
                int waitframes = -1)
        {
            this.tgtidx = tgtidx;
            this.stimSeq = stimSeq;
            this.utopiaController = utopiaController;
            this.stimulusStateStack = stimulusStateStack;
            this.numframes = numframes > 0 ? numframes : duration * 60;
            this.cueframes = cueframes > 0 ? cueframes : cueduration * 60;
            this.feedbackframes = feedbackframes > 0 ? feedbackframes : feedbackduration * 60;
            this.waitframes = waitframes > 0 ? waitframes : waitduration * 60;
            this.selectionThreshold = selectionThreshold;
            this.stage = 0;
            Console.WriteLine(String.Format("Cal: tgtidx={0:d}", this.tgtidx));
        }

        public override bool next(float t)
        {
            bool hasNext = true;
            if (this.stage == 0)
            {
                // trial-start + cue
                // tell decoder to start trial
                this.utopiaController.newTarget();
                // tell decoder to clear predictions if needed
                if (this.selectionThreshold > 0)
                {
                    this.utopiaController.selectionThreshold = this.selectionThreshold;
                    this.utopiaController.getNewMessages();
                    this.utopiaController.clearLastPrediction();
                }
                Console.WriteLine("0.cue");
                if (this.tgtidx >= 0)
                {
                    // only if target is set
                    this.stimulusStateStack.push(
                        new HighlightObject(
                            this.cueframes,
                            this.tgtidx,
                            2,
                            false)
                         );
                }
                else
                {
                    // skip cue+wait
                    this.stage = 1;
                }

            }
            else if (this.stage == 1)
            {
                // wait
                Console.WriteLine("1.wait");
                this.stimulusStateStack.push(
                        new HighlightObject(
                            this.waitframes,
                            -1,
                            -1,
                            false));

            }
            else if (this.stage == 2)
            {
                // stim
                Console.WriteLine(String.Format("2.stim, tgt:{0:d}", this.tgtidx));
                if (this.selectionThreshold > 0)
                {
                    // early stop if thres set
                    this.stimulusStateStack.push(
               new FlickerWithSelection(
                            this.stimSeq,
                            this.numframes,
                            this.tgtidx,
                            this.utopiaController,
                            true));
                }
                else
                {
                    // no selection based stopping
                    this.stimulusStateStack.push(
                new Flicker(
                    this.stimSeq,
                    this.numframes,
                    this.tgtidx,
                    true));
                }

            }
            else if (this.stage == 3)
            {
                // wait/feedback
                if (this.selectionThreshold >= 0)
                {
                    Console.WriteLine("3.feedback");
                    Tuple<int, bool> lastseln = this.utopiaController.getLastSelection();
                    int predObjId = lastseln.Item1;
                    bool selected = lastseln.Item2;
                    Console.WriteLine(String.Format(" pred:{0:d} sel:{1:d}", predObjId, selected));
                    if (selected)
                    {
                        // convert from objIDs to object index
                        int tgtidx = predObjId - 1;
                        //int tgtidx = Array.IndexOf(objIDs,predObjId);
                        this.stimulusStateStack.push(
                             new HighlightObject(
                                         this.feedbackframes,
                                         tgtidx,
                                         3,
                                         false,
                                         0)); // solid for feedback, i.e. no blink
                    }
                }
                else
                {
                    Console.WriteLine("3.wait");
                    this.stimulusStateStack.push(
                        new HighlightObject(
                                this.waitframes,
                                -1,
                            -1,
                            false));
                }
            }
            else
            {
                hasNext = false;
            }
            this.stage = this.stage + 1;
            return hasNext;
        }
    }

    //  do a complete calibration phase with nTrials x CalibrationTrial 
    public class CalibrationPhase : FSM
    {

        public object[] args;
        public bool isRunning;
        public int nTrials;
        public int[] objIDs;
        public int[][] stimSeq;
        public GSM stimulusStateStack;
        public int tgtidx;
        public int trli;
        public UtopiaController utopiaController;
        public Random rng;

        public CalibrationPhase(
            int[] objIDs,
            int[][] stimSeq,
        int nTrials = 10,
            UtopiaController utopiaController = null,
            GSM stimulusStateStack = null,
            params object[] args)
        {
            if (objIDs == null) throw new ArgumentNullException("objIDs cannot be null");
            this.objIDs = objIDs;
            this.stimSeq = stimSeq;
            this.nTrials = nTrials;
            this.utopiaController = utopiaController;
            this.stimulusStateStack = stimulusStateStack;
            this.isRunning = false;
            this.args = args;
            this.trli = 0;
            rng = new Random();
        }

        public override bool next(float t)
        {
            bool hasNext = true;
            if (!this.isRunning)
            {
                // tell decoder to start cal
                this.utopiaController.modeChange("Calibration.supervised");
                this.isRunning = true;
            }
            if (this.trli < this.nTrials)
            {
                this.tgtidx = rng.Next(this.objIDs.Length);
                Console.WriteLine(String.Format("Start Cal: {0:d}/{1:d} tgtidx={2:d}", this.trli, this.nTrials, this.tgtidx));
                //UnityEngine.Debug.Log(String.Format("Start Cal: {0:d}/{1:d} tgtidx={2:d}", this.trli, this.nTrials, this.tgtidx));
                this.stimulusStateStack.push(
            new SingleTrial(
                    this.stimSeq,
                    this.tgtidx,
                    this.utopiaController,
                    this.stimulusStateStack,
                    this.args)
                         );
            }
            else
            {
                this.utopiaController.modeChange("idle");
                hasNext = false;
            }
            this.trli = this.trli + 1;
            return hasNext;
        }
    }

    // do complete prediction phase with nTrials x trials with early-stopping feedback
    public class PredictionPhase
        : FSM
    {

        public object[] args;
        public bool isRunning;
        public int nTrials;
        public int[] objIDs;
        public float selectionThreshold;
        public int[][] stimSeq;
        public GSM stimulusStateStack;
        public int tgti;
        public UtopiaController utopiaController;

        public PredictionPhase(int[] objIDs,
                   int[][] stimSeq,
                   int nTrials = 10,
                   UtopiaController utopiaController = null,
                   GSM stimulusStateStack = null,
                   params object[] args)
        {
            this.objIDs = objIDs;
            this.stimSeq = stimSeq;
            this.nTrials = nTrials;
            this.utopiaController = utopiaController;
            this.stimulusStateStack = stimulusStateStack;
            this.args = args;

            this.isRunning = false;
            this.tgti = 0;
        }

        public override bool next(float t)
        {
            bool hasNext = true;
            if (!this.isRunning)
            {
                this.utopiaController.modeChange("Prediction.static");
                this.isRunning = true;
            }
            this.tgti = this.tgti + 1;
            if (this.tgti < this.nTrials)
            {
                Console.WriteLine(String.Format("Start Pred: {0:d}/{1:d}", this.tgti, this.nTrials));
                this.stimulusStateStack.push(
                  new SingleTrial(
                      this.stimSeq,
                      -1,
                      this.utopiaController,
                      this.stimulusStateStack,
                      this.args)
                         );
            }
            else
            {
                this.utopiaController.modeChange("idle");
                hasNext = false;
            }
            return hasNext;
        }
    }

    // do a complete experiment, with calibration -> prediction
    public class Experiment
        : FSM
    {

        public object[] calargs;
        public object[] predargs;
        public int nCal;
        public int nPred;
        public int[] objIDs;
        public float selectionThreshold;
        public int stage;
        public int[][] stimSeq;
        public GSM stimulusStateStack;
        public UtopiaController utopiaController;

        public Experiment(
            int[] objIDs,
            int[][] stimSeq,
        int nCal = 10,
        int nPred = 10,
        float selectionThreshold = 0.1f,
            UtopiaController utopiaController = null,
            GSM stimulusStateStack = null,
            params object[] args)
        {
            if (objIDs == null) throw new ArgumentNullException("objIDs cannot be null");
            this.objIDs = objIDs;
            this.stimSeq = stimSeq;
            this.nCal = nCal;
            this.nPred = nPred;
            this.selectionThreshold = selectionThreshold;
            this.utopiaController = utopiaController;
            this.stimulusStateStack = stimulusStateStack;

            // add the selection threshold (off for cal) to the arguments stack
            this.calargs = new object[args.Length + 1];
            this.predargs = new object[args.Length + 1];
            for (int i = 0; i < args.Length; i++)
            {
                this.predargs[i + 1] = args[i];
                this.calargs[i + 1] = args[i];
            }
            this.calargs[0] = -1.0f; // no selection during calibration
            this.predargs[0] = this.selectionThreshold;  // selection during prediction
            this.stage = 0;
        }

        public override bool next(float t)
        {
            bool hasNext = true;
            if (this.stage == 0)
            {
                this.stimulusStateStack.push(new WaitFor(2 * 60));

            }
            else if (this.stage == 1)
            {
                this.stimulusStateStack.push(
                    new CalibrationPhase(this.objIDs,
                           this.stimSeq,
                           this.nCal,
                           this.utopiaController,
                           this.stimulusStateStack,
                           this.calargs));

            }
            else if (this.stage == 2)
            {
                this.stimulusStateStack.push(new WaitFor(2 * 60));

            }
            else if (this.stage == 3)
            {
                this.stimulusStateStack.push(
                    new PredictionPhase(this.objIDs,
                           this.stimSeq,
                           this.nPred,
                           this.utopiaController,
                           this.stimulusStateStack,
                           this.predargs));
            }
            else
            {
                hasNext = false;
            }
            this.stage = this.stage + 1;
            return hasNext;
        }
    }


    // noisetag abstraction layer to handle *both* the sequencing of the stimulus
    //     flicker, *and* the communications with the Mindaffect decoder.  Clients can
    //     use this class to implement BCI control by:
    //      0) setting the flicker sequence to use (method: startFlicker, startFlickerWithSelection, startCalibration, startPrediction
    //      1) getting the current stimulus state (method: getStimulusState), and using that to draw the display
    //      2) telling Noisetag when *exactly* the stimulus update took place (method: sendStimulusState)
    //      3) getting the predictions/selections from noisetag and acting on them. (method: getLastPrediction() or getLastSelection())
    //      

    //public static UtopiaController uc = null;
    //public static int[] objIDs = Enumerable.Range(1, 10 - 1).ToArray();
    //public static int isi = 1 / 60;

    public class Noisetag
    {

        public StimulusState lastrawstate;
        public StimulusState laststate;
        public StimSeq noisecode;
        public int[][] stimSeq = null;
        public int[] objIDs = null;
        public GSM stimulusStateMachineStack = null;
        public UtopiaController utopiaController = null;

        public Noisetag() : this(new System.IO.StreamReader("mgold_61_6521_psk_60hz.txt"), null, null) { }
        public Noisetag(System.IO.TextReader stimFile, UtopiaController utopiaController, GSM stimulusSequenceStack)
        {
            // global flicker stimulus sequence
            this.noisecode = StimSeq.FromString(stimFile);
            //Console.WriteLine(this.noisecode);
            //this.noisecode.convertstimSeq2int();
            //this.noisecode.setStimRate(2);
            // get the stimSeq (as int) from the noisecode
            this.stimSeq = this.convertstimSeq2int(this.noisecode);
            // utopiaController
            this.utopiaController = utopiaController;
            if (this.utopiaController == null)
            {
                // TODO [] : make a singlention for the utopia controller?
                this.utopiaController = new UtopiaController();
            }
            // stimulus state-machine stack
            // Individual stimulus-state-machines track progress in a single
            // stimulus state playback function.
            // Stack allows sequencing of sets of playback functions in loops
            if (stimulusStateMachineStack == null)
            {
                stimulusStateMachineStack = new GSM();
            }
            this.laststate = new StimulusState(null, null, -1, false);
            this.lastrawstate = new StimulusState(null, null, -1, false);
            this.objIDs = null;
        }

        public bool connect(string host = null, int port = -1, int timeout_ms = 1000)
        {
            if (this.utopiaController.isConnected()) return true;
            this.utopiaController.autoconnect(host, port, timeout_ms);
            return this.utopiaController.isConnected();
        }
        public bool isConnected()
        {
            return this.utopiaController.isConnected();
        }
        public string getHostPort()
        {
            return this.utopiaController.getHostPort();
        }


        private int[][] convertstimSeq2int(StimSeq ss)
        {
            // N.B. use jagged array so stimseq[i] gives int[nobj]
            this.stimSeq = new int[ss.stimSeq.Length][];
            for (int t = 0; t < ss.stimSeq.Length; t++)
            {
                stimSeq[t] = new int[ss.stimSeq[t].Length];
                for (int oi = 0; oi < ss.stimSeq[t].Length; oi++)
                {
                    this.stimSeq[t][oi] = (int)ss.stimSeq[t][oi];
                }
            }
            return stimSeq;
        }

        // stimulus sequence methods via the stimulus state machine stack
        public virtual bool updateStimulusState(int t = -1)
        {
            bool hasNext = this.stimulusStateMachineStack.next(t);
            if (hasNext)
            {
                this.lastrawstate = this.stimulusStateMachineStack.get();
            }
            else
            {
                this.lastrawstate = null;
                // Fire a noise-tag complete event?
            }
            return hasNext;
        }

        public virtual StimulusState getStimulusState(int[] objIDs)
        {
            if (objIDs != null) this.setActiveObjIDs(objIDs);
            return this.getStimulusState();
        }

        public virtual StimulusState getStimulusState()
        {
            if (this.lastrawstate == null || this.lastrawstate.stimulusState == null) return null;
            if (this.laststate == null || this.laststate.stimulusState == null) return null;
            // extract the active objIDs subset and return
            //Console.WriteLine("Raw:"+this.lastrawstate);
            //Console.WriteLine("SS:"+this.laststate);
            // subset to set active objIDs
            for (int oi = 0; oi < this.objIDs.Length; oi++)
            {
                // objIDs = rawindex -1
                int stimidx = this.objIDs[oi] - 1;
                //Console.WriteLine("oi:"+oi+"stimidx:"+stimidx);
                this.laststate.stimulusState[oi] = this.lastrawstate.stimulusState[stimidx];
            }
            this.laststate.objIDs = this.objIDs;
            this.laststate.targetState = this.lastrawstate.targetState;
            this.laststate.sendEvents = this.lastrawstate.sendEvents;
            return this.laststate;
        }
        public virtual void setActiveObjIDs(int[] objIDs)
        {
            int nold = this.objIDs == null ? 0 : this.objIDs.Length;
            this.objIDs = objIDs;
            // make use last-state holder to reflect the new set active
            if (nold != this.objIDs.Length)
            {
                this.laststate = new StimulusState(null, this.objIDs, -1, false);
            }

        }
        public virtual void setnumActiveObjIDs(int nobj)
        {
            int[] objIDs = new int[nobj];
            for (int i = 0; i < objIDs.Length; i++)
                objIDs[i] = i + 1;
            setActiveObjIDs(objIDs);
        }

        // decoder interaction methods via. utopia controller
        public virtual void sendStimulusState(
            long timestamp = -1)
        {
            // get from last-state
            int[] stimState = this.laststate.stimulusState;
            int tgtState = this.laststate.targetState;
            int[] objIDs = this.laststate.objIDs;
            bool sendEvent = this.laststate.sendEvents;
            // send info about the stimulus displayed
            if (sendEvent && stimState != null)
            {
                //print((stimState,targetState))
                this.utopiaController.sendStimulusEvent(stimState, timestamp, tgtState, objIDs);
            }
        }

        public virtual PredictedTargetProb getLastPrediction()
        {
                return this.utopiaController.getLastPrediction();
        }
        public virtual float[] getLastSignalQuality()
        {
                return this.utopiaController.getLastSignalQuality();
        }
        public virtual void addMessageHandler(UtopiaController.MessageDegelateType cb)
        {
                this.utopiaController.addMessageHandler(cb);
        }
        public virtual void addPredictionHandler(UtopiaController.PredictionDegelateType cb)
        {
                this.utopiaController.addPredictionHandler(cb);
        }

        public virtual void addSelectionHandler(UtopiaController.SelectionDegelateType cb)
        {
                this.utopiaController.addSelectionHandler(cb);
        }

        public virtual void addSignalQualityHandler(UtopiaController.SignalQualityDegelateType cb)
        {
                this.utopiaController.addSignalQualityHandler(cb);
        }

        public virtual long getTimeStamp(int t0 = 0)
        {
            return this.utopiaController.getTimeStamp(t0);
        }
        public virtual void log(string msg)
        {
                this.utopiaController.log(msg);
        }
        public void modeChange(string newmode)
        {
                this.utopiaController.modeChange(newmode);
        }

        // methods to define what (meta) stimulus sequence we will play
        public virtual void startExpt(
            int nCal = 1,
            int nPred = 20,
            float selnThreshold = 0.1f,
            params object[] args)
        {
            if (objIDs == null) { objIDs = new int[10]; for (int i = 0; i < objIDs.Length; i++) objIDs[i] = i + 1; }
            this.objIDs = objIDs;
            if (this.stimulusStateMachineStack.stack.Count > 0)
            {
                Console.WriteLine("Warning: replacing running sequence?");
                this.stimulusStateMachineStack.clear();
            }
            if (args.Length == 0) args = null;

            this.stimulusStateMachineStack.push(
            new Experiment(this.objIDs,
                   this.stimSeq,
                   nCal,
                   nPred,
                   selnThreshold,
                   this.utopiaController,
                   this.stimulusStateMachineStack,
                   args));
        }

        public virtual void startCalibration(
            int nTrials = 10,
            int[][] stimSeq = null,
            params object[] args)
        {
            if (this.stimulusStateMachineStack.stack.Count > 0)
            {
                Console.WriteLine("Warning: replacing running sequence?");
                this.stimulusStateMachineStack.clear();
            }
            if (args.Length == 0) args = null;

            this.stimulusStateMachineStack.push(
               new CalibrationPhase(this.objIDs,
                        this.stimSeq,
                        nTrials,
                        this.utopiaController,
                        this.stimulusStateMachineStack,
                        args));
        }

        public virtual void startPrediction(
            int nTrials = 10,
            int[][] stimSeq = null,
        float selectionThreshold = .1f,
            params object[] args)
        {
            if (this.stimulusStateMachineStack.stack.Count > 0)
            {
                Console.WriteLine("Warning: replacing running sequence?");
                this.stimulusStateMachineStack.clear();
            }
            if (args.Length == 0) args = null;
            this.stimulusStateMachineStack.push(
             new PredictionPhase(this.objIDs,
                     this.stimSeq,
                     nTrials,
                     this.utopiaController,
                     this.stimulusStateMachineStack,
                     selectionThreshold,
                     args));
        }

        public virtual void startFlicker(
                     int[] objIDs,
                     int numframes = 100,
                     int tgtidx = -1,
                     bool sendEvents = false)
        {
            if (this.stimulusStateMachineStack.stack.Count > 0)
            {
                Console.WriteLine("Warning: replacing running sequence?");
                this.stimulusStateMachineStack.clear();
            }
            this.stimulusStateMachineStack.push(
               new Flicker(
                   this.stimSeq,
                   numframes,
                   tgtidx,
                   sendEvents));
        }

        public virtual void startFlickerWithSelection(
            int numframes = 100,
            int tgtidx = -1,
            bool sendEvents = false,
        float selectionThreshold = .1f
                              )
        {
            if (this.stimulusStateMachineStack.stack.Count > 0)
            {
                Console.WriteLine("Warning: replacing running sequence?");
                this.stimulusStateMachineStack.clear();
            }
            // set the selection threshold
            if (selectionThreshold > 0)
            {
                this.utopiaController.selectionThreshold = selectionThreshold;
            }
            this.stimulusStateMachineStack.push(
            new FlickerWithSelection(
                         this.stimSeq,
                         numframes,
                         tgtidx,
                         this.utopiaController,
                         sendEvents));
        }



        public static void newMessageHandler(List<UtopiaMessage> msgs)
        {
            // simple message handler to test callbacks
            Console.WriteLine(String.Format("Got {0:d} messages:", msgs.Count));
            foreach (UtopiaMessage m in msgs)
            {
                Console.WriteLine(m);
            }
        }

        public static void Main(string[] argv)
        {
            Noisetag nt = new Noisetag();
            if (!nt.connect())
            {
                Console.WriteLine("Error couldn't connect to hub!");
            }
            Console.WriteLine("Connected to " + nt.getHostPort());
            nt.setnumActiveObjIDs(10);
            // debug message handler : prints all new messages
            nt.addMessageHandler(newMessageHandler);
            nt.startExpt(1, 10, .1f);
            float isi = 1.0f / 60f;
            int nframe = 0;
            bool isRunning = true;
            while (isRunning)
            {
                nframe++;
                isRunning = nt.updateStimulusState(nframe);
                if (!isRunning) break;
                StimulusState ss = nt.getStimulusState();
                if (ss.targetState >= 0)
                {
                    Console.Write(ss.targetState > 0 ? "*" : ".");
                }
                else
                {
                    Console.Write(String.Format("{0:d} ", nframe));
                }
                nt.sendStimulusState();
                int sleeptime = (int)(isi * 1000.0f);
                System.Threading.Thread.Sleep((int)(isi * 1000.0f));
            }
        }
    }
}
