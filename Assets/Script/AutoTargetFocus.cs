using UnityEngine;
using System.Collections;

public class AutoTargetFocus : MonoBehaviour {

	public Transform target;
	public float lerp = 100;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate() {
		if (target != null) {
			float z = this.transform.position.z;
			Vector3 pos = this.transform.position;
			pos = Vector3.Lerp (pos, target.position, lerp / 100f);
			pos.z = z;
			this.transform.position = pos;
		}
	}
}
