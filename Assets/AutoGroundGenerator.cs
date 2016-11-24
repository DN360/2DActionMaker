using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoGroundGenerator : MonoBehaviour {

	public Sprite[] Sprites = new Sprite[10];
	public string[] SpritesKeys = new string[10] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
	public ColliderType[] SpriteColliderTypes = new ColliderType[10] { ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box, ColliderType.Box};

	public TextAsset map;

	public CenterType MapCenter = CenterType.BottomLeft;

	public Vector2 CurrentPosition = new Vector2(0, 0);
	public Vector2 Scale = new Vector2 (1, 1);

	public Transform PlayerTransform;

	Vector2 Offset = new Vector2(0, 0);
	Vector2 CellSize = new Vector2 (16, 16);
	bool SingleMode = false;

	List<int[]> AlReadyColl = new List<int[]>();


	string[,] mapData = new string[128, 128];

	public enum CenterType
	{
		TopLeft = 1,
		TopRight = 2,
		BottomLeft = 3,
		BottomRight = 4
	}

	public enum ColliderType
	{
		Box = 1,
		UpTriangle = 2,
		DownTriangle = 3,
		UpRevTriangle = 4,
		DownRevTriangle = 5,
		Start = 6,
		End = 7
	}

	// Use this for initialization
	void Start () {
		//Update Maps
		UpdateMap(map);

	}
	
	// Update is called once per frame
	void Update () {
	}

	public void UpdateMap(TextAsset newMap) {
		UpdateMap (newMap.text);
	}

	public void UpdateMap(string newMap) {
		//Load Config
		string[] lines = newMap.Split ('\n');
		int c = 0;
		for (c = 0; c < lines.Length; c++) {
			string lines_nr = lines [c].Split ('\r') [0];
			string[] splits = lines_nr.Split ('=');
			if (splits.Length >= 2) {
				if (splits [0].ToLower () == "offsetx") {
					int term = 0;
					if (int.TryParse (splits [1], out term)) {
						Offset.x = term;
					}
				}
				if (splits [0].ToLower () == "offsety") {
					int term = 0;
					if (int.TryParse (splits [1], out term)) {
						Offset.y = term;
					}
				}
				if (splits [0].ToLower () == "cellx") {
					float term = 0;
					if (float.TryParse (splits [1], out term)) {
						CellSize.x = term;
					}
				}
				if (splits [0].ToLower () == "celly") {
					float term = 0;
					if (float.TryParse (splits [1], out term)) {
						CellSize.y = term;
					}
				}
				if (splits [0].ToLower () == "singledisp") {
					int term = 0;
					if (int.TryParse (splits [1], out term)) {
						SingleMode = term == 1;
					}
				}
			}
			if (lines_nr == "#Map")
				break;
		}
		c++;
		Debug.Log (string.Format ("Offset is {0}", Offset));
		Debug.Log (string.Format ("SingleDipslay is {0}", SingleMode));
		//Load mapData
		int maxLengthX = 0;
		int maxLengthY = 0;
		for (int y = c; y < lines.Length; y++) {
			string lines_nr = lines [y].Split ('\r') [0];
			if (lines_nr == "#End")
				break;
			maxLengthY++;
			string[] raw_datas;
			if (SingleMode) {
				char[] raw_datas_charArr = lines_nr.ToCharArray ();
				raw_datas = new string[raw_datas_charArr.Length];
				for (int i = 0; i < raw_datas_charArr.Length; i++) {
					raw_datas [i] = new string (new char[] { raw_datas_charArr [i] });
				}
			} else
				raw_datas = lines_nr.Split (',');
			for (int x = 0; x < raw_datas.Length; x++) {
				mapData [x, y - c] = raw_datas [x];
			}
			maxLengthX = maxLengthX < raw_datas.Length ? raw_datas.Length : maxLengthX;
		}
		//Turn Pattern By Center and Justify array length;
		string[,] turnedData = new string[maxLengthX, maxLengthY];
		int lenX = maxLengthX;
		int lenY = maxLengthY;
		switch (MapCenter) {
		case CenterType.TopLeft:
			for (int y = 0; y < lenY; y++) {
				for (int x = 0; x < lenX; x++) {
					turnedData [x, y] = mapData [x, y];
				}
			}
			break;
		case CenterType.TopRight:
			for (int y = 0; y < lenY; y++) {
				for (int x = 0; x < lenX; x++) {
					turnedData [x, y] = mapData [lenX - x - 1, y];
				}
			}
			break;
		case CenterType.BottomLeft:
			for (int y = 0; y < lenY; y++) {
				for (int x = 0; x < lenX; x++) {
					turnedData [x, y] = mapData [x, lenY - y - 1];
				}
			}
			break;
		case CenterType.BottomRight:
			for (int y = 0; y < lenY; y++) {
				for (int x = 0; x < lenX; x++) {
					turnedData [x, y] = mapData [lenX - x - 1, lenY - y - 1];
				}
			}
			break;
		}
		mapData = (string[,])turnedData.Clone ();

		//put sprites
		Vector3 lastPosition = this.transform.position;
		this.transform.position = Vector3.zero;
		for (int y = 0; y < mapData.GetLength (1); y++) {
			if (y == 0)
			if (MapCenter == CenterType.BottomLeft || MapCenter == CenterType.BottomRight)
				CurrentPosition += new Vector2 (0, CellSize.y);
			for (int x = 0; x < mapData.GetLength (0); x++) {
				string key = mapData [x, y];
				int num = GetKeyNum (key);
				Vector2 putPosition = new Vector2 (
					                      Offset.x * CellSize.x + (MapCenter == CenterType.TopLeft ? CellSize.x / 2 : MapCenter == CenterType.TopRight ? -CellSize.x / 2 : MapCenter == CenterType.BottomLeft ? CellSize.x / 2 : -CellSize.x / 2),
					                      Offset.y * CellSize.y + (MapCenter == CenterType.TopLeft ? CellSize.y / 2 : MapCenter == CenterType.TopRight ? CellSize.y / 2 : MapCenter == CenterType.BottomLeft ? -CellSize.y / 2 : -CellSize.y / 2));
				
				if (num < 0) {
					CurrentPosition += new Vector2 ((MapCenter == CenterType.TopLeft || MapCenter == CenterType.BottomLeft ? 1 : -1) * CellSize.x, 0);
					continue;
				}
				if (Sprites [num] == null) {
					//特殊
					switch (SpriteColliderTypes [num]) {
					case ColliderType.Start:
						PlayerTransform.position = new Vector3 (
							((CurrentPosition + putPosition) * Scale.x).x + (lastPosition).x,
							((CurrentPosition + putPosition) * Scale.y).y + (lastPosition).y,
							0);
						break;
					}
					CurrentPosition += new Vector2 ((MapCenter == CenterType.TopLeft || MapCenter == CenterType.BottomLeft ? 1 : -1) * CellSize.x, 0);
					continue;
				}
				Sprite child = Instantiate<Sprite> (Sprites [num]);
				GameObject childObj = new GameObject (string.Format ("map_x{0:D4}_y{1:D4}", x, y), typeof(SpriteRenderer), typeof(PolygonCollider2D));
				SpriteRenderer childRnd = childObj.GetComponent<SpriteRenderer> ();
				childRnd.sprite = child;
				childRnd.color = Color.white;
				PolygonCollider2D childCol = childObj.GetComponent<PolygonCollider2D> ();
				childCol.pathCount = 1;
				switch (SpriteColliderTypes [num]) {
				case ColliderType.Box:
					childCol.enabled = false;
					int rx = GetBlockLengthX (x, y);
					int ry = GetBlockLengthY (x, y);
					if (rx > 0) {
						if (!isAlreadyColl (x, y, 0)) {
							childCol.SetPath (0, new Vector2[] {
								new Vector2 (-CellSize.x / 2, -CellSize.y / 2),
								new Vector2 (CellSize.x * (rx - 1) + CellSize.x / 2, -CellSize.y / 2),
								new Vector2 (CellSize.x * (rx - 1) + CellSize.x / 2, CellSize.y / 2),
								new Vector2 (-CellSize.x / 2, CellSize.y / 2)
							});
							childCol.enabled = true;
							//Y方向も
							if (ry > 0) {
								if (!isAlreadyColl (x, y, 1)) {
									childCol.SetPath (0, new Vector2[] {
										new Vector2 (-CellSize.x / 2, CellSize.y * (ry - 1) + CellSize.y / 2),
										new Vector2 (CellSize.x / 2, CellSize.y * (ry - 1) + CellSize.y / 2),
										new Vector2 (CellSize.x / 2, CellSize.y / 2),
										new Vector2 (CellSize.x * (rx - 1) + CellSize.x / 2, CellSize.y / 2),
										new Vector2 (CellSize.x * (rx - 1) + CellSize.x / 2, -CellSize.y / 2),
										new Vector2 (-CellSize.x / 2, -CellSize.y / 2)
									});
									childCol.enabled = true;
								}
							}
						} else {
							if (ry > 0) {
								if (!isAlreadyColl (x, y, 1)) {
									childCol.SetPath (0, new Vector2[] {
										new Vector2 (-CellSize.x / 2, -CellSize.y / 2),
										new Vector2 (CellSize.x / 2, -CellSize.y / 2),
										new Vector2 (CellSize.x / 2, CellSize.y * (ry - 1) + CellSize.y / 2),
										new Vector2 (-CellSize.x / 2, CellSize.y * (ry - 1) + CellSize.y / 2)
									});
									childCol.enabled = true;
								}
							}
						}
					} else if (ry > 0) {
						if (!isAlreadyColl (x, y, 1)) {
							childCol.SetPath (0, new Vector2[] {
								new Vector2 (-CellSize.x / 2, -CellSize.y / 2),
								new Vector2 (CellSize.x / 2, -CellSize.y / 2),
								new Vector2 (CellSize.x / 2, CellSize.y * (ry - 1) + CellSize.y / 2),
								new Vector2 (-CellSize.x / 2, CellSize.y * (ry - 1) + CellSize.y / 2)
							});
							childCol.enabled = true;
						}
					}
					break;
				case ColliderType.UpTriangle:
					childCol.SetPath (0, new Vector2[] {
						new Vector2 (-CellSize.x / 2, -CellSize.y / 2),
						new Vector2 (CellSize.x / 2, CellSize.y / 2),
						new Vector2 (CellSize.x / 2, -CellSize.y / 2)
					});
					break;
				case ColliderType.DownTriangle:
					childCol.SetPath (0, new Vector2[] {
						new Vector2 (-CellSize.x / 2, CellSize.y / 2),
						new Vector2 (CellSize.x / 2, -CellSize.y / 2),
						new Vector2 (-CellSize.x / 2, -CellSize.y / 2)
					});
					break;
				case ColliderType.UpRevTriangle:
					childCol.SetPath (0, new Vector2[] {
						new Vector2 (-CellSize.x / 2, CellSize.y / 2),
						new Vector2 (CellSize.x / 2, CellSize.y / 2),
						new Vector2 (-CellSize.x / 2, -CellSize.y / 2)
					});
					break;
				case ColliderType.DownRevTriangle:
					childCol.SetPath (0, new Vector2[] {
						new Vector2 (-CellSize.x / 2, CellSize.y / 2),
						new Vector2 (CellSize.x / 2, CellSize.y / 2),
						new Vector2 (CellSize.x / 2, -CellSize.y / 2)
					});
					break;
				}


				childObj.transform.position = this.transform.position;
				childObj.transform.position = CurrentPosition + putPosition;
				childObj.transform.parent = this.transform;
				childObj.layer = 8;
				CurrentPosition += new Vector2 ((MapCenter == CenterType.TopLeft || MapCenter == CenterType.BottomLeft ? 1 : -1) * CellSize.x, 0);
			}
			CurrentPosition.x = 0;
			CurrentPosition += new Vector2 (0, (MapCenter == CenterType.TopLeft || MapCenter == CenterType.TopRight ? -1 : 1) * CellSize.y);
		}

		//Scaling
		this.transform.localScale = new Vector3 (Scale.x, Scale.y, 1);

		//Back to position
		this.transform.position = lastPosition;
	}

	int GetKeyNum(string k) {
		for (int n = 0; n < SpritesKeys.Length; n++) {
			if (SpritesKeys [n] == k) {
				return n;
			}
		}
		return -1;
	}

	bool isBlock(int x, int y) {
		string t = mapData [x, y];
		int num = GetKeyNum (t);
		return  !(num < 0 || Sprites [num] == null || SpriteColliderTypes[num] != ColliderType.Box);
	}

	bool isAlreadyColl(int x, int y, int type) {
		return AlReadyColl.Exists (t => t [0] == x && t [1] == y && t [2] == type);
	}

	int GetBlockLengthX(int x, int y) {
		return GetBlockLengthX (x, y, 0);
	}

	int GetBlockLengthX(int x, int y, int r) {
		if (r > 0)
			AlReadyColl.Add (new int[]{ x, y, 0 });
		if (x >= mapData.GetLength (0)) {
			//Xの限界
			return r;
		}
		if (!isBlock (x, y)) { 
			//もうブロックじゃない
			return r;
		}
		if (y == 0) {
			if (isBlock (x, y + 1)) {
				//上ブロック
				return r;
			}
		}
		if (y + 1 == mapData.GetLength (1)) {
			if (isBlock (x, y - 1)) {
				//下ブロック
				return r;
			}

		}
		if (y > 0 && y < mapData.GetLength (1)) {
			if (isBlock (x, y - 1) && isBlock (x, y + 1)) {
				//上も下もブロック
				return r;
			}
		}
		return GetBlockLengthX (x + 1, y, r + 1);
	}

	int GetBlockLengthY(int x, int y) {
		return GetBlockLengthY (x, y, 0);
	}

	int GetBlockLengthY(int x, int y, int r) {
		if (r > 0)
			AlReadyColl.Add (new int[]{ x, y, 1 });
		if (y >= mapData.GetLength (1)) {
			//Yの限界
			return r;
		}
		if (!isBlock (x, y)) { 
			//もうブロックじゃない
			return r - 1;
		}
		if (x == 0) {
			if (isBlock (x + 1, y)) {
				//右ブロック
				return r;
			}
		}
		if (x + 1 == mapData.GetLength (0)) {
			if (isBlock (x - 1, y)) {
				//左ブロック
				return r;
			}

		}
		if (x > 0 && x < mapData.GetLength (0)) {
			if (isBlock (x - 1, y) && isBlock (x + 1, y)) {
				//右も左もブロック
				return r;
			}
		}
		return GetBlockLengthY (x, y + 1, r + 1);
	}
}
