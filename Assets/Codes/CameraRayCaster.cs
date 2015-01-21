using UnityEngine;
using System.Collections;

public class CameraRayCaster : MonoBehaviour {

	private Ray ray;
	private RaycastHit hit;
	private Transform camTransform;
	private GameObject lastGO; 

	void Start ()
	{
		camTransform = Camera.main.transform;
	}

	void Update () 
	{
		ray = new Ray(camTransform.position, camTransform.forward);

		if (Physics.Raycast(ray, out hit, 10000f)) 
		{
			if ( hit.transform.gameObject.Equals(lastGO) )
			{
				hit.transform.SendMessage("Seen", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				hit.transform.SendMessage("Unseen", SendMessageOptions.DontRequireReceiver);
			}

			lastGO = hit.transform.gameObject;

		}
	}
}
