using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class DepthData : MonoBehaviour {
	public GameObject DepthSrcMan;
	private MultiSourceManager depthManager;

	private ushort[] depths;

	private int width = 512;
	private int height = 424;

	//Angabe in mm
	private float leinwand = 1000;
	private int treshold = 30;
	private float right_side_limit = 498;

	private Color color = new Color(0,0,0);

	// 1 -> x, 2 -> y, 3 -> minval
	//private ArrayList drawedPoints = new ArrayList();

	//Create new GameObject (Circle)
	public GameObject theCircle;
	public GameObject instance;

	//Canvas Action
	public GameObject canvas;

	private Texture2D tex;

	// Use this for initialization
	void Start() {
		if (DepthSrcMan == null) {
			Debug.Log("Assign GameObject with Depth Source");
		} else {
			depthManager = DepthSrcMan.GetComponent<MultiSourceManager>();
			canvas.SetActive(false);
			RenderTexture rt = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32);
			rt.Create();
			tex = CurrentFrame.GetRTPixels(rt);
		}
	}

	// Update is called once per frame
	void Update() {

		float minval = 10000;
		int minval_x = 0;
		int minval_y = 0;
		
		if (depthManager == null) {
			Debug.Log("depthManager = null");
			return;
		}

		depths = depthManager.GetDepthData();

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {

				if (depths[y * width + x] < (leinwand - treshold) && depths[y * width + x] != 0) {
					if (canvas.activeSelf == false && (depths[y * width + x] - depths[y * width + x - 1]) <= 50) {
						if (x > right_side_limit) {
							//color = new Color(255, 0, 0);
							canvas.SetActive(true);
						}

						//check the depths of the pixel next to the current to reduce voice
						if (x < right_side_limit) {
							minval = depths[y * width + x];

							minval_x = x;
							minval_y = y;

							Vector3 current_min_val = new Vector3(minval_x, minval_y, minval);
							instance = (GameObject)Instantiate(theCircle, new Vector3(minval_x, minval_y, 1), transform.rotation);
							instance.GetComponent<Renderer>().material.color = color;
						}

					} else {

						if (x < 200) {
							canvas.SetActive(false);
						}

						//color = tex.GetPixel(x, y);
					}

				}

			}
		}
		
		//Debug.Log(minval + "-" + minval_x + "-" + minval_y);
	}
}
