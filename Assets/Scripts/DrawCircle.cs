using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts {
	class DrawCircle {
		public DrawCircle (LineRenderer lr, Vector3 position) {
			int i = 0;

			lr.SetPosition(i, position);
			i++;
		}
	}
}
