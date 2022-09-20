using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class crossFadeToZero : MonoBehaviour {

	// Use this for initialization
	void Awake () {

		RawImage rawImage = this.gameObject.GetComponent<RawImage>();	// get the raw image
		rawImage.CrossFadeAlpha(0f, 0f, false);							// fade it to alpha 0
		}
	
}
