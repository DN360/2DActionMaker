using UnityEngine;
using System.Collections;

public class GeneralTrigger : MonoBehaviour {

	public static int CheckPointMax = 0;

	public int CheckPointNum = 0;

	public UnityChan2DController UC2DC;

	public enum TriggerType {
		Goal = 0,
		CheckPoint = 1,
		Out = 2,
		Start = 3,
		SpeedUp_L = 4,
		SpeedUp_U = 5,
		SpeedUp_R = 6,
		SpeedUp_D = 7,
	}

	public TriggerType thisTriggerType = TriggerType.Out;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D col) {
		if (col.transform.tag == "Player") {
			if (thisTriggerType == TriggerType.Goal)
				AutoGroundGenerator.ActionEnd = true;
			if (thisTriggerType == TriggerType.Out) {
				if (AutoGroundGenerator.m_state != UnityChan2DController.State.Invincible) {
					if (!AutoGroundGenerator.ActionEnd) 
						AutoGroundGenerator.Score = (int)(0.8f * AutoGroundGenerator.Score);
					AutoGroundGenerator.Out = true;
				}
			}
			if (thisTriggerType == TriggerType.CheckPoint)
				if (!AutoGroundGenerator.CheckPointThroughList.Exists (x => x == this.transform.name)) {
					AutoGroundGenerator.CheckPointThroughList.Add (this.transform.name);
					AutoGroundGenerator.RespornPoint = this.transform.position;
					AutoGroundGenerator.Score += 1000;
			}
		}
	}


	void OnTriggerStay2D(Collider2D col) {
		if (col.transform.tag == "Player") {
			float boost = 2f;
			if (thisTriggerType == TriggerType.SpeedUp_L) {
				UC2DC.m_rigidbody2D.velocity = new Vector2 (UC2DC.m_rigidbody2D.velocity.x, 0f);
				UC2DC.Boost (-boost, 0f);
			}
			if (thisTriggerType == TriggerType.SpeedUp_R) {
				UC2DC.m_rigidbody2D.velocity = new Vector2 (UC2DC.m_rigidbody2D.velocity.x, 0f);
				UC2DC.Boost (boost, 0f);
			}
			if (thisTriggerType == TriggerType.SpeedUp_U) {
				UC2DC.Boost (0, boost);
			}
			if (thisTriggerType == TriggerType.SpeedUp_D) {
				UC2DC.Boost (0, -boost);
			}
		}
	}


}
