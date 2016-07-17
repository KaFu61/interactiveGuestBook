using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.IO;
using System;
using System.Collections.Generic;

public class DepthData : MonoBehaviour {
	public GameObject DepthSrcMan;
	private MultiSourceManager depthManager;

	private ushort[] depths;

	private int width = 512;
	private int height = 424;

	//Angabe in mm
	private float screen_depth = 0;
	private byte mintreshold = 60;
	private byte maxtreshold = 100;
	private ushort right_side_limit = 498;

	private ushort bigDifference = 3;

	private int secondFrame = 0;

	private Color color = new Color(0,0,0);

	// 1 -> x, 2 -> y, 3 -> minval
	private List<Vector3> pointsToDraw = new List<Vector3>();
	private int maxPointsToDraw = 200;

	//Create new GameObject (Circle)
	public GameObject theCircle;
	public GameObject instance;

	//Canvas Action
	public GameObject canvas;
	private int first_screen;

	private Texture2D tex;

	// Use this for initialization
	void Start() {
		if (DepthSrcMan == null) {
			Debug.Log("Assign GameObject with Depth Source");
		} else {
			depthManager = DepthSrcMan.GetComponent<MultiSourceManager>();
			canvas.SetActive(false);
			byte[] imageFile = File.ReadAllBytes("../interactiveGuestBook/Assets/Sprites/color_wheel.jpg");
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

		//delete lists content to produce a new one for this frame
		pointsToDraw.Clear();

		if (depthManager == null) {
			Debug.Log("depthManager = null");
			return;
		}

		depths = depthManager.GetDepthData();

		if (first_screen == 180) {
			screen_depth = (depths[72397] + depths[72499] + depths[108800] + depths[144589] + depths[144691]) / 5;
			Debug.Log(screen_depth);
			first_screen = 10000;
		} else if (first_screen < 180) {
			first_screen++;
		}

		if (screen_depth == 0) {
			return;
		}
		if (secondFrame == 0) {

			for (int x = 0; x < width; x = x + 1) {
				for (int y = 0; y < height; y = y + 1) {
					if (depths[y * width + x] < (screen_depth - mintreshold) && depths[y * width + x] < minval && depths[y * width + x] != 0) {
						
						if (canvas.activeSelf == false && isInAllowedRange(x, y) && (depths[y * width + x] > screen_depth - maxtreshold)) {

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
								getPointsToDraw(current_min_val);
							}

						} else if (canvas.activeSelf == true) {

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

			secondFrame = 0;
		} else {
			secondFrame++;
		}

		foreach (Vector3 point in pointsToDraw) {
			instance = (GameObject)Instantiate(theCircle, new Vector3(point.x, point.y, 1), transform.rotation);
			instance.GetComponent<Renderer>().material.color = color;
		}
		
		//Debug.Log(minval + "-" + minval_x + "-" + minval_y);
	}

	public bool isInAllowedRange(int x, int y) {
		if (x > 0 && y > 0 && y < (height - 1) && x < (width - 1)) {
			try {
				if ((Math.Abs(depths[(y - 1) * width + (x - 1)] - depths[y * width + x])) <= bigDifference
				&& (Math.Abs(depths[(y - 1) * width + x] - depths[y * width + x])) <= bigDifference
				&& (Math.Abs(depths[(y - 1) * width + (x + 1)] - depths[y * width + x])) <= bigDifference

				&& (Math.Abs(depths[y * width + (x - 1)] - depths[y * width + x])) <= bigDifference
				&& (Math.Abs(depths[y * width + (x + 1)] - depths[y * width + x])) <= bigDifference

				&& (Math.Abs(depths[(y + 1) * width + (x - 1)] - depths[y * width + x])) <= bigDifference
				&& (Math.Abs(depths[(y + 1) * width + x] - depths[y * width + x])) <= bigDifference
				&& (Math.Abs(depths[(y + 1) * width + (x + 1)] - depths[y * width + x])) <= bigDifference) {

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
		if(pointsToDraw.Count == 0) {
			pointsToDraw.Add(currentPixel);
		} else {
			int i = pointsToDraw.Count;

			try {
				while (i > 0 && currentPixel.z < pointsToDraw[i-1].z) {
					i--;
				}
			} catch (ArgumentOutOfRangeException ex) {
				Debug.Log(i);
			}
			
			if (i < maxPointsToDraw) {
				pointsToDraw.Insert(i, currentPixel);
			}

			if (pointsToDraw.Count > maxPointsToDraw) {
				pointsToDraw.RemoveAt(maxPointsToDraw);
			}
		}	
	}
}
