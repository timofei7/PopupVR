using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

[AddComponentMenu("RageTools/Rage Group %#g")]
[ExecuteInEditMode]
public partial class RageGroup : MonoBehaviour {
	public bool Tweak;

	[SerializeField] private List<RageGroupElement> _list;
	public List<RageGroupElement> List {
		get {
			//if (_list != null && _list[0] == null) UpdatePathList();
			if (_list != null) return _list;
			_list = new List<RageGroupElement>();
			return _list;
		}
		set { _list=value; }
	}

	//[SerializeField] private bool _visible = true;
	public bool Visible {
		get {
				if ((List.Count == 0) || List[0] == null || List[0].Spline == null || List[0].Spline.Rs == null) return true;
// 				if (List == null) return false;
// 				if (List.Count == 0) return false;
// 			if (List[0] == null) return false;
// 			if (List[0].Spline == null) return false;
// 			if (List[0].Spline.Rs == null) return false;
				if ((List.Count != 0) && List[0].Spline != null && List[0].Spline.Rs != null) return List[0].Spline.Rs.Visible;
				return true;
			}
		set {
				if ((List.Count == 0) || List[0] == null || List[0].Spline == null || List[0].Spline.Rs == null) return;
				if (value == List[0].Spline.Rs.Visible) return;
				foreach (RageGroupElement item in List)
					item.Spline.Rs.Visible = value;
				QueueRefresh();
			}
	}
    [SerializeField]private bool _pinUVs;
    public bool PinUVs {
        get {return _pinUVs;}
        set {
            if (value == _pinUVs) return;
            _pinUVs = value;
            foreach (var element in List)
                element.Spline.Rs.PinUvs = value;
        }
    }
    [SerializeField]
    private bool _autoRefresh;
    public bool AutoRefresh {
        get { return _autoRefresh; }
        set {
            if (value == _autoRefresh) return;
            _autoRefresh = value;
            foreach (var element in List)
				if (element != null && element.Spline != null)
					element.Spline.Rs.AutoRefresh = value;
        }
    }
    
    [SerializeField]private bool _localAaWidth;
    public bool LocalAaWidth {
        get { return _localAaWidth; }
        set {
            if (value == _localAaWidth) return;
            _localAaWidth = value;
            List.SetLocalAa(value);
        }
    }

	public List<RageGroup> ExcludedGroups;
	public List<RageSplineStyle> Styles;
	public List<string> StyleNames;
	public float AntiAlias = 1.0f;
	[SerializeField]private int _density = 3;
	public int Density {
		get { return _density; }
		set { _density = Mathf.Max(1,value); }
	}
	[SerializeField] private float _currentOpacity = 1f;
	public float Opacity {
		get { return _currentOpacity; }
		set { if (Mathf.Approximately(_currentOpacity, value)) return;
			_currentOpacity = value;
			DoUpdateOpacity();
		}
	}
	public float AaMult = 1.0f;
	public float DensityMult = 1.0f;
	[SerializeField] private float _currentOpacityMult = 1f;
	public float OpacityMult {
		get { return _currentOpacityMult; }
		set {
			if (Mathf.Approximately(_currentOpacityMult, value)) return;
			_currentOpacityMult = value;
			DoUpdateOpacity();
		}
	}

    #region Optimize / OptimizeAngle
    [SerializeField]private bool _optimize;
    public bool Optimize {
        get { return _optimize; }
        set {
            if (value == _optimize) return;
            _optimize = value;
            DoUpdateOptimize(_optimize);
        }
    }
    [SerializeField]private float _optimizeAngle = 5f;
    public float OptimizeAngle {
        get { return _optimizeAngle; }
        set {
            if (Mathf.Approximately(value,_optimizeAngle) || value < 0f) return;
            _optimizeAngle = value;
            DoUpdateOptimize(_optimize, _optimizeAngle);
        }
    }
    #endregion

	public bool Proportional = true;
	public Vector3[] Boundaries;
	public Vector3 Center;
	public bool GroupExclusion;
	public bool ShowSwitchsets;
	public bool UseStyles;			// used by the Editor
	public int UpdateStep = 8;
	public bool Draft;
	public bool IsRefreshing {
		get { return _doRefresh; }
	}
	private bool _doRefresh;
	private int _updaterIndex;
	private float _zMin, _zMax;
	private bool _dirtyRefresh;
	private bool _extraRefresh;

