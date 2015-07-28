using UnityEngine;
using System.Collections.Generic;
/* This is an example script that demonstrates the usage of connection tags to follow a certain connection
 * 
 */


public class ConnectionWalkerControl : MonoBehaviour {
    public SplineWalkerCon Walker;

    int mDirection;
    int mPreferredTrack;
	
	// Update is called once per frame
	void Update () {
        // if we travel a closed spline, don't stop!
        if (Walker && Walker.Spline) {
            Walker.Clamping = (Walker.Spline.Closed) ? CurvyClamping.Loop : CurvyClamping.Clamp;
        }
	}

    void OnGUI()
    {
        if (!Walker)
            return;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Movement: ");
        mDirection = GUILayout.Toolbar(mDirection, new string[] { "Forward", "Backward" });
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Follow Track: ");
        mPreferredTrack = GUILayout.Toolbar(mPreferredTrack, new string[] { "Main", "Upper", "Lower" });
        GUILayout.EndHorizontal();

        // Set movement direction
        Walker.Forward=(mDirection==0);
        // Add tags depending on which track we want to follow
        switch (mPreferredTrack){
            case 0: Walker.AdditionalTags ="MainTrack";break;
            case 1:Walker.AdditionalTags="UpperTrack";break;
            case 2:Walker.AdditionalTags="LowerTrack";break;
        }
        // We need to allow the way back to MainTrack even if the PreferredTrack changes
        if (Walker.Spline.name=="Main")
            Walker.MinTagMatches = 2;
        else
            Walker.MinTagMatches = 1;
        

        GUILayout.Label("Current active Tags: "+string.Join(" ",Walker.ResultingTags));
    }

}
