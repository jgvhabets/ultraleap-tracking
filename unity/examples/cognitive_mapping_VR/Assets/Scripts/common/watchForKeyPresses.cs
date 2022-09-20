using UnityEngine;
using System.Collections;

public class watchForKeyPresses : MonoBehaviour {

	void Start() {

		}

	// Update is called once per frame
	void Update () {

		if (Input.GetKey(KeyCode.Escape)) {
			Application.Quit();

			}

	}
}
