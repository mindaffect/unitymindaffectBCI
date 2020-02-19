namespace nl.ma.utopiaserver{
using System.Diagnostics;
/**
 * Class to provide the timeStamp information needed for the messages
 */
/*
 * Copyright (c) MindAffect B.V. 2018
 * For internal use only.  Distribution prohibited.
 */
public class TimeStampClock {
    static long t0;
    /**
     * construct a new time-stamp clock.
     */
    public TimeStampClock(){
        t0 = 0;//getAbsTime();
	 }
    /**
     * get the current time, relative to clock construction time.
     */   
    public static long getTime(){
        return (int)(getAbsTime()-t0);
    }
    /**
     * get the current absolute time -- i.e. from nanoTime
     */
    public static long getAbsTime(){
        return (long) (Stopwatch.GetTimestamp() * 1000.0d/Stopwatch.Frequency);
    }
};
}