	[SerializeField] private float _currentAa;
	[SerializeField] private int _currentDensity;
	[SerializeField] private float _currentAaMult;
	[SerializeField] private  float _currentDensityMult;

	[SerializeField] private List<string> _switchsetsKeys;
	[SerializeField] private List<Switchset> _switchsetsValues;
	private Dictionary<string, Switchset> _switchsets;
	public Dictionary<string, Switchset> Switchsets {
		get {
			if (_switchsets == null) InitSwitchsets();
			return _switchsets;
		}
		set { _switchsets = value; }
	}

	public void InitSwitchsets() {
		_switchsets = new Dictionary<string, Switchset>();
		if (_switchsetsKeys == null) _switchsetsKeys = new List<string>();
		if (_switchsetsValues == null) _switchsetsValues = new List<Switchset>();

		if (_switchsetsKeys.Count > 0)
			for (int i = 0; i < _switchsetsKeys.Count; i++) {
				_switchsets.Add(_switchsetsKeys[i], _switchsetsValues[i]);
				var thisSwitchset = _switchsets[_switchsetsKeys[i]];
				for (int j = 0; j < thisSwitchset.ItemKeys.Count; j++) {
					thisSwitchset.Items.Add(thisSwitchset.ItemKeys[j], thisSwitchset.ItemValues[j]);
				}
			}
	}

	public void ApplyStyle() {
	    if (StyleNames.Count > 0 ) ApplyStyle(StyleNames, Styles);
	}

	public void ApplyStyle(List<string> names, List<RageSplineStyle> styles) {
		if(List == null) return;

		for(int i = 0; i < names.Count; i++ ) {
			foreach(RageGroupElement element in List) {
				if(element.Spline == null) continue;
				if(!element.Spline.GameObject.name.Contains(names[i])) continue;

				var rSpline = element.Spline.Rs;
				rSpline.SetStyle (null);			//Assures no RageSplineStyle overrides the action

				rSpline.SetTexturing1(styles[i].GetTexturing1());
				rSpline.SetTexturing2(styles[i].GetTexturing2());
				rSpline.SetTextureOffset(styles[i].GetTextureOffset());
				rSpline.SetTextureOffset2(styles[i].GetTextureOffset2());
				rSpline.SetTextureAngleDeg(styles[i].GetTextureAngleDeg());
				rSpline.SetTextureAngle2Deg(styles[i].GetTextureAngle2Deg());
				rSpline.SetTextureScale2Inv(styles[i].GetTextureScale2Inv());
				rSpline.SetTextureScaleInv(styles[i].GetTextureScaleInv());

				rSpline.SetFill(styles[i].GetFill());
				rSpline.SetFillColor1(styles[i].GetFillColor1());
				rSpline.SetFillColor2(styles[i].GetFillColor2());

				rSpline.SetGradientAngleDeg(styles[i].GetGradientAngleDeg());
				rSpline.SetGradientOffset(styles[i].GetGradientOffset());
				rSpline.SetGradientScaleInv(styles[i].GetGradientScaleInv());

				rSpline.SetOutline(styles[i].GetOutline());
				rSpline.SetOutlineColor1(styles[i].GetOutlineColor1());
				rSpline.SetOutlineColor2(styles[i].GetOutlineColor2());
				rSpline.SetOutlineWidth(styles[i].GetOutlineWidth());
				rSpline.SetOutlineGradient(styles[i].GetOutlineGradient());
				rSpline.SetOutlineNormalOffset(styles[i].GetOutlineNormalOffset());
				rSpline.SetOutlineTexturingScaleInv(styles[i].GetOutlineTexturingScaleInv());

				rSpline.SetCorners(styles[i].GetCorners());
				rSpline.SetOptimize(styles[i].GetOptimize());
				rSpline.SetOptimizeAngle(styles[i].GetOptimizeAngle());

				rSpline.RefreshMesh();
			}
		}
	}

