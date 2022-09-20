/*
Preamble
--------------

This class is used to create a log file, which then can be used to log different events.

*/

using UnityEngine;
using System.IO;

public class logfile {

    /* class to provide logging functionality
    This class provides basic functionality to open, close and write data to a file
    (c) Johannes Achtzehn, 07|2022, Charite - Universitaetsmedizin Berlin, johannes.achtzehn@charite.de
    */

    private StreamWriter sw;                // this will be our streamwriter

	public void openFile(string fileName) {
		sw = new StreamWriter(fileName);	// open StreamWriter to file
        sw.AutoFlush = true;
	}

	public void writeHeader (string data) {
		sw.WriteLine(data);                 // writeHeader() writes data without timing information
	}

	// Function which writes given information to the log file, either only position data or also event information
	public void write (string data) {

		string timeStamp = (global_timer.paradigmTime).ToString("0.0000"); 		        // get the current time since startup

		sw.WriteLine( global_timer.theTime + "," + timeStamp + "," + data);

        }

   public void closeFile()
        {
        sw.Close();
        }

}
