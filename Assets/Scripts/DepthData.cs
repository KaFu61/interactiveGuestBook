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

	// 1 -> x, 2 -> y, 3 -> minval
	private ArrayList drawedPoints = new ArrayList();

	//Create new GameObject (Circle)
	public GameObject theCircle;
	private GameObject instance;

	//Angabe in mm
	private int treshold = 30;

	// Use this for initialization
	void Start() {
		if (DepthSrcMan == null) {
			Debug.Log("Assign GameObject with Depth Source");
		} else {
			depthManager = DepthSrcMan.GetComponent<MultiSourceManager>();
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

					//check the depths of the pixel next to the current to reduce voice
					if ((depths[y * width + x] - depths[y * width + x - 1]) <= 100) {
						minval = depths[y * width + x];

						minval_x = x;
						minval_y = y;

						Vector3 current_min_val = new Vector3(minval_x, minval_y, minval);
						instance = (GameObject)Instantiate(theCircle, new Vector3(minval_x, minval_y, 1), transform.rotation);
					}

				}

			}
		}
		
		Debug.Log(minval + "-" + minval_x + "-" + minval_y);
	}
}
