import UnityEngine
import UnityEditor

[CustomEditor(typeof(RageGroup))]
public class RageGroupEditor (RageToolsEdit): 

	private _tweak as bool
	private _showList as bool
	private _showStyle as bool
	private _showPhysics as bool
	private _rageGroup as RageGroup
	private _newGroup as RageGroup
	private _newStyle as RageSplineStyle
	private _missingMemberItem as bool
	private _newSwitchsetName as string = ""
	private _newSwitchsetGroup as RageGroup
	private _newSwitchsetGroupId as string = ""

	//private static _forceUpdate as bool
	private _deleteButton as Texture2D = Resources.Load('deletebutton', Texture2D)
	private _worldButton as Texture2D
	private _worldButtonOn as Texture2D = Resources.Load('world', Texture2D)
	private _worldButtonOff as Texture2D = Resources.Load('worldoff', Texture2D)

	public def OnDrawInspectorHeaderLine():
		_rageGroup = target if _rageGroup == null
		GUILayout.Space (6);
		EasyToggle "Visible", _rageGroup.Visible, MaxWidth(66f)
		EasyToggle "Auto", _rageGroup.AutoRefresh, MaxWidth(45f)

		if GUILayout.Button("Reset", MaxWidth(50f), GUILayout.MinHeight(20f)):
			RegisterUndo("RageGroup: Reset")
			_rageGroup.Reset()
			
		if GUILayout.Button("Update", GUILayout.MinHeight(20f)):
			RegisterUndo("RageGroup: Update")
			_rageGroup.UpdatePathList()
			_rageGroup.AaMult = 1f
			_rageGroup.DensityMult = 1f
			

	public def OnDrawInspectorGUI():
		_rageGroup = target if _rageGroup == null			
		if _rageGroup.List == null or _rageGroup.List.Count==0:
			EasyRow:
				Warning(" * No Members. Click on 'Update' to initialize")
				
		EasyRow:
			LookLikeControls(40f)
			EasyIntField "Step", _rageGroup.UpdateStep, MaxWidth(70f)
			GUILayout.Space (3);
			EasyToggle "Draft", _rageGroup.Draft, MaxWidth(54f)
			GUILayout.Space (3);
			EasyToggle "Tweak", _rageGroup.Tweak, MaxWidth(62f)
			if _rageGroup.Tweak:
				_rageGroup.CheckForEdgetune()
			EasyToggle "Multiply", _rageGroup.Proportional, MaxWidth(90f)
		
		ShowTweakSettings()
		ShowOpacitySettings()
		AddProportionalWarningCheck()

		EasyLine 1
		EasyRow:
			GUILayout.Space (6);
			EasyToggle "Pin UVs", _rageGroup.PinUVs, MaxWidth(66f)
			EasyToggle "Optimize", _rageGroup.Optimize, MaxWidth(75f)
			if _rageGroup.Optimize:
				LookLikeControls(40f,15f)
				EasyFloatField "Angle", _rageGroup.OptimizeAngle

		EasyLine2 2
		AddRageStyleList()
		AddMemberList()
		AddIgnoreGroupList()
		EasyFoldout "Switchsets (" + _rageGroup.Switchsets.Count + ")", _rageGroup.ShowSwitchsets:
			AddSwitchsetsList()

		if ((Event.current.type == EventType.ValidateCommand and Event.current.commandName == "UndoRedoPerformed") or GUI.changed):
		  Repaint()
		  EditorUtility.SetDirty (_rageGroup)
								
	private def AddIgnoreGroupList():
		LookLikeControls(110f, 50f)
		EasyFoldout "Ignore Groups (" + _rageGroup.ExcludedGroups.Count + ")", _rageGroup.GroupExclusion:
			EasyRow:
				EasyCol:
					LookLikeControls(110f, 100f)
					for i in range(0, _rageGroup.ExcludedGroups.Count):		
						EasyRow 5:
							EasyObjectField	"", _rageGroup.ExcludedGroups[i], typeof(RageGroup)
							if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
								_rageGroup.ExcludedGroups.RemoveAt(i)
								_rageGroup.UpdatePathList()
								break
					EasyRow 5:
						LookLikeControls(15f, 10f)
						EasyObjectField	"+", _newGroup, typeof(RageGroup)
				
		if _newGroup != null:
			if (_newGroup == _rageGroup or _rageGroup.ExcludedGroups.Contains(_newGroup)):
				_newGroup = null
				Separator()
				AddMemberList()
				return			

			_rageGroup.AddExcludedGroupIfPossible(_newGroup)
			_rageGroup.UpdatePathList()
			_newGroup = null

	private def AddSwitchsetsList():
		//groupFocus as bool
		EasyRow 5f:
			LookLikeControls(20f, 60f)
			EasyTextField "Id", _newSwitchsetName //, GUILayout.MaxWidth(80)

			if GUILayout.Button("Add Switchset", GUILayout.MaxHeight(18f)): // or ((Event.current.type == EventType.KeyDown) and (Event.current.keyCode == KeyCode.Return)):
				RegisterUndo("RageGroup: Switchset Added")
				if not _rageGroup.Switchsets.ContainsKey(_newSwitchsetName):
					newSwitchset = Switchset()
					_rageGroup.AddSwitchset(_newSwitchsetName, newSwitchset)
					_newSwitchsetName = ""
					GUI.changed = true
					return

		LookLikeControls(110f, 50f)
		EasyRow 10f:
			EasyCol:
				for entry in _rageGroup.Switchsets:
					EasyRow:
						thisSwitchsetId as string = entry.Key
						thisSwitchset as Switchset = entry.Value
						show = EditorGUILayout.Foldout( thisSwitchset.ShowInInspector, thisSwitchsetId + " : ("+ thisSwitchset.Items.Count +") items")
						_rageGroup.SetShownSwitchset(thisSwitchsetId, show)
						if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
							_rageGroup.RemoveSwitchset(thisSwitchsetId)
							break
					if (thisSwitchset.ShowInInspector):
						AddSwitchsetItems(thisSwitchset)

	private def AddSwitchsetItems(switchset as Switchset):
		// Iterate the group items
		for entry in switchset.Items:
			thisGroup as RageGroup = entry.Value
			thisId as string = entry.Key
			thisActive as bool = switchset.ActiveItem == thisId
			EasyRow:
				setActive = EditorGUILayout.Toggle ("", thisActive, GUILayout.MaxWidth(20))
				if setActive:
					switchset.ActiveItem = thisId
				LookLikeControls(82f, 1f)
				EditorGUILayout.LabelField (thisId, GUILayout.MaxWidth(80))
				EasyObjectField	"", thisGroup, typeof(RageGroup), GUILayout.MaxWidth(120)
				if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MinHeight(16)):
					switchset.RemoveItem(thisId)
					break
		EasyRow:
			LookLikeControls(15f, 10f)
			EasyTextField "+", _newSwitchsetGroupId, GUILayout.MaxWidth(110)
			EasyObjectField	"", _newSwitchsetGroup, typeof(RageGroup)

