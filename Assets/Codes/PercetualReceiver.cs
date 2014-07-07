using UnityEngine;
using System.Collections;

public class PercetualReceiver : MonoBehaviour {

	public Material MaterialToEffect;
	public Light LightToEffect;

	public Color MaterialTargetColor;
	public Color LightTargetColor;

	public float seen;
	public float step = 0.001f;

	void Start ()
	{
		seen = 0f;
	}
	
	void Update ()
	{
	
	}

	public void Seen ()
	{
		seen += step * Time.deltaTime;
		MaterialToEffect.color = Color.Lerp(MaterialToEffect.color, MaterialTargetColor, seen);
		LightToEffect.color = Color.Lerp(LightToEffect.color, LightTargetColor, seen);
	}

	public void Unseen ()
	{
		seen = 0;
	}
}
