using UnityEngine;
using System.Collections;
using Windows.Kinect;
using Assets.Scripts;

public class DepthData : MonoBehaviour {
	public GameObject DepthSrcMan;
	private MultiSourceManager depthManager;

	private ushort[] depths;

	private int width = 512;
	private int height = 424;

	private DrawCircle point;


	//Angabe in mm
	private float leinwand = 1000;

	// 1 -> x, 2 -> y, 3 -> minval
	private ArrayList gezeichnetePunkte = new ArrayList();

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
				if (depths[y * width + x] < leinwand) {
					if (minval > depths[y * width + x] && depths[y * width + x] != 0) {
						minval = depths[y * width + x];

						minval_x = x;
						minval_y = y;
					}
				}
			}
		}

		Debug.Log(minval + "-" + minval_x + "-" + minval_y);
		Vector3 current_min_val = new Vector3(minval_x, minval_y, minval);
		LineRenderer lr = transform.GetComponent<LineRenderer>();
		point = new DrawCircle(lr, current_min_val);
		gezeichnetePunkte.Add(current_min_val);
	}
}
