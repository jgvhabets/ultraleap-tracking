using UnityEngine;
using System.Collections;

public class rotateCamera : MonoBehaviour {

	[SerializeField] private float rotateSpeed;
	private Transform mainCamera;

	// Use this for initialization
	void Start () {
		mainCamera = this.gameObject.transform;		
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKey(KeyCode.LeftArrow)) {
			mainCamera.Rotate(0, -rotateSpeed * Time.deltaTime, 0);
			}

		if (Input.GetKey(KeyCode.RightArrow)) {
			mainCamera.Rotate(0, rotateSpeed * Time.deltaTime, 0);
			}
		}

}