// 		if (_newSwitchsetGroupId == ""):
// 			_switchsetNameMessageShown = true

		if _newSwitchsetGroup != null:
			if _newSwitchsetGroup == _rageGroup or switchset.Items.ContainsValue(_newSwitchsetGroup):
				_newSwitchsetGroup = null
				return

			if not switchset.Items.ContainsKey(_newSwitchsetGroupId):
				_rageGroup.AddSwitchsetGroup(switchset, _newSwitchsetGroupId, _newSwitchsetGroup)
			_newSwitchsetGroup = null
			_newSwitchsetGroupId = ""
		
	private def AddRageStyleList():
		_rageGroup = target if _rageGroup == null
		return if _rageGroup.Styles == null
		EasyFoldout "Styles (" + _rageGroup.Styles.Count + ")", _rageGroup.UseStyles:
			//LookLikeControls(0f,0f)		
			EasyRow 5:
				EasyCol:
					for i in range(0, _rageGroup.Styles.Count):
						GUILayout.Label("Name") if i==0
						EasyObjectField	"", _rageGroup.Styles[i], typeof(ScriptableObject), GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true)
				EasyCol:
					for i in range(0, _rageGroup.Styles.Count):	
						GUILayout.Label("Filter") if i==0
						EasyRow:
							EasyTextField "", _rageGroup.StyleNames[i], GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true)
							if GUILayout.Button(_deleteButton, GUILayout.ExpandWidth(false), GUILayout.MaxHeight(16)):
								_rageGroup.Styles.RemoveAt(i)
								_rageGroup.StyleNames.RemoveAt(i)
								break
			EasyRow 5:
				LookLikeControls(20f, 1f)	
				EasyObjectField	"+", _newStyle, typeof(ScriptableObject)									
			EasyRow:
				Separator()
			EasyRow:
				if _rageGroup.Styles.Count > 0:
					if GUILayout.Button("Apply Styles"):
						RegisterUndo("RageGroup: Apply Styles")
						_rageGroup.ApplyStyle()
					if GUILayout.Button("Apply Physics"):
						RegisterUndo("RageGroup: Apply Style Physics")
						_rageGroup.ApplyStylePhysics()
						return;
																				
		if _newStyle != null:
			_rageGroup.AddStyle(_newStyle)
			_newStyle = null

	private def ShowTweakSettings():
		return if not _rageGroup.Tweak
		LookLikeControls(70f, 10f)
		EasyRow:
			if _rageGroup.Proportional:		
				EasyFloatField "AntiAlias x", _rageGroup.AaMult, MaxWidth(120f)
				EasyToggleButton _worldButton, _worldButtonOn, _worldButtonOff, _rageGroup.LocalAaWidth, "World Coords Aa Width"
				EasyFloatField "Density x", _rageGroup.DensityMult, MinWidth(120f)
				return
				
			EasyFloatField "AntiAlias", _rageGroup.AntiAlias, MaxWidth(120f)
			EasyToggleButton _worldButton, _worldButtonOn, _worldButtonOff, _rageGroup.LocalAaWidth, "World Coords Aa Width"
			EasyIntField "Density", _rageGroup.Density, MinWidth(120f)

	private def ShowOpacitySettings():
		LookLikeControls(110f,40f)
		EasyRow:
			if _rageGroup.Proportional:	
				EasyPercent "Opacity x", _rageGroup.OpacityMult, 1
			else:
				EasyPercent "Opacity", _rageGroup.Opacity, 1

	private def AddProportionalWarningCheck():
		return if not _rageGroup.Tweak
		if _rageGroup.Proportional:
			EasyRow:
				Warning("* Proportional On. Click 'Update' when done.")


	private def AddMemberList():
		_rageGroup = target if _rageGroup == null
		EasyFoldout "Members (" + _rageGroup.List.Count + ")", _showList:
			EasyRow 5:
				//Separator()
				EasyCol:
					GUILayout.Label("Name", EasyStyles.ListTitle(), GUILayout.MaxWidth(120f))
					for item in _rageGroup.List:
						continue if item == null or item.Spline == null 
						if item.Spline.Rs == null:
							_missingMemberItem = true
							continue
						EasyRow:
							GUILayout.Label(item.Spline.GameObject.name, EasyStyles.ListItem(), GUILayout.MaxWidth(120f))
					if _missingMemberItem == true:
						_rageGroup.UpdatePathList()
						_missingMemberItem = false
				Separator()
				EasyCol:
					GUILayout.Label("Def.AA", EasyStyles.ListTitle(), GUILayout.MaxWidth(50f))
					for item in _rageGroup.List:
						continue if item == null or item.Spline == null
						EasyRow:
							GUILayout.Label(item.DefaultAa.ToString(), EasyStyles.ListItem(), GUILayout.MaxWidth(50f))
				Separator()
				EasyCol:
					GUILayout.Label("Def.Dens", EasyStyles.ListTitle(), GUILayout.MaxWidth(50f))
					for item in _rageGroup.List:
						continue if item == null or item.Spline == null
						EasyRow:
							GUILayout.Label(item.DefaultDensity.ToString(), EasyStyles.ListItem(), GUILayout.MaxWidth(50f))
			Separator()
			Separator()

	protected override def OnGuiRendered():
		_rageGroup = target if _rageGroup == null
		Repaint()
		SetDirty(_rageGroup) if GUI.changed or _rageGroup.Tweak or _rageGroup.IsRefreshing