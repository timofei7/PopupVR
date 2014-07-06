using System.Collections.Generic;
using UnityEngine;

[System.Serializable]public class RageButtonData {

	public enum ActionTypes{Enable, Disable, Instantiate, Delete}
	public List<GameObject> ToDisableOnClick, ToEnableOnClick, ToInstantiateOnClick, ToDeleteOnClick;
	public List<MonoBehaviour> ToExecuteOnClick;

	public ButtonExternalData Click, HoverIn, HoverOut;

	public bool UnloadUnusedAssets;
	public bool ClickOnAwake;
	public List<Holoville.HOTween.Tweener> ClickTweeners;
	public List<Holoville.HOTween.Tweener> HoverInTweeners;
	public List<Holoville.HOTween.Tweener> HoverOutTweeners;

    public RageButtonData() {
		ToDisableOnClick = new List<GameObject>();
		ToEnableOnClick = new List<GameObject>();
		ToInstantiateOnClick = new List<GameObject>();
		ToDeleteOnClick = new List<GameObject>();
		ToExecuteOnClick = new List<MonoBehaviour>();
		Click    =	new ButtonExternalData();
		HoverIn  =	new ButtonExternalData();
		HoverOut =	new ButtonExternalData();
		ClickTweeners = new List<Holoville.HOTween.Tweener>();
		HoverInTweeners = new List<Holoville.HOTween.Tweener>();
		HoverOutTweeners = new List<Holoville.HOTween.Tweener>();
    }

	public void InitTargets(GameObject gO) {
		if (Click.Target == null) Click.Target = gO;
		if (HoverIn.Target == null) HoverIn.Target = gO;
		if (HoverOut.Target == null) HoverOut.Target = gO;
	}
}

[System.Serializable]public class ButtonExternalData {
	public AudioSource Sound;
	public string TweenId;
	public bool PlayReverse;
	public bool Reset;
	public GameObject Target;

    public ButtonExternalData() {
        Sound = null;
        TweenId = "";
        PlayReverse = false;
        Reset = false;
        Target = null;
    }
}

// [System.Serializable]public class ButtonClickData {
// 	public RageButtonData.ActionTypes ActionType { get; set; }
// 	public GameObject gO { get; set; }
// }
