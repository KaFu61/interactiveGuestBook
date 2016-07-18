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

	private int width = 512;
	private int height = 424;

	//Angabe in mm
	private float screen_depth = 0;
	private byte mintreshold = 70;
	private byte maxtreshold = 100;
	private ushort right_side_limit = 498;

	private ushort big_difference = 3;

	private int secondFrame = 0;

	private Color color = new Color(0,0,0);

	// 1 -> x, 2 -> y, 3 -> minval
	private List<Vector3> points_to_draw = new List<Vector3>();
	private int max_points_to_draw = 300;

	//Create new GameObject (Circle)
	public GameObject the_circle;
	public GameObject instance;

	//Canvas Action
	public GameObject canvas;
	private int wait_til_draw;
	private int frames_to_wait_canvas = 0;

	private int waiting_frames = 50;

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
		float minval = 10000;
		int minval_x = 0;
		int minval_y = 0;

		//delete lists content to produce a new one for this frame
		points_to_draw.Clear();

		if (depth_manager == null) {
			Debug.Log("depthManager = null");
			return;
		}

		depths = depth_manager.GetDepthData();

		if (wait_til_draw == 180) {
			screen_depth = (depths[72397] + depths[72499] + depths[108800] + depths[144589] + depths[144691]) / 5;
			Debug.Log(screen_depth);
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

							Vector3 current_min_val = new Vector3(minval_x, minval_y, minval);
							getPointsToDraw(current_min_val);
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
		if (frames_to_wait_canvas == 0) {
			foreach (Vector3 point in points_to_draw) {
				instance = (GameObject)Instantiate(the_circle, new Vector3(point.x, point.y, 1), transform.rotation);
				instance.GetComponent<Renderer>().material.color = color;
			}
		}
		
		//Debug.Log(minval + "-" + minval_x + "-" + minval_y);
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

	public void getPointsToDraw(Vector3 currentPixel) {
		if(points_to_draw.Count == 0) {
			points_to_draw.Add(currentPixel);
		} else {
			int i = points_to_draw.Count;

			try {
				while (i > 0 && currentPixel.z < points_to_draw[i-1].z) {
					i--;
				}
			} catch (ArgumentOutOfRangeException ex) {
				Debug.Log(i);
			}
			
			if (i < max_points_to_draw) {
				points_to_draw.Insert(i, currentPixel);
			}

			if (points_to_draw.Count > max_points_to_draw) {
				points_to_draw.RemoveAt(max_points_to_draw);
			}
		}	
	}
}
