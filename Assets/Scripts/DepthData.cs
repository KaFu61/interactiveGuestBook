using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.IO;

public class DepthData : MonoBehaviour {
	public GameObject DepthSrcMan;
	private MultiSourceManager depthManager;

	private ushort[] depths;

	private int width = 512;
	private int height = 424;

	//Angabe in mm
	private float screen_depth = 0;
	private int mintreshold = 60;
	private int maxtreshold = 250;
	private float right_side_limit = 498;

	private Color color = new Color(0,0,0);

	// 1 -> x, 2 -> y, 3 -> minval
	//private ArrayList drawedPoints = new ArrayList();

	//Create new GameObject (Circle)
	public GameObject theCircle;
	public GameObject instance;

	//Canvas Action
	public GameObject canvas;
	private int first_screen;

	private Texture2D tex;
	public 

	// Use this for initialization
	void Start() {
		if (DepthSrcMan == null) {
			Debug.Log("Assign GameObject with Depth Source");
		} else {
			depthManager = DepthSrcMan.GetComponent<MultiSourceManager>();
			canvas.SetActive(false);
			byte[] imageFile = File.ReadAllBytes("../interactiveGuestBook/Assets/Sprites/color_wheel.jpg");
			Debug.Log(imageFile);
			tex = new Texture2D(1,1);
			tex.LoadImage(imageFile);

			first_screen = 0;
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

		if (first_screen == 180) {
			screen_depth = (depths[72397] + depths[72499] + depths[108800] + depths[144589] + depths[144691]) / 5;
			Debug.Log(screen_depth);
			first_screen = 10000;
		}
		else if (first_screen < 180) {
			first_screen++;
		}

		if (screen_depth == 0) {
			return;
		}

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (depths[y * width + x] < (screen_depth - mintreshold) && depths[y * width + x] != 0) {
					if (canvas.activeSelf == false && (depths[y * width + x] - depths[y * width + x - 1]) <= 10 
						&& (depths[y * width + x] - depths[y * width + x - 2]) <= 10 
						&& (depths[y * width + x] - depths[y * width + x + 1]) <= 10
						&& (depths[y * width + x] > screen_depth - maxtreshold)) {
						if (x > right_side_limit) {
							//color = new Color(255, 0, 0);
							canvas.SetActive(true);
							Debug.Log("show Canvas");
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

					} else if (canvas.activeSelf == true){

						if (x < 200) {
							//canvas.SetActive(false);
						} else if ((depths[y * width + x] - depths[y * width + x - 1]) <= 10
									&& (depths[y * width + x] - depths[y * width + x - 2]) <= 10
									&& (depths[y * width + x] - depths[y * width + x + 1]) <= 10
									&& 90 < x && x < 400) {
							color = tex.GetPixel(x, y);
							Debug.Log(color);
							canvas.SetActive(false);
						}
					}

				}

			}
		}
		
		//Debug.Log(minval + "-" + minval_x + "-" + minval_y);
	}
}
