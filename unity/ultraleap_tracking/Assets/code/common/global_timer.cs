using UnityEngine;
using System;
using System.Collections;

public class global_timer : MonoBehaviour {

    /* class to provide timing information
    (c) Johannes Achtzehn, 07|2022, Charite - Universitaetsmedizin Berlin, johannes.achtzehn@charite.de
    */

	public static double paradigmTime;      // this is a timestamp representing the amount of time that has has passes since the start
	public static string theTime;			// current time nicely formatted
	DateTime beginTime;						// public variable to keep track when the paradigm started

	// Use this for initialization
	void Awake () {
		beginTime = DateTime.Now;       // get the time at the start
	}
	
	// Update() is called every frame
	void Update () {
		DateTime newTime = DateTime.Now;

		TimeSpan elapsedTime = newTime.Subtract(beginTime) ;

		// update our two output variables each frame
		theTime = newTime.ToString("HH") + ":" + newTime.ToString("mm") + ":" + newTime.ToString("ss") + ":" + newTime.ToString("ffff");
		paradigmTime = elapsedTime.TotalSeconds;

	}
}
