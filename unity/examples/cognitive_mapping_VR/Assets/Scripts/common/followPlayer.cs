using UnityEngine;

public class followPlayer : MonoBehaviour {

	public bool translation = true;
	public bool rotation = true;

	private Transform playerPos;
	private Transform cameraPos;

	public float rotationSmoothing;
	public float positionSmoothing;

	// Use this for initialization
	void Awake () {

		playerPos = GameObject.Find("player").GetComponent<Transform>();
		cameraPos = this.gameObject.transform;

	}
	
	// Update is called once per frame
	void LateUpdate () {

		Vector3 newPos = new Vector3(playerPos.position.x, 1.8f, playerPos.position.z);
		Quaternion newRot = Quaternion.Euler(0, playerPos.rotation.eulerAngles.y, 0);

		if (translation == true) {
			cameraPos.position = Vector3.Lerp(cameraPos.position, newPos, Time.deltaTime * positionSmoothing);
			}
		if (rotation == true) {
			cameraPos.rotation = Quaternion.Lerp(cameraPos.rotation, newRot, Time.deltaTime * rotationSmoothing);
			}

	}
}