	public void ApplyStylePhysics() { ApplyStylePhysics(StyleNames, Styles); }
	public void ApplyStylePhysics(List<string> names, List<RageSplineStyle> styles) {
		if(List == null) return;

		for(int i = 0; i < names.Count; i++ ) {
			foreach(RageGroupElement element in List) {
				if(element.Spline == null) continue;
				if(!element.Spline.GameObject.name.Contains(names[i])) continue;

				var rSpline = (RageSpline)element.Spline.MonoBehaviour;
				rSpline.SetPhysics(styles[i].GetPhysics());
				rSpline.SetPhysicsMaterial(styles[i].GetPhysicsMaterial());
				rSpline.SetPhysicsColliderCount(styles[i].GetPhysicsColliderCount());
				rSpline.SetPhysicsNormalOffset(styles[i].GetPhysicsNormalOffset());
				rSpline.SetPhysicsZDepth(styles[i].GetPhysicsZDepth());
				rSpline.SetCreatePhysicsInEditor(styles[i].GetCreatePhysicsInEditor());

				rSpline.RefreshMesh();
			}
		}
	}

	public void Awake() {
		if (List == null || List.Count == 0)
			UpdatePathList();
		foreach (var switchset in Switchsets)
			switchset.Value.ResetItemsVisibility();
	}

	//[Conditional("UNITY_EDITOR")]
#if UNITY_EDITOR
	public void OnDrawGizmos() {
		// Fix for prefab spawn issue
		if (List == null || List.Count == 0 || (List.Count >= 1 && List[0] == null)) {
			UpdatePathList();
			ProgressiveUpdater();
		}
// 		if (Application.isPlaying) return;
// 		ProgressiveUpdateCheck();
 	}
#endif

	public void LateUpdate() {
		if (Tweak) TweakCheck();
		ProgressiveUpdateCheck();
	}

	private void ProgressiveUpdateCheck( ) {
		if (_doRefresh) {
			ProgressiveUpdater();
			return;
		}
		if (_extraRefresh) {
			_extraRefresh = false;
			_doRefresh = true;
			ProgressiveUpdater();
		}
	}

	private void TweakCheck() {
		if(Proportional) {
			if(!Mathf.Approximately(AaMult, _currentAaMult)) DoUpdateAa();
			if (!Mathf.Approximately(OpacityMult, _currentOpacityMult)) DoUpdateOpacity();
			if(!Mathf.Approximately(DensityMult, _currentDensityMult)) DoUpdateDensity();
			QueueRefresh();
			return;
		}
		if(!Mathf.Approximately(AntiAlias, _currentAa)) DoUpdateAa();
		if (!Mathf.Approximately(Opacity, _currentOpacity)) DoUpdateOpacity();
		if(!Mathf.Approximately(Density, _currentDensity)) DoUpdateDensity();
		QueueRefresh();
	}

	private void DoUpdateOpacity( ) {
		if (Proportional) {
			List.UpdateOpacity(OpacityMult, true, false);
			_currentOpacityMult = OpacityMult;
			return;
		}
		List.UpdateOpacity(Opacity, false, false);
		_currentOpacity = Opacity;
        QueueRefresh();
	}

	private void DoUpdateAa() {
		if (Proportional) {
			List.UpdateAa(AaMult, true, false);
			_currentAaMult = AaMult;
			return;
		}
		List.UpdateAa(AntiAlias, false, false);
		_currentAa = AntiAlias;
	}

	private void DoUpdateDensity() {
		if (Proportional) {
			DensityMult = Mathf.Max(0.1f, DensityMult);
			List.UpdateDensity(DensityMult, 100);
			_currentDensityMult = DensityMult;
			return;
		}
		List.UpdateDensity(Density);
		_currentDensity = Density;
	}

    private void DoUpdateOptimize(bool optimize, float angle = -1f) {
        List.UpdateOptimize(optimize, angle);
		QueueRefresh();
    }

	private static bool IsNull(Object o){ return o == null; }
	
	public void AddExcludedGroupIfPossible(RageGroup group){
		if(group.ExcludedGroups.Contains(this)) return;
		ExcludedGroups.Add(group);
	}

	public void AddStyle(RageSplineStyle style) {
		Styles.Add(style);
		StyleNames.Add("");
	}

