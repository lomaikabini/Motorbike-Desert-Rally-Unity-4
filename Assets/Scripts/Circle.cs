using UnityEngine;
using System.Collections;

public class Circle : MonoBehaviour {

	public Type type;
	public Material [] materials;
	private int[] points = {1000,1250,1750,2000,2250,-1,-1,-1};
	private GameData data;
	[HideInInspector]
	public bool isShow = true;
	public Game gm;
	public enum Type
	{
		blue,
		green,
		red,
		turquoise,
		white,
		collectable,
		drug,
		gold
	}

	void Start()
	{
		data = GameData.Get ();
		setType (type);
	}

	public void setType(Type ringType)
	{
		type = ringType;
		Material mat = getMaterialByType ();
		if (mat != null)
			gameObject.GetComponent<MeshRenderer> ().material = mat;
		else
			Debug.LogError("Material didn't find!");
	}

	Material getMaterialByType ()
	{
		string name = "Ring" + type.ToString ();
		for(int i = 0; i < materials.Length; i++)
		{
			if(materials[i].name == name)
			   return materials[i];
		}
		return null;
	}

	void OnTriggerEnter(Collider other) 
	{
		if(other.name.Trim() == "Body" && isShow)
		{
			isShow = false;
			StartCoroutine(hideRing());
			gm.showScore(points [(int)type],transform.GetSiblingIndex ());
		}
	}

	public void refresh()
	{
		gameObject.SetActive (true);
		isShow = true;
	}

	IEnumerator hideRing()
	{
		yield return new WaitForSeconds (0.5f);
		gameObject.SetActive (false);
		yield return null;
	}
}
