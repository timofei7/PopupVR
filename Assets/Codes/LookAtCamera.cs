using UnityEngine;
using System.Collections;

public class LookAtCamera : MonoBehaviour {

	private Transform camTransform;

	// Use this for initialization
	void Start () {
		camTransform = Camera.main.transform;

	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(transform.position + camTransform.rotation * Vector3.forward,  camTransform.rotation * Vector3.up);
	}
}