	public void UpdatePathList() {
		if (List == null)
			List = new List<RageGroupElement>();

		if (ExcludedGroups == null)
			ExcludedGroups = new List<RageGroup>();
		
		ExcludedGroups.RemoveAll(IsNull);
		var toExcludeSplines = new List<long>();			
		if(GroupExclusion){
			foreach (RageGroup group in ExcludedGroups){
				group.UpdatePathList();
				foreach (RageGroupElement element in group.List)
					toExcludeSplines.Add(element.Spline.Oid);
			}
		}

		ISpline[] splines = Spline.GetSplinesInChildren(gameObject);
		if (splines.Length == 0){
			List.Clear();
			return;
		}

		List<RageGroupElement> oldList = List;
		List = new List<RageGroupElement>();

		foreach(RageSplineAdapter spline in splines) {
			if(toExcludeSplines.Contains(spline.Oid)) continue;
			var pathPointCount = spline.PointsCount;

			RageGroupElement item;
			if(oldList.Count > 0) {
				item = oldList[0];
				oldList.RemoveAt(0);
			} else 
				item = ScriptableObject.CreateInstance<RageGroupElement>();
			
			if (item == null) continue;
			item.Spline = spline;
			item.DefaultAa = spline.Outline.AntialiasingWidth;
			item.DefaultDensity = spline.VertexDensity;
			item.DefaultFillColor1 = spline.FillColor;
			item.DefaultFillColor2 = spline.Rs.GetFillColor2();
			item.DefaultOutlineColor = spline.OutlineColor;
			item.GroupPointCache = new RageGroupPointCache[pathPointCount];
			item.GradientOffsetCache = spline.GameObject.transform.TransformPoint(spline.FillGradient.Offset);
			item.TextureOffsetCache = spline.GameObject.transform.TransformPoint(spline.Rs.GetTextureOffset());
			item.TextureOffsetCache2 = spline.GameObject.transform.TransformPoint(spline.Rs.GetTextureOffset2());
			item.TextureScaleCache = spline.Rs.GetTextureScaleInv();
			item.TextureScaleCache2 = spline.Rs.GetTextureScale2Inv();
			item.TextureAngleCache = spline.Rs.GetTextureAngleDeg();
			item.TextureAngleCache2 = spline.Rs.GetTextureAngle2Deg();
			item.PositionCache = spline.GameObject.transform.position;
			item.RotationCache = spline.GameObject.transform.rotation;
			item.ScaleCache = spline.GameObject.transform.lossyScale;
// 			if (item.MeshRenderer==null)
// 				item.MeshRenderer = spline.GameObject.GetComponent<MeshRenderer>();

			CachePointsDefaultData(spline, item, pathPointCount);
			List.Add(item);
		}

		oldList.Clear();
		UpdateCenter();
		AaMult = 1f;
		DensityMult = 1f;
	}

	/// <summary> Updates the Group Center without updating the Path List </summary>
	public void UpdateCenter() {
		if (List.Count == 0) return;
		var referenceSpline = FindFirstValidSpline();
		if (referenceSpline == null) return;
		Vector3 firstPointPos = referenceSpline.GetPositionWorldSpace(0);
		Boundaries = new Vector3[2];
		Boundaries[0] = Boundaries[1] = new Vector3(firstPointPos.x, firstPointPos.y, firstPointPos.z);

		foreach(var path in List)
			if (path.Spline.Rs != null) Boundaries = CheckBoundaries(path.Spline, Boundaries);

		Center = new Vector3((Boundaries[0].x + Boundaries[1].x) / 2, 
							 (Boundaries[0].y + Boundaries[1].y) / 2,
							 (Boundaries[0].z + Boundaries[1].z) / 2);
	}

	private RageSpline FindFirstValidSpline( ) {
		RageSpline referenceSpline = null;
		foreach (RageGroupElement element in List) {
			if (element.Spline.Rs == null) continue;
			referenceSpline = element.Spline.Rs;
		}
		return referenceSpline;
	}

