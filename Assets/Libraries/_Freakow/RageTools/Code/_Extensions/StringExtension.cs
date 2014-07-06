//namespace Rage {
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class StringExtension {

		private const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

		/// <summary> Removes any trailing alphabetic (A-Z) characters from the string </summary>
		/// <param name="stringToTrim"> String to be trimmed </param>
		/// <returns></returns>
		public static string TrimTrailingAlphas(this string stringToTrim) {
			int trimLength = 0;
			for (int i = stringToTrim.Length - 1; i >= 0; i--) {
				char letter = stringToTrim[i];
				// Proceeds until it finds an alphabetic or percentage char
				if(!(letter.IsLetter() || letter == '%')) break;
				trimLength++;
			}

			if (trimLength > 0)
				stringToTrim = stringToTrim.Substring(0, stringToTrim.Length - trimLength);

			return stringToTrim;
		}

		/// <summary> Gets the last part from the path (after the last '/') </summary>
		public static string GetFileName(this string fileString) {
			fileString = fileString.Replace ("%20", " ");
			var pathParts = fileString.Split(new[] { '/' });
			return (pathParts.Length > 1) ? pathParts[pathParts.Length - 1] : pathParts[0];
		}

		/// <summary> Two possible formats: [ clip-path="url(#SVGID_2_)" ] and [ xlink:href="#SVGID_1_" ] </summary>
 		public static string ExtractIdFromUrl(this string urlString) {
			if (!urlString.Contains("#")) return "";
			string[] clipPathParts = urlString.Split(new[] { '#' }, 2);							//clip-path="url(#clipPath3026)"
			string afterNumberSign = clipPathParts[1];											//clipPath3026)
			if (urlString.Substring(0, 4) == "url(")
				return afterNumberSign.Substring(0, afterNumberSign.LastIndexOf(')'));			//clipPath3026
			return afterNumberSign;
 		}

		public static string[] RemoveEmptyEntries(this string[] stringArray) {
			var newArray = new List<string>();

			foreach(string t in stringArray) {
				if(t == null) continue;
				if(t == "") continue;
				newArray.Add(t);
			}

			return newArray.ToArray();
		}

		/// <summary>Used to remove all special characters and spaces from a string; Needed for material string formatting.</summary>
		public static string RemoveSpecialCharacters(this string input) {
			var r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
			return r.Replace(input, string.Empty);
		}

		public static bool IsLetter(this char value) { return Letters.Contains("" + value); }
		public static float SvgToFloat(this string svgValue) {
			float value;
			float.TryParse(svgValue.TrimTrailingAlphas(), out value);
			return value;
		}

		public static string Pad(this string text, int alignment, int displaySize) {
			switch (alignment) {
				case 0: return text.PadR(displaySize); //Left (was .PadRight)
				case 1: return text.PadR(displaySize); //Center
				case 2: return text.PadL(displaySize);  //Right
				default: return text;
			}
		}

		/// <summary> Alternative to DOT.NET's PadRight, not supported by Unity's Flash Exporter </summary>
		/// <param name="text">String to be Processed</param>
		/// <param name="width">Total Width of the Output String</param>
		public static string PadR(this string text, int width) {
			var trimmedText = text.Trim();
			if (width <= trimmedText.Length) return text;
			return trimmedText + CreatePadding(width, trimmedText);
		}

		/// <summary> Alternative to DOT.NET's PadLeft, not supported by Unity's Flash Exporter </summary>
		/// <param name="text">String to be Processed</param>
		/// <param name="width">Total Width of the Output String</param>
		public static string PadL(this string text, int width) {
			var trimmedText = text.Trim();
			if (width <= trimmedText.Length) return text;
			return CreatePadding(width, trimmedText) + trimmedText;
		}

		private static string CreatePadding(int width, string trimmedText) {
			string padding = "";
			for (int i = 0; i < width - trimmedText.Length; i++)
				padding += " ";
			return padding;
		}
	}
//}
