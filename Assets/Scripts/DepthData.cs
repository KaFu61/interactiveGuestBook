using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.IO;
using System;
using System.Collections.Generic;

public class DepthData : MonoBehaviour {
	public GameObject depth_src_man;
	private MultiSourceManager depth_manager;

	private ushort[] depths;

	private ushort width = 512;
	private ushort height = 424;

	//Angabe in mm
	private ushort screen_depth = 0;
	private byte mintreshold = 55;
	private byte maxtreshold = 150;
	private ushort right_side_limit = 497;

	private byte big_difference = 3;

	private int secondFrame = 0;

	private Color color = new Color(0,0,0);

	private Vector3 current_minval = new Vector3(0,0,0);
	private Vector3 last_minval = new Vector3 (0,0,0);

	//Create new GameObject (Circle)
	public GameObject the_circle;
	public GameObject instance;

	private float z = 900;

	//Canvas Action
	public GameObject canvas;
	private ushort wait_til_draw;
	private int frames_to_wait_canvas = 0;

	private int waiting_frames = 50;

	private bool has_drawn = false;

	private Texture2D tex;

	// Use this for initialization
	void Start() {
		if (depth_src_man == null) {
			Debug.Log("Assign GameObject with Depth Source");
		} else {
			depth_manager = depth_src_man.GetComponent<MultiSourceManager>();
			canvas.SetActive(false);
			byte[] imageFile = File.ReadAllBytes("../interactiveGuestBook/Assets/Sprites/color_wheel.jpg");
			tex = new Texture2D(1,1);
			tex.LoadImage(imageFile);

			wait_til_draw = 0;
		}
	}

	// Update is called once per frame
	void Update() {

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.P)) {
			DateTime currentDate = DateTime.Now;
			Application.CaptureScreenshot(Application.dataPath + "/screenshots/screenshot_" + currentDate.Year + currentDate.Month + currentDate.Day + currentDate.Hour + currentDate.Minute + ".png");
			Debug.Log(Application.dataPath  + "/screenshots/screenshot_" + currentDate.Year + currentDate.Month + currentDate.Day + currentDate.Hour + currentDate.Minute + ".png");
		}

		float minval = 10000;
		int minval_x = 0;
		int minval_y = 0;

		has_drawn = false;
		current_minval = new Vector3(0, 0, 0);

		if (depth_manager == null) {
			Debug.Log("depthManager = null");
			return;
		}

		depths = depth_manager.GetDepthData();

		if (wait_til_draw == 180) {
			screen_depth = getScreenDepth();
			Debug.Log("screen_depth: " + screen_depth);
			wait_til_draw = 10000;
		} else if (wait_til_draw < 180) {
			wait_til_draw++;
		}

		if (screen_depth == 0) {
			return;
		}

		if (frames_to_wait_canvas > 0) {
			frames_to_wait_canvas--;
			return;
		}
		
		for (int x = 0; x < width; x = x + 1) {
			for (int y = 0; y < height; y = y + 1) {
				if (depths[y * width + x] < (screen_depth - mintreshold) && depths[y * width + x] < minval && depths[y * width + x] != 0 && isInAllowedRange(x, y)) {

					if (canvas.activeSelf == false && (depths[y * width + x] > screen_depth - maxtreshold)) {

						if (x > right_side_limit) {
							canvas.SetActive(true);
							frames_to_wait_canvas = waiting_frames;
						}

						//check the depths of the pixel next to the current to reduce voice
						if (x < right_side_limit) {
							minval = depths[y * width + x];

							minval_x = x;
							minval_y = y;

							current_minval = new Vector3(minval_x, minval_y, minval);
						}

					} else if (canvas.activeSelf == true) {
						if (x > width - right_side_limit && x < right_side_limit) {
							color = tex.GetPixel(x, y);
							canvas.SetActive(false);
							frames_to_wait_canvas = waiting_frames;
						} else if (x < width - right_side_limit) {
							canvas.SetActive(false);
							frames_to_wait_canvas = waiting_frames;
						}
					}

				}

			}
		}

		if (frames_to_wait_canvas == 0 && current_minval != new Vector3(0,0,0)) {
			instance = (GameObject)Instantiate(the_circle, new Vector3(current_minval.x, current_minval.y, z), transform.rotation);
			instance.GetComponent<Renderer>().material.color = color;
			byte point_size = (byte)(1+ (screen_depth - mintreshold - current_minval.z) / 10);
			instance.GetComponent<Renderer>().transform.localScale = new Vector3(point_size, point_size, 1);
			z = z - 0.001f;

			has_drawn = true;

			if(last_minval != new Vector3(0,0,0)){
				Debug.Log(last_minval);
				drawLine(new Vector3(last_minval.x, last_minval.y, z), new Vector3(current_minval.x, current_minval.y, z), color, point_size);
			}

			last_minval = current_minval;
		}

		if (!has_drawn) {
			last_minval = new Vector3(0, 0, 0);
		}
		
	}

	public bool isInAllowedRange(int x, int y) {
		if (x > 0 && y > 0 && y < (height - 1) && x < (width - 1)) {
			try {
				if ((Math.Abs(depths[(y - 1) * width + (x - 1)] - depths[y * width + x])) <= big_difference
				&& (Math.Abs(depths[(y - 1) * width + x] - depths[y * width + x])) <= big_difference
				&& (Math.Abs(depths[(y - 1) * width + (x + 1)] - depths[y * width + x])) <= big_difference

				&& (Math.Abs(depths[y * width + (x - 1)] - depths[y * width + x])) <= big_difference
				&& (Math.Abs(depths[y * width + (x + 1)] - depths[y * width + x])) <= big_difference

				&& (Math.Abs(depths[(y + 1) * width + (x - 1)] - depths[y * width + x])) <= big_difference
				&& (Math.Abs(depths[(y + 1) * width + x] - depths[y * width + x])) <= big_difference
				&& (Math.Abs(depths[(y + 1) * width + (x + 1)] - depths[y * width + x])) <= big_difference) {

					return true;
				}

				return false;

			} catch (IndexOutOfRangeException e) {
				Debug.Log("IndexOutOfRangeException bei x=" + x + " y=" + y);
				Debug.Log(e.StackTrace);
			}

		}
		return false;
	}

	public ushort getScreenDepth() {
		uint depth = 0;
		uint cntr = 0;

		for (int x = 0; x < width; x = x + 1) {
			for (int y = 0; y < height; y = y + 1) {
				depth += depths[y * width + x];
				cntr++;
			}
		}

		return (ushort) (depth / cntr);
	}



	void drawLine(Vector3 start, Vector3 end, Color color, float point_size) {
		GameObject myLine = new GameObject();
		myLine.name = "line";
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Sprites/Default"));
		lr.material.color = color;
		lr.SetWidth(point_size, point_size);
		lr.SetPosition(0, start);
		lr.SetPosition(1, end);
	}
}
