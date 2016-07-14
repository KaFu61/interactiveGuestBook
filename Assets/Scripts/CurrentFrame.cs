using UnityEngine;
using System.Collections;

// Get the contents of a RenderTexture into a Texture2D
public class CurrentFrame : MonoBehaviour {
	static public Texture2D GetRTPixels(RenderTexture rt) {

		// Remember currently active render texture
		RenderTexture currentActiveRT = RenderTexture.active;

		// Set the supplied RenderTexture as the active one
		RenderTexture.active = rt;

		// Create a new Texture2D and read the RenderTexture image into it
		Texture2D tex = new Texture2D(rt.width, rt.height);
		tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

		// Restorie previously active render texture
		RenderTexture.active = currentActiveRT;
		return tex;
	}
}