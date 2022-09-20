/*
Preamble
--------------

This class is used to create a log file, which then can be used to log different events.

*/

using UnityEngine;
using System.IO;

public class logFile {

    private StreamWriter sw;     
	public bool debug = false;

	public void openFile(string fileName) {
		sw = new StreamWriter(fileName);	// open StreamWriter to file
        sw.AutoFlush = true;
	}

	public void writeHeader (string data) {
		sw.WriteLine("// " + data);
	}

	// Function which writes given information to the log file, either only position data or also event information
	public void write (string data) {

		string timeStamp = (globalTimer.paradigmTime).ToString("0.000"); 		        // get the current time since startup

		sw.WriteLine(timeStamp + "," + data);

		if (debug == true) {
			Debug.Log(timeStamp + ": " + data);
		}
        }

   public void closeFile()
        {
        sw.Close();
        }

}
