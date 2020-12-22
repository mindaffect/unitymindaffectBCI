namespace nl.ma.utopia {
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class StimSeq {
    // [ nEvent x nSymb ] stimulus code for each time point for each stimulus
    // N.B. use a jagged array so stimSeq[ei] gives [nSymb]
    public float[][] stimSeq=null; 
    // time stimulus i should finish,
    // i.e. stimulus i is on screen from stimTime_ms[i-1]-stimTime_ms[i]
    public int[]     stimTime_ms=null; // [ nEvent x 1 ]

    //-----------------------------------------------------------------------------------
    // Constructor
    StimSeq(float [][]istimSeq, int[] istimTime_ms){ stimSeq=istimSeq; stimTime_ms=istimTime_ms; }
    StimSeq(float [,]istimSeq, int[] istimTime_ms){
         stimTime_ms=istimTime_ms;
         stimSeq = new float[istimSeq.GetLength(0)][];
         for ( int i=0; i<istimSeq.GetLength(0); i++){
            stimSeq[i]= new float[istimSeq.GetLength(1)];
            for ( int j=0; j<istimSeq.GetLength(1); j++){
               stimSeq[i][j] = istimSeq[i,j];
            }
         }
    }    
    

    //-----------------------------------------------------------------------------------
    // S T R I N G (to/from)
    public override string ToString(){
        return ToString(stimSeq,stimTime_ms);
    }
    public static string ToString(float[][] stimSeq, int[] stimTime_ms){
        string str="";
        str = str + "# stimTime : ";
        if ( stimSeq==null ) {
            str += "<null>\n[]\n\n";
        }else{
            str += "1x" +  stimTime_ms.Length + "\n";
            for(int i=0;i<stimTime_ms.Length-1;i++) str += stimTime_ms[i]+"\t";
            str += stimTime_ms[stimTime_ms.Length-1] + "\n";
            str += "\n\n"; // two new lines mark the end of the array
        }
        if ( stimSeq==null ) {
            str += "# stimSeq[]=<null>\n[]\n";
        } else {
            str += "# stimSeq : " + stimSeq[0].Length + "x" + stimSeq.Length + "\n";
            str += WriteArray(stimSeq,false);
        }
        return str;
    }

    public static StimSeq FromString(TextReader reader) {
        // Read the stimTimes_ms
        float [,]tmpStimTime = ReadArray(reader);
        System.Console.WriteLine("stimTimes=["+tmpStimTime.GetLength(0)+","+tmpStimTime.GetLength(1)+"]");
        if ( tmpStimTime.GetLength(1)>1 && tmpStimTime.GetLength(0)>1 ) {
            System.Console.WriteLine("more than 1 row of stim Times?\n");
            throw new IOException("Vector stim times expected");
        }
        float [,]tmpStimSeq = ReadArray(reader);
        System.Console.WriteLine("stimSeq=["+tmpStimSeq.GetLength(0)+","+tmpStimSeq.GetLength(1)+"]");
        if ( tmpStimSeq.Length<1 ){
                System.Console.WriteLine("No stimSeq found in file!");
            throw new IOException("no stimSeq in file");
        } else if ( tmpStimSeq.GetLength(1) != tmpStimTime.Length ) {
            System.Console.WriteLine("Mismatched lengths of stimTime (" + tmpStimTime.GetLength(0) + "x" + tmpStimTime.GetLength(1) + ")" +
                    " and stimSeq (" + tmpStimSeq.GetLength(0) +"x"+ tmpStimSeq.GetLength(1) + ")");
            throw new IOException("stimTime and stimSeq lengths unequal");
        }
        // All is good convert stimTimes to int vector and construct
        int[] stimTime_ms = new int[tmpStimTime.Length];
        for ( int i=0, k=0; i<tmpStimTime.GetLength(0); i++)
                for ( int j=0; j<tmpStimTime.GetLength(1); j++, k++)
                    stimTime_ms[k]=(int)tmpStimTime[i,j];
        //System.Console.WriteLine("st done");
        // Transpose the stimSeq into [epoch][stimulus], i.e. so faster change over stimulus
        float[][] stimSeq = new float[tmpStimSeq.GetLength(1)][];
        for ( int i=0; i<tmpStimSeq.GetLength(1); i++){// time in tmpStimSeq
            stimSeq[i]=new float[tmpStimSeq.GetLength(0)];
            for ( int j=0; j<tmpStimSeq.GetLength(0); j++){//targets in tmpStimSeq
                stimSeq[i][j]=tmpStimSeq[j,i];
            }
        }
        return new StimSeq(stimSeq,stimTime_ms);
    }


    public static StimSeq FromFilename(string filename) {
          StreamReader sr = new StreamReader(filename);
    	  return StimSeq.FromString(sr);
    }


    //-------------------------------------------------------------------------------------------
    // Utility functions

    public static string WriteArray(float [,]array){ return WriteArray(array,true); }
    public static string WriteArray(float [,]array, bool incSize){
            string str="";
        if ( incSize ) {
            str += "# size = " + array.GetLength(0) + "x" + array.GetLength(1) + "\n";
        }
        for ( int ti=0; ti<array.GetLength(0); ti++){ // time points
            for(int i=0;i<array.GetLength(1)-1;i++) str += array[ti,i] + "\t";
            str += array[ti,array.GetLength(1)-1] + "\n";
        }
        str += "\n\n"; // two new-lines mark the end of the array
        return str;
    }
    public static string WriteArray(float [][]array){
           return WriteArray(array,true);
    }
    public static string WriteArray(float [][]array, bool incSize){
            string str="";
        if ( incSize ) {
            str += "# size = " + array.Length + "x" + array[0].Length + "\n";
        }
        for ( int i=0; i<array.Length; i++){ // time points
            for(int j=0;j<array[i].Length-1;j++) str += array[i][j] + "\t";
            str += array[i][array[i].Length-1] + "\n";
        }
        str += "\n\n"; // two new-lines mark the end of the array
        return str;
    }
 
    public static float[,] ReadArray(TextReader reader){
        if ( reader == null ) {
            System.Console.WriteLine("could not allocate reader");
            throw new IOException("Couldnt allocate a reader");
        }
        int width=-1;
        // tempory store for all the values loaded from file
        List<float[]> rows=new List<float[]>(10);
        string line;
        int nEmptyLines=0;
        //System.out.println("Starting new matrix");
        while ( (line = reader.ReadLine()) != null ) {
            // skip comment lines
            if ( line == null || line.StartsWith("#") ){
                continue;
            } if ( line.Length==0 ) { // double empty line means end of this array
                nEmptyLines++;
                if ( nEmptyLines >1 && width>0 ) { // end of matrix by 2 empty lines
                    //System.Console.WriteLine.println("Got 2 empty lines");
                    break;
                } else { // skip them
                    continue;
                }
            }
            //System.Console.WriteLine("Reading line:\n" + line);

            // split the line into entries on the split character
            string[] values = line.Trim().Split("[ ,	]".ToCharArray()); // split on , or white-space
            //System.Console.WriteLine("Containing [" + values.Length + "]\n");
            
            //foreach ( string str in values ) System.Console.Write(str + ", ");
            if ( width>0 && values.Length != width ) {
                throw new IOException("Row widths are not consistent!");
            } else if ( width<=0 ) {
                width = values.Length;
            }
            // read the row
            float[] cols = new float[width]; // tempory store for the cols data
            for ( int i=0; i<values.Length; i++ ) {
                  //System.Console.WriteLine(i+"="+values[i]);
                  cols[i] = float.Parse(values[i]);
            }
            //System.Console.WriteLine("done");
            // add to the tempory store
            rows.Add(cols);
            //System.Console.WriteLine("added");
        }
        //if ( line==null ) System.Console.WriteLine("line == null");

        if ( width<0 ) return null; // didn't load anything

        // Now put the data into an array
        float[,] array = new float[rows.Count,width];
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < rows[i].Length; j++)
            {
                 array[i, j] = rows[i][j];
            }
        }
        return array;
    }

public static void Main(string[] argv) {
       string stimFile="../../resources/codebooks/mgold_65_6532_60hz.txt";
       if( argv.Length>0 ) {
           stimFile=argv[0];
       }
       // open the file and try to read in the stim-sequence
       try { 
           StreamReader sr = new StreamReader(stimFile);
           StimSeq ss = StimSeq.FromString(sr);
           // print out the stim-sequence
           System.Console.WriteLine("Read ss:");
           System.Console.WriteLine(ss);
       } catch ( IOException ) {
           System.Console.WriteLine("Couldn't open the stimFile "+stimFile);
       }
}


};



};