	/// <summary> Check the boundaries against this path boundaries and updates it if necessary </summary>
	/// <param name="path"></param>
	/// <param name="boundaries">boundaries[0]=min, boundaries[1]=max</param>
	private Vector3[] CheckBoundaries(ISpline path, Vector3[] boundaries) {
		float xMin = boundaries[0].x; float yMin = boundaries[0].y;
		float xMax = boundaries[1].x; float yMax = boundaries[1].y;

		for(int i = 0; i < path.Rs.GetPointCount(); i++) {
			Vector3 point = path.Rs.GetPositionWorldSpace(i);
			xMin = Mathf.Min(point.x, xMin);
			yMin = Mathf.Min(point.y, yMin);
			xMax = Mathf.Max(point.x, xMax);
			yMax = Mathf.Max(point.y, yMax);
		}
		float zMin = Mathf.Min(path.Rs.transform.position.z, boundaries[0].z);
		float zMax = Mathf.Max(path.Rs.transform.position.z, boundaries[1].z);
		boundaries[0]=new Vector3 (xMin, yMin, zMin); 
		boundaries[1]=new Vector3 (xMax, yMax, zMax);
		return boundaries;
	}

	/// <summary> Caches the original world position of the spline points. Very useful for Pivotools. </summary>
	private static void CachePointsDefaultData(ISpline path, RageGroupElement item, int pathPointCount) {
		for (int i = 0; i < pathPointCount; i++) {
			item.GroupPointCache[i] = new RageGroupPointCache();
			ISplinePoint point = path.GetPointAt(i);
			item.GroupPointCache[i].PointPos = point.Position;
			item.GroupPointCache[i].InCtrlPos = point.InTangent;
			item.GroupPointCache[i].OutCtrlPos = point.OutTangent;
		}
	}

	public void Reset() {
		if (List == null) return;

		_currentAaMult = AaMult = 1f;
		_currentDensityMult = DensityMult = 1f;
		Proportional = true;
		Tweak = false;
		DoUpdateDensity();
		DoUpdateAa();
		foreach (RageGroupElement item in List)
			item.Spline.Rs.RefreshMeshInEditor(true,true,true);
		
// 		ISpline spline = Spline.GetSplineInChildren(gameObject);
// 		if(spline == null) return;
// 		if (spline.Outline != null)
// 			_currentAa = AntiAlias = spline.Outline.AntialiasingWidth;
// 		_currentDensity = Density = spline.VertexDensity;
	}

	public void CheckForEdgetune() {
		var thisEdgetune = GetComponent<RageEdgetune>();

		if (thisEdgetune == null) return;
		if (!thisEdgetune.On) return;

		thisEdgetune.On = false;
		Debug.LogWarning("Edgetune disabled to prevent conflict with Group Tweak.");
	}

	private void ProgressiveUpdater () {
#if UNITY_EDITOR
		if (!Application.isPlaying) ProgressiveUpdater (0);
		else
#endif
		ProgressiveUpdater (UpdateStep);
	}

	[ExecuteInEditMode]
	/// <summary> Progressively refreshes all splines of a RageGroup </summary>
	private void ProgressiveUpdater(int updateStep) {
		bool finishUpdate = false;
		int passEndIndex = _updaterIndex + updateStep;
		if (passEndIndex > List.Count || updateStep <= 0) {
			passEndIndex = List.Count;
			finishUpdate = true;
		}
		while (_updaterIndex < passEndIndex) {
			if (List[_updaterIndex] == null || List[_updaterIndex].Spline == null) {
				UpdatePathList();
				break;
			}
			var spline = List[_updaterIndex].Spline;
			_updaterIndex++;
			if (spline == null) continue;
			if (_dirtyRefresh && !spline.IsDirty) continue;
			RedrawDraftCheck (spline, Draft);
			spline.IsDirty = false;
		}
		if (finishUpdate) {
			_updaterIndex = 0;
			_doRefresh = false;
		}
	}

	private static void RedrawDraftCheck(ISpline spline, bool draft) {
		if (draft) {
			spline.Optimize = false;
			spline.Redraw (false, true, false, true);
		}
		else spline.Redraw();
	}

	/// <summary> Fit the Texture scale horizontally and offset of all group elements to this group boundaries </summary> 
	[ContextMenu("UV Fit Horizontal")]
	public void UvFitHorizontal() {
		RageGroupUvFit (this, true);
	}

