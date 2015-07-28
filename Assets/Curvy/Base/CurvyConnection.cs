// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using System.Collections.Generic;

/// <summary>
/// Defines a connection between two Control Points
/// </summary>
[System.Serializable]
public class CurvyConnection
{
    #region ### Public Fields and Properties ###

    /// <summary>
    /// Connection Heading mode
    /// </summary>
    public enum HeadingMode
    {
        /// <summary>
        /// Head toward the targets start (negative F)
        /// </summary>
        Left = -1, 
        /// <summary>
        /// No heading
        /// </summary>
        Sharp = 0, 
        /// <summary>
        /// Head toward the targets end (positive F)
        /// </summary>
        Right = 1, 
        /// <summary>
        /// Automatically tries to avoid sharp cuts
        /// </summary>
        Auto = 2 
    }
    
    /// <summary>
    /// Connection Sync Mode
    /// </summary>
    public enum SyncMode 
    {
        /// <summary>
        /// Don't sync anything
        /// </summary>
        NoSync, 
        /// <summary>
        /// Sync Control Point Position
        /// </summary>
        SyncPos, 
        /// <summary>
        /// sync Control Point Rotation
        /// </summary>
        SyncRot, 
        /// <summary>
        /// Sync Control Point Position & Rotation
        /// </summary>
        SyncPosAndRot 
    }

    /// <summary>
    /// The Control Point that initiated the connection
    /// </summary>
    public CurvySplineSegment Owner;
    
    /// <summary>
    /// The other Control Point (Target)
    /// </summary>
    public CurvySplineSegment Other;
    
    /// <summary>
    /// Heading mode from Owner to Other
    /// </summary>
    public HeadingMode Heading;
    
    /// <summary>
    /// Synchronization mode between Owner and Other
    /// </summary>
    public SyncMode Sync;
    
    /// <summary>
    /// Space separated list of tags
    /// </summary>
    public string Tags;

    /// <summary>
    /// Whether this connection is active (i.e. Owner and Other set!)
    /// </summary>
    public bool Active 
    { 
        get
        {
            return (Owner!=null && Other != null);
        }
    }

    #endregion

    public CurvyConnection()
    {
        Owner = null;
        Other = null;
    }

    #region ### Public Methods ###

    /// <summary>
    /// Clear this connection
    /// </summary>
    public void Disconnect()
    {
        if (Active)
            Other.ConnectedBy.Remove(Owner);
        Owner = null;
        Other = null;
    }

    /// <summary>
    /// Checks if the connection is valid. If not, it will be disconnected
    /// </summary>
    public void Validate()
    {
        if (Active && !Other.ConnectedBy.Contains(Owner)) {
            Owner = null;
            Other = null;
        }
    }

    /// <summary>
    /// Triggers a refresh of both splines involved
    /// </summary>
    public void RefreshSplines()
    {
        if (Active) {
            Owner.Spline.Refresh();
            Other.Spline.Refresh();
        }
    }

    /// <summary>
    /// Sets the options of this connection
    /// </summary>
    /// <param name="me">Owner of the connection</param>
    /// <param name="other">the partner Control Point </param>
    /// <param name="heading">the heading mode</param>
    /// <param name="sync">the synchronization mode</param>
    /// <param name="tags">tags of this connection</param>
    public void Set(CurvySplineSegment me, CurvySplineSegment other, HeadingMode heading, SyncMode sync, params string[] tags)
    {
        Owner = me;
        Other = other;
        Other.ConnectedBy.Add(me);
        Heading = heading;
        Sync = sync;
        SetTags(tags);
    }

    /// <summary>
    /// Gets the other Control Point
    /// </summary>
    /// <param name="cp">a ControlPoint being part of this connection</param>
    /// <returns>Either Owner, Other or Null</returns>
    public CurvySplineSegment GetCounterpart(CurvySplineSegment cp)
    {
        if (Owner == cp)
            return Other;
        else if (Other == cp)
            return Owner;
        else
            return null;
    }

