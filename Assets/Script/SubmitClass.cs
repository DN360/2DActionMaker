using UnityEngine;
using System.Collections;

public class SubmitClass : MonoBehaviour {

	public static string MapText = "";
	public UnityEngine.TextAsset mapText;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (mapText != null) {
			MapText = @"
OffsetX=0
OffsetY=0
CellX=0.64
CellY=0.64
SingleDisp=1
#Map
" + mapText.text + "\n#End";
			this.transform.parent.FindChild ("InputField").transform.FindChild ("MapInput").GetComponent<UnityEngine.UI.Text> ().text = mapText.text;
		}
	}

	public void OnClick() {
		UnityEngine.SceneManagement.SceneManager.LoadScene ("GenerateMapTest");
	}
}