    /// <summary> Fit the Texture scale vertically and offset of all group elements to this group boundaries </summary> 
    [ContextMenu("UV Fit VerticalStyled")]
    public void UvFitVertical() {
        RageGroupUvFit(this, false);
    }

	[ContextMenu("Refresh Layers")]
	public void RefreshLayers() {
		foreach (RageGroupElement item in List) {
			var layer = item.Spline.GameObject.GetComponent<RageLayer>();
			if (layer == null) continue;
			layer.SetMaterialRenderQueue();
		}
		Debug.LogWarning("RageTools: Please disregard error messages above.");
	}

	/// <summary> Opens a window and applies a material to the selected group </summary> 
//	[ContextMenu("Apply Material")]

	/// <summary> Cleans up all group elements name extensions (after first underscore) </summary> 
	[ContextMenu("Cleanup Names")]
	public void CleanupExtensions() {
		gameObject.CleanupElementNames();
		UpdatePathList();
	}

    /// <summary> Fit the Texture scale and offset of all group elements to the group boundaries </summary>
    /// <param name="group"> Group to be processed </param>
    /// <param name="horizontal"> </param>
    public void RageGroupUvFit(RageGroup @group, bool horizontal) {
		UpdateCenter();

		var min = group.Boundaries[0];
		var max = group.Boundaries[1];

		foreach (var item in group.List) {
			var offset = group.Center - item.Spline.Rs.gameObject.transform.position;
			item.Spline.Rs.SetTextureOffset(offset);
			item.Spline.Rs.SetTextureOffset2(offset);
            float textureScale = horizontal ? 1 / (new Vector2(0, max.x) - new Vector2(0, min.x)).magnitude
                                            : 1 / (new Vector2(0, max.y) - new Vector2(0, min.y)).magnitude;
			item.Spline.Rs.SetTextureScaleInv(textureScale);
			item.Spline.Rs.SetTextureScale2Inv(textureScale);
			item.Spline.Rs.SetTextureAngleDeg(0f);
			item.Spline.Rs.SetTextureAngle2Deg(0f);
			item.Spline.Rs.RefreshMeshInEditor(true,true,true);
		}
	}

	public void QueueRefresh() {
		if (!_doRefresh) _doRefresh = true;
		else _extraRefresh = true;
		_dirtyRefresh = false;
	}

	public void QueueDirtyRefresh() {
		if (!_doRefresh) _doRefresh = true;
		else _extraRefresh = true; 
		_dirtyRefresh = true;
	}

	public int GetMaxDensity( ) {
		int maxDensity = 0;
		foreach (var item in List)
			maxDensity = (item.DefaultDensity > maxDensity) ? item.DefaultDensity : maxDensity;
		return maxDensity;
	}

	public void SetVisibility( bool visible ) {
		Visible = visible;
	}

	public void SetOpacity (float opacity) {
		if (!Visible) return;
		if (Proportional) OpacityMult = opacity;
		else Opacity = opacity;
		DoUpdateOpacity();
		QueueRefresh();
	}

	// v^v^v^v^v^
	// SWITCHSETS 
	// v^v^v^v^v^

	/// <summary> Switches the first occurrence of an Item Id. 
	/// Switchset items (elements) can be separated by commas, and have their host switchset pointed by a preceding sub-element
	/// Sub-elements are separated by points. Eg.: "lhand.lhandopen,rhand.rhandopen" or "lhandopen,rhandopen" both work. </summary>
	public void SwitchsetItem(string switchsetItem) {
		string[] elements = switchsetItem.Split(new[] { ',' });
		Switchset targetSwitchset;
		foreach (var itemElement in elements) {
			string[] subElements = itemElement.Split(new[] { '.' });
			targetSwitchset = subElements.Length == 1 ? FindSwitchset(itemElement) : Switchsets[subElements[0]];
			//Debug.Log("item: "+itemElement+" switchset: " + Switchsets.ContainsValue(targetSwitchset));
			if (targetSwitchset == null) {
				Debug.Log("Error: Switchset Element not found");
				continue;
			}
			targetSwitchset.ActiveItem = subElements.Length == 1 ? itemElement : subElements[1];
		}
	}

