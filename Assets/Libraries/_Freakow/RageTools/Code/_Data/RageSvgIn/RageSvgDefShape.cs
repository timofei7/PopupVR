
public class RageSvgDefShape {
	public string SvgCommand;
	public bool ClipPath;
	public bool UserSpaceOnUse; // Used only for ClipPaths (value for ClipPathUnits), which also use this class

	public RageSvgDefShape() {
		SvgCommand = "";
		ClipPath = false;
 		UserSpaceOnUse = true;
	}

	public void CopyDataFrom(RageSvgDefShape src) {
		SvgCommand = src.SvgCommand;
		ClipPath = src.ClipPath;
		UserSpaceOnUse = src.UserSpaceOnUse;
	}


// clipPathUnits = "userSpaceOnUse* | objectBoundingBox"
// Defines the coordinate system for the contents of the ‘clipPath’.
// If clipPathUnits="userSpaceOnUse", the contents of the ‘clipPath’ represent values in the current user coordinate system in place at the time when the ‘clipPath’ element is referenced (i.e., the user coordinate system for the element referencing the ‘clipPath’ element via the ‘clip-path’ property).
// If clipPathUnits="objectBoundingBox", then the user coordinate system for the contents of the ‘clipPath’ element is established using the bounding box of the element to which the clipping path is applied (see Object bounding box units
}
