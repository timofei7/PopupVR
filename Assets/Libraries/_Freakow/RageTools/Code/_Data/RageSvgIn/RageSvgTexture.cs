using System.Text.RegularExpressions;
using UnityEngine;

public class RageSvgTexture {

    public RageSvgTexture() {
        X = Y = Width = Height = 0f;
        TextureOffset = new Vector2();
        TextureScale = TextureAngle = 0f;
        Id = "";
    }

    public string Id = "";
    public float X;
    public float Y;
    public float Width;
    public float Height;
    public float TextureAngle;
    public Vector2 TextureOffset {
        get { return new Vector2(X, Y); }
        private set { X = value.x; Y = value.y; }
    }
    public float TextureScale;

	public float EndX {
		get { return X + Width; }
	}
	public float EndY {
		get { return Y + Height; }
	}

	public Material Material;
	public string TransformString;

	// 		var start = new Vector2 (x, -y);
	// 		var end = new Vector2 (x + width, -y - height);
	public void ProcessValues() {
		if (string.IsNullOrEmpty(TransformString))
			TextureOffset = new Vector2((X + EndX) / 2, (Y + EndY) / 2);
		else
			ApplyTextureTransform(TransformString);
		TextureScale = 1 / Width;
		Y *= -1;			// Flips Y
		Height *= -1;		// Flips Height
		//Debug.Log ("X: " + X + " EndX: " + EndX + " Y: " + Y + " End Y: " + EndY + " Width: "+Width+" Height: "+Height);
	}

	private void ApplyTextureTransform(string newTransformString) {
		//Debug.Log("Transformation String: " + newTransformString);

		Regex r = new Regex(@",\s*", RegexOptions.IgnoreCase);
		newTransformString = r.Replace(newTransformString, " ");
		var transformCommand = newTransformString.Split(new[] { ' ', ',', '(', ')', '\r', '\n' });
		transformCommand.RemoveEmptyEntries();

		var posOffset = Vector2.zero;
		var rotOffset = 0f;
		float scaleFactor = 1f;
		for (var i = 0; i < transformCommand.Length; i++) {
			if (transformCommand[i] == "matrix") {
				ApplyTextureTransformMatrix(transformCommand);
				break;
			}
			if (transformCommand[i] == "translate") {
//  				if (DebugMeshCreation) Debug.Log("\tTransform translate: " + transformCommand[i + 1].SvgToFloat() + "," + transformCommand[i + 2].SvgToFloat());
				posOffset += new Vector2(transformCommand[i + 1].SvgToFloat(),
										 transformCommand[i + 2].SvgToFloat());
				i = i + 2;
			}
			if (transformCommand[i] == "rotate") {
// 					if (DebugMeshCreation) Debug.Log("\tTransform Rotate: " + transformCommand[i + 1].SvgToFloat());
				rotOffset += transformCommand[i + 1].SvgToFloat();
				i = i + 1;
			}
			if (transformCommand[i] == "scale") {
// 					if (DebugMeshCreation) Debug.Log("\tTransform scale: " + transformCommand[i + 1].SvgToFloat());
				scaleFactor *= transformCommand[i + 1].SvgToFloat();
				i = i + 1;
			}
		}

		TextureOffset += posOffset;
		TextureAngle  -= rotOffset;
		TextureScale  *= scaleFactor;
	}

	private void ApplyTextureTransformMatrix(string[] transformCommand) {
		// Eg.: <g transform="matrix(0.240147,0.000000,0.000000,0.240147,650.7029,5.991577)">
		// transformCommand :: [1 2 3 4 5 6]
		//                     | a  b  tx |       | a c e |
		//          Inkscape = | c  d  ty | SVG = | b d f |
		//                     | 0  0  1  |       | 0 0 1 |
		var a = transformCommand[1].SvgToFloat(); // x Scale
		var b = transformCommand[2].SvgToFloat(); // x Skew
		var c = transformCommand[3].SvgToFloat(); // y Skew
		var d = transformCommand[4].SvgToFloat(); // y Scale
		var tx = transformCommand[5].SvgToFloat();
		var ty = transformCommand[6].SvgToFloat();

		var pos = TextureOffset;
		var finalPos = new Vector2((pos.x * a) + (pos.y * c) + tx,
									(pos.x * b) + (pos.y * d) + ty);
		float xSign = Mathf.Sign(a);
		float ySign = Mathf.Sign(d);
		// from matrix(a,-b,b,a,c,d), where a and b are not both zero => rotation angle is atan2(b,a).
		var r = Mathf.Atan2(c, a) * Mathf.Rad2Deg;
		TextureAngle = xSign * ySign * r;

		// a,b,c,d,tx,ty => the scale is sx=sqrt(a^2+b^2) and sy=sqrt(c^2+d^2)
		// In SVG: a,c,b,d,e,f => the scale is sx=sqrt(a^2+c^2) and sy=sqrt(b^2+d^2)
		var sx = Mathf.Sqrt((Mathf.Pow(a, 2)) + (Mathf.Pow(c, 2)));
		//var sy = Mathf.Sqrt ((Mathf.Pow (b, 2)) + (Mathf.Pow (d, 2)));

		TextureOffset = new Vector2(finalPos.x, finalPos.y);
		TextureOffset = new Vector2((X + EndX) / 2, (Y + EndY) / 2);
		TextureScale = xSign * sx;
		//texture.TextureScale = new Vector2((xSign*sx), (ySign*sy));		//TODO: Add non-proportional scale support, once (if) it's added to RageSpline
	}

	// 	public void CopyDataFrom(RageSvgTexture src) {
	// 		Id = src.Id;
	// 		X = src.X;
	// 		Y = src.Y;
	// 		EndX = src.EndX;
	// 		EndY = src.EndY;
	// 		TextureAngle = src.TextureAngle;
	// 		Width = src.Width;
	// 		Height = src.Height;
	// 		Material = src.Material;
	// 	}
	// 
	// 	public void SetStart(Vector2 position) {
	// 		X = position.x;
	// 		Y = position.y;
	// 	}
	// 
	// 	public void SetEnd(Vector2 position) {
	// 		EndX = position.x;
	// 		EndY = position.y;
	// 	}

}