	public Switchset FindSwitchset(string switchsetItem) {
		Switchset targetSwitchset = null;
 		foreach (var entry in Switchsets) {
 			var thisSwitchset = entry.Value;
 			if (thisSwitchset.Items.ContainsKey(switchsetItem))
 				targetSwitchset = thisSwitchset;
 		}
		return targetSwitchset;
	}

	public void SwitchsetItem (string switchSetId, string itemToEnable) {
		Switchsets[switchSetId].ActiveItem = itemToEnable;
	}

	public void AddSwitchset (string id, Switchset switchset) {
		Switchsets.Add (id, switchset);
		_switchsetsKeys.Add (id);
		_switchsetsValues.Add (switchset);
	}

	public void AddSwitchsetGroup (Switchset targetSwitchset, string groupId, RageGroup newSwitchsetGroup) {
		targetSwitchset.AddItem (groupId, newSwitchsetGroup);
		if (targetSwitchset.ActiveItem == "")
			targetSwitchset.ActiveItem = groupId;
		targetSwitchset.ResetItemsVisibility();
	}

	public void RemoveSwitchset (string switchsetId) {
		_switchsetsKeys.Remove(switchsetId);
		_switchsetsValues.Remove(Switchsets[switchsetId]);
		Switchsets.Remove (switchsetId);
		if (Switchsets.Count == 0) LastShownSwitchsetId = "";
	}

	public string LastShownSwitchsetId;

	public void SetShownSwitchset (string switchsetId, bool show) {
		Switchset targetSwitchset = Switchsets[switchsetId];
		if (!show) {
			targetSwitchset.ShowInInspector = false;
			return;
		}
		if (LastShownSwitchsetId == switchsetId) return;
		targetSwitchset.ShowInInspector = true;

 		foreach (var switchset in Switchsets)
 			switchset.Value.ShowInInspector = (switchset.Value == targetSwitchset);
		LastShownSwitchsetId = switchsetId;
	}
}

/// <summary> Switchsets are collections of RageGroups, where only one of them can be visible at a time </summary>
[Serializable]public class Switchset {

/*	public Dictionary<string, RageGroup> Items;*/
	[SerializeField]private string _activeItem;
	public string ActiveItem {
		get { return _activeItem; }
		set {
			if (_activeItem == value || !Items.ContainsKey(value)) return;
			if (Items.ContainsKey(_activeItem)) Items[_activeItem].Visible = false;
			Items[value].Visible = true;
			_activeItem = value;
		}
	}
	public bool ShowInInspector;

	public Switchset() {
		_activeItem = "";
		InitSwitchsetItems();
		ShowInInspector = true;
	}

	/// <summary> Ideally to be called on start, to fix any user mess up in editor time </summary>
	public void ResetItemsVisibility() {
		foreach (KeyValuePair<string, RageGroup> entry in Items)
			if (entry.Value)
				entry.Value.Visible = (entry.Key == ActiveItem);
	}

	[SerializeField] public List<string> ItemKeys;
	[SerializeField] public List<RageGroup> ItemValues;
	private Dictionary<string, RageGroup> _items;
	public Dictionary<string, RageGroup> Items {
		get {
			if (_items == null) InitSwitchsetItems();
			return _items;
		}
		set { _items = value; }
	}
	public void InitSwitchsetItems() {
		if (ItemKeys == null) ItemKeys = new List<string>();
		if (ItemValues == null) ItemKeys = new List<string>();
		if (ItemKeys.Count == 0)
			_items = new Dictionary<string, RageGroup>();
		else
			for (int i = 0; i < ItemKeys.Count; i++)
				_items.Add(ItemKeys[i], ItemValues[i]);
	}

	public void AddItem(string groupId, RageGroup newSwitchsetGroup) {
		Items.Add(groupId, newSwitchsetGroup);
		ItemKeys.Add(groupId);
		ItemValues.Add(newSwitchsetGroup);
	}

	public void RemoveItem (string groupId) {
		ItemKeys.Remove(groupId);
		ItemValues.Remove(Items[groupId]);
		Items.Remove(groupId);
		if (ActiveItem == groupId && Items.Count > 0)
			ActiveItem = ItemKeys[0];
		if (Items.Count == 0) ActiveItem = "";
	}
}
