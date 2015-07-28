using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text;

public class Performance : MonoBehaviour {
    public static string Prefix;
    public static List<Result> Results = new List<Result>();
    public static int Runs = 50000;
    
    int splineSelection;
    int granularity = 20;
    bool includeDependent = true;
    bool includeGetNearestPoint = false;
    bool includeIndependent = true;
    bool includeMisc = true;

    bool running = false;

    CurvyInterpolation splineType;

    List<CurvySpline> Splines = new List<CurvySpline>();
    
    Vector2 scroll;
    

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 1000, 600));
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spline Type: ");
        splineSelection = GUILayout.SelectionGrid(splineSelection, new string[] { "Linear", "Catmul-Rom", "TCB" ,"Bezier"}, 4);
        GUILayout.EndHorizontal();

        switch (splineSelection) {
            case 0: splineType = CurvyInterpolation.Linear; Prefix = "Linear"; break;
            case 1: splineType = CurvyInterpolation.CatmulRom; Prefix = "Catmul"; break;
            case 2: splineType = CurvyInterpolation.TCB; Prefix = "TCB"; break;
            case 3: splineType = CurvyInterpolation.Bezier; Prefix = "Bezier"; break;
        }

        Prefix += " G=" + granularity;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spline Granularity: "+granularity);
        int oldGran = granularity;
        granularity=(int)GUILayout.HorizontalSlider(granularity, 1, 100);
        GUILayout.EndHorizontal();

        if (Splines.Count > 0 && (splineType != Splines[0].Interpolation || granularity!=oldGran))
            Clear();
        includeDependent = GUILayout.Toggle(includeDependent, "Run tests that vary by different Spline Types");
        includeIndependent = GUILayout.Toggle(includeIndependent, "Run tests that vary by different Granularity");
        includeMisc = GUILayout.Toggle(includeMisc, "Run tests for misc. methods (conversions, etc.)");
        includeGetNearestPoint = GUILayout.Toggle(includeGetNearestPoint, "Run CurvySpline.GetNearestPointTF() - takes some time");
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Result"))
            Results.Clear();
        GUI.enabled = !running;
        if (GUILayout.Button((running) ? "Please wait..." : "Run!"))
            StartCoroutine(RunTests());
        if (!Application.isWebPlayer) {
            GUI.enabled = Results.Count > 0;
            if (GUILayout.Button("Save as CSV"))
                SaveToCSV();
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();
        
        GUILayout.Label("* 'Create & Precalculate' as an exception is a single run only, the other values are calculated by averaging 50k runs (including time needed to get random values)!");
        GUILayout.Label("* Times are in Milliseconds");
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Test", GUILayout.Width(400));
        GUILayout.Label("2 Segments", GUILayout.Width(120));
        GUILayout.Label("10 Segments", GUILayout.Width(120));
        GUILayout.Label("50 Segments", GUILayout.Width(120));
        GUILayout.Label("100 Segments", GUILayout.Width(120));
        GUILayout.EndHorizontal();
        scroll=GUILayout.BeginScrollView(scroll);
        foreach (Result R in Results)
            R.OnGUI();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    void Clear()
    {
        foreach (CurvySpline spl in Splines)
            GameObject.Destroy(spl.gameObject);
        Splines.Clear();
    }

    IEnumerator RunTests()
    {
        running = true;
        yield return null;
        if (Splines.Count == 0) {
            // === CREATE ===
            using (var M = new Measure("Create & Precalculate")) {
                M.Start();
                Splines.Add(CreateSpline(splineType, 3, granularity)); // 2 Segs
                M.StartNext();
                Splines.Add(CreateSpline(splineType, 11, granularity)); // 10 Segs
                M.StartNext();
                Splines.Add(CreateSpline(splineType, 51, granularity)); // 50 Segs
                M.StartNext();
                Splines.Add(CreateSpline(splineType, 101, granularity)); // 100 Segs
                M.Stop();
            }
        }
        
        // === SPLINE ===
        if (includeDependent) {
            using (var M = new Measure("CurvySpline.Interpolate(TF)")) {
                M.Start();
                RunMultiple(delegate { Splines[0].Interpolate(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].Interpolate(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].Interpolate(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].Interpolate(Random.value); });
                M.Stop();
            }
            yield return null;
        }
        if (includeIndependent) {
            using (var M = new Measure("CurvySpline.InterpolateFast(TF)")) {
                M.Start();
                RunMultiple(delegate { Splines[0].InterpolateFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].InterpolateFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].InterpolateFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].InterpolateFast(Random.value); });
                M.Stop();
            }
            yield return null;
        }
        if (includeDependent) {
            using (var M = new Measure("CurvySpline.GetTangent(TF)")) {
                M.Start();
                RunMultiple(delegate { Splines[0].GetTangent(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].GetTangent(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].GetTangent(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].GetTangent(Random.value); });
                M.Stop();
            }
            yield return null;
        }
        if (includeIndependent) {
            using (var M = new Measure("CurvySpline.GetTangentFast(TF)")) {
                M.Start();
                RunMultiple(delegate { Splines[0].GetTangentFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].GetTangentFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].GetTangentFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].GetTangentFast(Random.value); });
                M.Stop();
            }
            yield return null;

            using (var M = new Measure("CurvySpline.GetOrientationFast(TF)")) {
                M.Start();
                RunMultiple(delegate { Splines[0].GetOrientationFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].GetOrientationFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].GetOrientationFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].GetOrientationFast(Random.value); });
                M.Stop();
            }
            yield return null;

            using (var M = new Measure("CurvySpline.GetOrientationUpFast(TF)")) {
                M.Start();
                RunMultiple(delegate { Splines[0].GetOrientationUpFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].GetOrientationUpFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].GetOrientationUpFast(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].GetOrientationUpFast(Random.value); });
                M.Stop();
            }
            yield return null;
        }

        if (includeMisc) {

            using (var M = new Measure("CurvySpline.DistanceToSegment")) {
                M.Start();
                RunMultiple(delegate { Splines[0].DistanceToSegment(Random.Range(0, Splines[0].Length)); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].DistanceToSegment(Random.Range(0, Splines[1].Length)); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].DistanceToSegment(Random.Range(0, Splines[2].Length)); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].DistanceToSegment(Random.Range(0, Splines[3].Length)); });
                M.Stop();
            }
            yield return null;

            using (var M = new Measure("CurvySpline.DistanceToTF")) {
                M.Start();
                RunMultiple(delegate { Splines[0].DistanceToTF(Random.Range(0, Splines[0].Length)); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].DistanceToTF(Random.Range(0, Splines[1].Length)); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].DistanceToTF(Random.Range(0, Splines[2].Length)); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].DistanceToTF(Random.Range(0, Splines[3].Length)); });
                M.Stop();
            }
            yield return null;

            using (var M = new Measure("CurvySpline.TFToSegment")) {
                M.Start();
                RunMultiple(delegate { Splines[0].TFToSegment(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].TFToSegment(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].TFToSegment(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].TFToSegment(Random.value); });
                M.Stop();
            }
            yield return null;

            using (var M = new Measure("CurvySpline.TFToDistance")) {
                M.Start();
                RunMultiple(delegate { Splines[0].TFToDistance(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].TFToDistance(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].TFToDistance(Random.value); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].TFToDistance(Random.value); });
                M.Stop();
            }
            yield return null;

            
        }
        

        // === SEGMENT ===
        if (includeDependent) {
            using (var M = new Measure("CurvySplineSegment.Interpolate(F)")) {
                M.Start();
                RunMultiple(delegate { Splines[0][0].Interpolate(Random.value); });
                M.Stop();
                M.AddLast();
                M.AddLast();
                M.AddLast();
            }
            yield return null;
        }
        if (includeIndependent) {
            using (var M = new Measure("CurvySplineSegment.InterpolateFast(F)")) {
                M.Start();
                RunMultiple(delegate { Splines[0][0].InterpolateFast(Random.value); });
                M.Stop();
                M.AddLast();
                M.AddLast();
                M.AddLast();
            }
            yield return null;
        }
        if (includeMisc) {
            using (var M = new Measure("CurvySplineSegment.DistanceToLocalF")) {
                M.Start();
                RunMultiple(delegate { Splines[0][0].DistanceToLocalF(Random.Range(0, Splines[0].Length)); });
                M.Stop();
                M.AddLast();
                M.AddLast();
                M.AddLast();
            }
            yield return null;

            using (var M = new Measure("CurvySplineSegment.LocalFToDistance")) {
                M.Start();
                RunMultiple(delegate { Splines[0][0].LocalFToDistance(Random.value); });
                M.Stop();
                M.AddLast();
                M.AddLast();
                M.AddLast();
            }
            yield return null;
            using (var M = new Measure("CurvySplineSegment.LocalFToTF")) {
                M.Start();
                RunMultiple(delegate { Splines[0][0].LocalFToTF(Random.value); });
                M.Stop();
                M.AddLast();
                M.AddLast();
                M.AddLast();
            }
            yield return null;
        }

        // === GET NEAREST ===
        if (includeGetNearestPoint) {
            using (var M = new Measure("CurvySpline.GetNearestPointTF()")) {
                M.Start();
                RunMultiple(delegate { Splines[0].GetNearestPointTF(Random.insideUnitCircle * 10); });
                M.StartNext();
                RunMultiple(delegate { Splines[1].GetNearestPointTF(Random.insideUnitCircle * 10); });
                M.StartNext();
                RunMultiple(delegate { Splines[2].GetNearestPointTF(Random.insideUnitCircle * 10); });
                M.StartNext();
                RunMultiple(delegate { Splines[3].GetNearestPointTF(Random.insideUnitCircle * 10); });
                M.Stop();
            }
        }
        running = false;
    }

    void RunMultiple(System.Action param)
    {
        for (int i=0;i<Runs;i++)
            param();
    }

    CurvySpline CreateSpline(CurvyInterpolation type, int points, int granularity)
    {
        CurvySpline spl = CurvySpline.Create();
        spl.Interpolation = type;
        spl.Granularity = granularity;
        spl.Closed = false;
        spl.ShowGizmos = false;
        spl.AutoEndTangents = true;
        for (int i = 0; i < points; i++)
            spl.Add(null, false).Position = Random.insideUnitCircle * 10;
        spl.RefreshImmediately();
        return spl;
    }

    void SaveToCSV()
    {
#if !UNITY_WEBPLAYER
        string filename = Application.dataPath + "/CurvyPerformance_" + string.Format("{0:yyyyMMdd_HHmm}", System.DateTime.Now)+".csv";
        var sb = new StringBuilder();
        sb.AppendLine("Test;2 Segments;10 Segments;50 Segments;100 Segments");
        foreach (Result R in Results){
            sb.Append(R.Name);
            foreach (double v in R.Values)
                sb.Append(string.Format(";{0:0.0000}", v));
            sb.AppendLine(";");
        }
        System.IO.File.WriteAllText(filename, sb.ToString());
        Debug.Log("File saved as '"+filename+"'");
#endif
    }
}

public class Result
{
    public string Name;
    public List<double> Values = new List<double>();

    public void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(Name, GUILayout.Width(400));
        foreach (double v in Values)
            GUILayout.Label(string.Format("{0:0.0000}", v), GUILayout.Width(120));
        GUILayout.EndHorizontal();
    }
}


public class Measure : System.IDisposable
{
    System.Diagnostics.Stopwatch T;
    Result R;

    public Measure(string name)
    {
        R = new Result() { Name = "[" + Performance.Prefix + "] " + name };
        T = new System.Diagnostics.Stopwatch();
    }

    public void Start()
    {
        T.Start();
    }

    public void StartNext()
    {
        T.Stop();
        R.Values.Add((T.ElapsedTicks/(double)System.TimeSpan.TicksPerMillisecond)/(double)Performance.Runs);
        T.Reset();
        T.Start();
    }

    public void Stop()
    {
        T.Stop();
        R.Values.Add((T.ElapsedTicks / (double)System.TimeSpan.TicksPerMillisecond)/(double)Performance.Runs);
    }

    public void AddLast()
    {
        R.Values.Add(R.Values[R.Values.Count - 1]);
    }

    #region IDisposable Member

    public void Dispose()
    {
        Performance.Results.Add(R);
    }

    #endregion
}