    /// <summary>
    /// Gets the part of the connection belonging to the spline
    /// </summary>
    /// <param name="spline">a spline</param>
    /// <returns>the Control Point belonging to spline that is part of this connection or null</returns>
    public CurvySplineSegment GetFromSpline(CurvySpline spline)
    {
        if (Owner && Owner.Spline == spline)
            return Owner;
        else if (Other && Other.Spline == spline)
            return Other;
        else
            return null;
    }

    /// <summary>
    /// Gets all tags as a string array
    /// </summary>
    public string[] GetTags()
    {
        return Tags.Split(' ');
    }

    /// <summary>
    /// Sets all tags
    /// </summary>
    /// <param name="tags">list of tags to set</param>
    public void SetTags(params string[] tags)
    {
        Tags = string.Join(" ", tags).Trim(' ');
    }

    /// <summary>
    /// Add one or more tags
    /// </summary>
    /// <param name="tags">list of tags to add</param>
    public void AddTags(params string[] tags)
    {
        string[] a=GetTags();
        string[] r = new string[a.Length + tags.Length];
        a.CopyTo(r, 0);
        tags.CopyTo(r, a.Length);
        SetTags(r);
    }
    /// <summary>
    /// Remove one or more tags
    /// </summary>
    /// <param name="tags">list of tags to remove</param>
    public void RemoveTags(params string[] tags)
    {
        List<string>t=new List<string>(GetTags());
        for (int i = 0; i < tags.Length; i++)
            if (t.Contains(tags[i]))
                t.Remove(tags[i]);
        SetTags(t.ToArray());
    }

    /// <summary>
    /// Returns true if at least one tag matches
    /// </summary>
    /// <param name="tags">tags to be checked</param>
    public bool Matches(params string[] tags)
    {
        if (tags.Length == 0) 
            return true;
        string r = " " + Tags + " ";
        
        for (int i=0;i<tags.Length;i++)
            if (r.Contains(" " + tags[i] + " "))
                return true;
        return false;
    }

    /// <summary>
    /// Returns all tags that matches
    /// </summary>
    /// <param name="tags">one or more tags to match</param>
    /// <returns>all tags that are present both in the connection and the tags list</returns>
    public List<string> MatchingTags(params string[] tags)
    {
        List<string> res = new List<string>();
        if (tags.Length > 0) {
            string r = " " + Tags + " ";
            for (int i = 0; i < tags.Length; i++)
                if (r.Contains(" " + tags[i] + " "))
                    res.Add(tags[i]);
        }
        return res;
    }

    #endregion

    #region ### Public Static Methods ###

    /// <summary>
    /// Returns all tags of the connections
    /// </summary>
    /// <param name="uniqueTagsOnly">whether duplicates should be removed</param>
    /// <param name="connections">one or more connections</param>
    /// <returns>all found tags</returns>
    public static List<string> GetTags(bool uniqueTagsOnly, params CurvyConnection[] connections)
    {
        List<string> res = new List<string>();
        
        for (int c=0;c<connections.Length;c++) {
            string[]tags=connections[c].Tags.Split(' ');
            if (uniqueTagsOnly) {
                for (int i=0;i<tags.Length;i++)
                    if (!res.Contains(tags[i]))
                        res.Add(tags[i]);
            } else
                res.AddRange(tags);
        }

        return res;
    }

    /// <summary>
    /// Returns the connection with the most matching tags
    /// </summary>
    /// <param name="connections">one or more connections</param>
    /// <param name="tags">one or more tags to match</param>
    /// <returns>the connection with the most matching tag count</returns>
    public static CurvyConnection GetBestMatchingConnection(List<CurvyConnection> connections, params string[] tags)
    {
        CurvyConnection res = null;
        int maxcount = 0;
        for (int i = 0; i < connections.Count; i++) {
            int cnt = connections[i].MatchingTags(tags).Count;
            if (cnt > maxcount) {
                maxcount = cnt;
                res = connections[i];
            }
        }
        return res;
    }

    #endregion
}

