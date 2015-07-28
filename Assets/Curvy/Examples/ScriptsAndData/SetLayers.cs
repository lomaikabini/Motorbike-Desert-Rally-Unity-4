using UnityEngine;
using System.Collections;
/*
 * This script maps objects to layers and setups layer's collision ignore.
 * 
 * NOTE: In a real project you want to create layers in the project settings and assign them in the editor, but that settings don't ship with packages!
 * 
 * 
 */
public class SetLayers : MonoBehaviour {
    public LayerSet[] Layers;
    public LayerIgnore[] IgnoreLayers;


	// Use this for initialization
	void Awake () {
        foreach (LayerSet set in Layers) {
            foreach (var obj in set.Objects) {
                obj.gameObject.layer = set.Layer;
            }
        }
        foreach (LayerIgnore ign in IgnoreLayers)
            Physics.IgnoreLayerCollision(ign.A, ign.B);
	}
	

}

[System.Serializable]
public class LayerSet
{
    public string Name;
    public int Layer;
    public GameObject[] Objects;
}

[System.Serializable]
public class LayerIgnore
{
    public int A;
    public int B;
}

