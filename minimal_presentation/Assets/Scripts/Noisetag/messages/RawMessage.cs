namespace nl.ma.utopiaserver.messages { 
     public class RawMessage {

        public static int VERBOSITY = 0;

        public int msgID { get; }

        public int version;
         
        public ByteBuffer msgbuffer;

        //public ByteOrder order;
        public int order;

        public RawMessage(int msgID, int version, ByteBuffer msgbuffer) {
            this.version = version;
            this.msgID = msgID;
            this.msgbuffer = msgbuffer;
            this.order = Constants.UTOPIABYTEORDER;
        }

        public RawMessage(int msgID, int version, ByteBuffer msgbuffer, int order) {
            this.version = version;
            this.msgID = msgID;
            this.msgbuffer = msgbuffer;
            this.order = order;
        }

        //  serialize into a bytestream
        public void serialize(ByteBuffer outbuffer) {
            //outbuffer.order(this.order);
            outbuffer.put(((byte)(this.msgID)));
            //  msgID
            outbuffer.put(((byte)(this.version)));
            //  ver
            outbuffer.putShort(((short)(this.msgbuffer.remaining())));
            //  msg size
            outbuffer.put(this.msgbuffer);
            //  payload
        }

        public static void serialize(ByteBuffer buffer, int msgID, int version, ByteBuffer msgbuffer) {
            int order = Constants.UTOPIABYTEORDER;
            //  BODGE! : for byte-order to native..
            //buffer.order(this.order);
            buffer.put(((byte)(msgID)));
            buffer.put(((byte)(version)));
            buffer.putShort(((short)(msgbuffer.remaining())));
            buffer.put(msgbuffer);
        }

        //  deserialize and create RawMessage object
        public static RawMessage deserialize(ByteBuffer buffer) {
            if ((buffer.remaining() < 4)) {
                throw new ClientException("Not enough in receive buffer for RawMessage header");
            }

            int order = Constants.UTOPIABYTEORDER;
            //buffer.order(order);
            int prevpos = (int)buffer.position();
            //  record this so return unmodified buffer if failed to decode
            //  get the message ID (byte->string)
            int msgID = (int)buffer.get();
            //  get the message version (byte->int)
            int version = (int)buffer.get();
            //  get the message size (short->int)
            int size = (int)buffer.getShort();
            System.Console.WriteLine("Message: ID:" + msgID + "(" + version + ")" + size);
            //  Check if size and the number of bytes in the buffer are sufficient
            if ((buffer.remaining() < size)) {
                //  incomplete message, leave buffer to get rest in next call
                buffer.position(prevpos);
                throw new ClientException("Not a full messages worth of data in the buffer!");
            }
            else if ((size < 1)) {
                System.Console.WriteLine("Malformed BODY size");
                throw new ClientException("Malformed BODY size");
            }

            //  copy the bytes for the rest of the message
            ByteBuffer msgbuffer = ByteBuffer.allocate(size);
            msgbuffer.put(buffer.array(), (int)buffer.position(), size);
            //msgbuffer.order(order);
            msgbuffer.rewind();
            buffer.position((buffer.position() + size));
            //  update buffer with processed
            return new RawMessage(msgID, version, msgbuffer, order);
        }

        public UtopiaMessage decodePayload() {
            //  Decode the payload
            UtopiaMessage evt = null;
            if ((this.msgID == StimulusEvent.MSGID)) {
                if ((VERBOSITY > 2)) {
                    System.Console.WriteLine("Trying to read "
                                      + StimulusEvent.MSGNAME + " message");
                }

                evt = StimulusEvent.deserialize(this.msgbuffer);
            }
             else if ((this.msgID == PredictedTargetProb.MSGID)) {
                 if ((VERBOSITY > 2)) {
                     System.Console.WriteLine("Trying to read "
                                       + PredictedTargetProb.MSGNAME + " message");
                 }

                 evt = PredictedTargetProb.deserialize(this.msgbuffer);
             }
             else if ((this.msgID == PredictedTargetDist.MSGID)) {
                 if ((VERBOSITY > 2)) {
                     System.Console.WriteLine("Trying to read "
                                       + PredictedTargetDist.MSGNAME + " message");
                 }

                 evt = PredictedTargetDist.deserialize(this.msgbuffer);
             }
            else if ((this.msgID == ModeChange.MSGID)) {
                if ((VERBOSITY > 2)) {
                    System.Console.WriteLine("Trying to read " + ModeChange.MSGNAME + " message");
                }

                evt = ModeChange.deserialize(this.msgbuffer);
            }
            else if ((this.msgID == Reset.MSGID)) {
                 if ((VERBOSITY > 2)) {
                     System.Console.WriteLine("Trying to read " + Reset.MSGNAME + " message");
                 }

                 evt = Reset.deserialize(this.msgbuffer);
            }
            else if ((this.msgID == NewTarget.MSGID)) {
                if ((VERBOSITY > 2)) {
                    System.Console.WriteLine("Trying to read " + NewTarget.MSGNAME + " message");
                }

                evt = NewTarget.deserialize(this.msgbuffer);
            }
            else if ((this.msgID == Heartbeat.MSGID)) {
                if ((VERBOSITY > 2)) {
                    System.Console.WriteLine("Trying to read " + Heartbeat.MSGNAME + " message");
                }

                evt = Heartbeat.deserialize(this.msgbuffer);
            }
            else if ((this.msgID == SignalQuality.MSGID))
            {
                if ((VERBOSITY > 2))
                {
                    System.Console.WriteLine("Trying to read " + SignalQuality.MSGNAME + " message");
                }

                evt = SignalQuality.deserialize(this.msgbuffer);
            }
            else
            {
                throw new ClientException("Unsupported Message type: " + this.msgID);
            }

            if ((VERBOSITY > 1)) {
                System.Console.WriteLine("Got message: " + evt.ToString());
            }

            return evt;
        }

        public override string ToString() {
            return "{t:" + this.msgID + "." + this.version + " [" + this.msgbuffer.capacity() + "] }";
    }
}  
}
