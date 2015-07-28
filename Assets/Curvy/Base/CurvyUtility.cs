// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Curvy.Utils
{
    /// <summary>
    /// Helper class used by mesh extrusion
    /// </summary>
    [System.Serializable]
    public class MeshInfo
    {

        // Outline
        internal EdgeLoop[] EdgeLoops = new EdgeLoop[0];
        public Vector3[] EdgeVertices = new Vector3[0];
        public Vector2[] EdgeUV = new Vector2[0];
        // full Mesh
        public Vector3[] Vertices;
        public Vector2[] UVs;
        public int[] Triangles;

        /// <summary>
        /// Total number of Vertices
        /// </summary>
        public int VertexCount { get { return Vertices.Length; } }

        public int LoopVertexCount { get { return EdgeVertices.Length; } }

        /// <summary>
        /// Number of TriIndices of all EdgeLoops
        /// </summary>
        public int LoopTriIndexCount
        {
            get
            {
                int t = 0;
                foreach (EdgeLoop L in EdgeLoops)
                    t += L.TriIndexCount;
                return t;
            }
        }

        public MeshInfo(Mesh mesh, bool calculateLoops, bool mirror)
        {

            if (mirror)
                mesh = MirrorMesh(mesh);

            Vector3[] vt = mesh.vertices;
            Vector2[] uv = mesh.uv;
            int[] tri = mesh.triangles;

            // Fill data arrays
            Vertices = new Vector3[vt.Length];
            UVs = new Vector2[uv.Length];
            Triangles = new int[tri.Length];
            vt.CopyTo(Vertices, 0);
            uv.CopyTo(UVs, 0);
            tri.CopyTo(Triangles, 0);
            if (calculateLoops) {
                // Build Edge Loops
                EdgeLoops = MeshHelper.BuildEdgeLoops(MeshHelper.BuildManifoldEdges(mesh));
                // Fill Outline Data
                EdgeVertices = new Vector3[vt.Length];
                EdgeUV = new Vector2[uv.Length];

                int minIndex = int.MaxValue;
                int vertsUsed = 0;
                foreach (EdgeLoop loop in EdgeLoops) {
                    for (int vi = 0; vi < loop.vertexCount; vi++) {
                        EdgeVertices[vertsUsed + vi] = vt[loop.vertexIndex[vi]];
                        EdgeUV[vertsUsed + vi] = uv[loop.vertexIndex[vi]];
                        minIndex = Mathf.Min(minIndex, loop.vertexIndex[vi]);
                    }
                    vertsUsed += loop.vertexCount;
                }
                // modify EdgeLoops' indices (we need to address changed indices by omitting non-outlined edges
                foreach (EdgeLoop loop in EdgeLoops)
                    loop.ShiftIndices(-minIndex);

                System.Array.Resize<Vector3>(ref EdgeVertices, vertsUsed);
                System.Array.Resize<Vector2>(ref EdgeUV, vertsUsed);
            }
        }

        public Vector3[] TRSVertices(Matrix4x4 matrix)
        {
            Vector3[] res = new Vector3[Vertices.Length];
            for (int i = 0; i < res.Length; i++)
                res[i] = matrix.MultiplyPoint3x4(Vertices[i]);
            return res;
        }

        Mesh MirrorMesh(Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            Vector2[] uv = mesh.uv;
            Vector3[] normals = mesh.normals;
            int[] tris = mesh.triangles;
            Mesh res = new Mesh();

            // Flip z-axis
            for (int i = 0; i < verts.Length; i++)
                verts[i].z *= -1;

            // Flip normals
            for (int i = 0; i < normals.Length; i++)
                normals[i] *= -1;

            for (int i = 0; i < tris.Length; i += 3) {
                int temp = tris[i + 0];
                tris[i + 0] = tris[i + 1];
                tris[i + 1] = temp;
            }

            res.vertices = verts;
            res.uv = uv;
            res.normals = normals;
            res.triangles = tris;

            return res;
        }
    }

    /// <summary>
    /// Helper class for mesh creation
    /// </summary>
    public class MeshHelper
    {
        public static void CalculateMeshTangents(Mesh mesh)
        {
            //speed up math by copying the mesh arrays
            int[] triangles = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector2[] uv = mesh.uv;
            Vector3[] normals = mesh.normals;

            //variable definitions
            int triangleCount = triangles.Length;
            int vertexCount = vertices.Length;

            Vector3[] tan1 = new Vector3[vertexCount];
            Vector3[] tan2 = new Vector3[vertexCount];

            Vector4[] tangents = new Vector4[vertexCount];

            for (long a = 0; a < triangleCount; a += 3) {
                long i1 = triangles[a + 0];
                long i2 = triangles[a + 1];
                long i3 = triangles[a + 2];

                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];

                Vector2 w1 = uv[i1];
                Vector2 w2 = uv[i2];
                Vector2 w3 = uv[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;

                float div = s1 * t2 - s2 * t1;
                float r = div == 0.0f ? 0.0f : 1.0f / div;

                Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }


            for (long a = 0; a < vertexCount; ++a) {
                Vector3 n = normals[a];
                Vector3 t = tan1[a];
                Vector3.OrthoNormalize(ref n, ref t);
                tangents[a].x = t.x;
                tangents[a].y = t.y;
                tangents[a].z = t.z;

                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }

        static Edge getNextEdge(int curVI, ref List<Edge> pool, out int nextVI)
        {
            foreach (Edge next in pool) {
                if (next.vertexIndex[0] == curVI) {
                    nextVI = next.vertexIndex[1];
                    return next;
                }
                else if (next.vertexIndex[1] == curVI) {
                    nextVI = next.vertexIndex[0];
                    return next;
                }
            }
            nextVI = -1;
            Debug.LogError("Curvy Mesh Builder: Open Edge Loop detected! Please check your StartMesh!");
            return null;
        }

        internal static EdgeLoop[] BuildEdgeLoops(Edge[] manifoldEdges)
        {
            List<EdgeLoop> loops = new List<EdgeLoop>();
            if (manifoldEdges.Length == 0) return loops.ToArray();
            List<Edge> Stock = new List<Edge>(manifoldEdges);

            List<int> Loop = new List<int>();

            int nextVI;
            Loop.Add(Stock[0].vertexIndex[0]);
            Loop.Add(Stock[0].vertexIndex[1]);
            int curVI = Stock[0].vertexIndex[1];
            Stock.RemoveAt(0);

            while (Stock.Count > 0) {
                // find edge that connects to curVI
                Edge E = getNextEdge(curVI, ref Stock, out nextVI);
                if (E == null)
                    return new EdgeLoop[0]; // Open Loop
                Loop.Add(nextVI);
                curVI = nextVI;
                Stock.Remove(E);

                if (curVI == Loop[0]) {
                    loops.Add(new EdgeLoop(Loop));
                    Loop.Clear();
                    if (Stock.Count > 0) {
                        Loop.Add(Stock[0].vertexIndex[0]);
                        Loop.Add(Stock[0].vertexIndex[1]);
                        curVI = Stock[0].vertexIndex[1];
                        Stock.RemoveAt(0);
                    }
                }
            }
            if (Loop.Count > 0)
                loops.Add(new EdgeLoop(Loop));

            return loops.ToArray();
        }

        /// Code taken from Unity's Procedural example
        internal static Edge[] BuildManifoldEdges(Mesh mesh)
        {
            // Build a edge list for all unique edges in the mesh
            Edge[] edges = BuildEdges(mesh.vertexCount, mesh.triangles);

            // We only want edges that connect to a single triangle
            ArrayList culledEdges = new ArrayList();
            foreach (Edge edge in edges) {
                if (edge.faceIndex[0] == edge.faceIndex[1]) {
                    culledEdges.Add(edge);
                }
            }

            return culledEdges.ToArray(typeof(Edge)) as Edge[];
        }

        internal static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
        {
            int maxEdgeCount = triangleArray.Length;
            int[] firstEdge = new int[vertexCount + maxEdgeCount];
            int nextEdge = vertexCount;
            int triangleCount = triangleArray.Length / 3;

            for (int a = 0; a < vertexCount; a++)
                firstEdge[a] = -1;

            // First pass over all triangles. This finds all the edges satisfying the
            // condition that the first vertex index is less than the second vertex index
            // when the direction from the first vertex to the second vertex represents
            // a counterclockwise winding around the triangle to which the edge belongs.
            // For each edge found, the edge index is stored in a linked list of edges
            // belonging to the lower-numbered vertex index i. This allows us to quickly
            // find an edge in the second pass whose higher-numbered vertex index is i.
            Edge[] edgeArray = new Edge[maxEdgeCount];

            int edgeCount = 0;
            for (int a = 0; a < triangleCount; a++) {
                int i1 = triangleArray[a * 3 + 2];
                for (int b = 0; b < 3; b++) {
                    int i2 = triangleArray[a * 3 + b];
                    if (i1 < i2) {
                        Edge newEdge = new Edge();
                        newEdge.vertexIndex[0] = i1;
                        newEdge.vertexIndex[1] = i2;
                        newEdge.faceIndex[0] = a;
                        newEdge.faceIndex[1] = a;
                        edgeArray[edgeCount] = newEdge;

                        int edgeIndex = firstEdge[i1];
                        if (edgeIndex == -1) {
                            firstEdge[i1] = edgeCount;
                        }
                        else {
                            while (true) {
                                int index = firstEdge[nextEdge + edgeIndex];
                                if (index == -1) {
                                    firstEdge[nextEdge + edgeIndex] = edgeCount;
                                    break;
                                }

                                edgeIndex = index;
                            }
                        }

                        firstEdge[nextEdge + edgeCount] = -1;
                        edgeCount++;
                    }

                    i1 = i2;
                }
            }

            // Second pass over all triangles. This finds all the edges satisfying the
            // condition that the first vertex index is greater than the second vertex index
            // when the direction from the first vertex to the second vertex represents
            // a counterclockwise winding around the triangle to which the edge belongs.
            // For each of these edges, the same edge should have already been found in
            // the first pass for a different triangle. Of course we might have edges with only one triangle
            // in that case we just add the edge here
            // So we search the list of edges
            // for the higher-numbered vertex index for the matching edge and fill in the
            // second triangle index. The maximum number of comparisons in this search for
            // any vertex is the number of edges having that vertex as an endpoint.

            for (int a = 0; a < triangleCount; a++) {
                int i1 = triangleArray[a * 3 + 2];
                for (int b = 0; b < 3; b++) {
                    int i2 = triangleArray[a * 3 + b];
                    if (i1 > i2) {
                        bool foundEdge = false;
                        for (int edgeIndex = firstEdge[i2]; edgeIndex != -1; edgeIndex = firstEdge[nextEdge + edgeIndex]) {
                            Edge edge = edgeArray[edgeIndex];
                            if ((edge.vertexIndex[1] == i1) && (edge.faceIndex[0] == edge.faceIndex[1])) {
                                edgeArray[edgeIndex].faceIndex[1] = a;
                                foundEdge = true;
                                break;
                            }
                        }

                        if (!foundEdge) {
                            Edge newEdge = new Edge();
                            newEdge.vertexIndex[0] = i1;
                            newEdge.vertexIndex[1] = i2;
                            newEdge.faceIndex[0] = a;
                            newEdge.faceIndex[1] = a;
                            edgeArray[edgeCount] = newEdge;
                            edgeCount++;
                        }
                    }

                    i1 = i2;
                }
            }

            Edge[] compactedEdges = new Edge[edgeCount];
            for (int e = 0; e < edgeCount; e++)
                compactedEdges[e] = edgeArray[e];

            return compactedEdges;
        }

        /// <summary>
        /// Creates a line mesh
        /// </summary>
        /// <param name="width">the with in units</param>
        public static Mesh CreateLineMesh(float width)
        {
            if (width <= 0) return null;
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[2] { new Vector3(-width / 2, 0, 0), new Vector3(width / 2, 0, 0) };
            mesh.uv = new Vector2[2] { new Vector2(0, 0), new Vector2(1, 0) };
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Creates a N-gon mesh
        /// </summary>
        /// <param name="n">number of edges</param>
        /// <param name="radius">radius of the N-gon</param>
        /// <param name="hollowPercent">cutout in percent (0..1)</param>
        public static Mesh CreateNgonMesh(int n, float radius, float hollowPercent)
        {
            if (n < 3) return null;
            Mesh mesh = new Mesh();
            mesh.name = "Ngon";
            Vector3[] verts;
            Vector2[] uv;
            int[] tris;
            float a = Mathf.PI * 2 / (float)n;

            if (hollowPercent == 0) {
                verts = new Vector3[n + 1];
                uv = new Vector2[n + 1];
                tris = new int[n * 3];

                verts[0] = new Vector3(0, 0, 0);
                uv[0] = new Vector2(0.5f, 0.5f);



                for (int i = 0; i < n; i++) {
                    verts[i + 1] = new Vector3(Mathf.Sin(i * a) * radius, Mathf.Cos(i * a) * radius, 0);
                    uv[i + 1] = new Vector2((1 + Mathf.Sin(i * a)) * 0.5f, (1 + Mathf.Cos(i * a)) * 0.5f);
                }

                for (int i = 0; i < n; i++) {
                    tris[i * 3] = 0;
                    tris[i * 3 + 1] = i + 1;
                    tris[i * 3 + 2] = i + 2;
                }
                tris[n * 3 - 1] = 1;
            }
            else {
                verts = new Vector3[n * 2];
                uv = new Vector2[n * 2];
                tris = new int[n * 6];

                for (int i = 0; i < n; i++) {
                    verts[i] = new Vector3(Mathf.Sin(i * a) * radius, Mathf.Cos(i * a) * radius, 0);
                    verts[i + n] = new Vector3(Mathf.Sin(i * a) * radius * hollowPercent, Mathf.Cos(i * a) * radius * hollowPercent, 0);
                    uv[i] = new Vector2((1 + Mathf.Sin(i * a)) * 0.5f, (1 + Mathf.Cos(i * a)) * 0.5f);
                    uv[i + n] = new Vector2((1 + Mathf.Sin(i * a) * hollowPercent) * 0.5f, (1 + Mathf.Cos(i * a) * hollowPercent) * 0.5f);
                }
                int t = 0;
                for (int i = 0; i < n - 1; i++) {
                    tris[t] = i;
                    tris[t + 1] = i + 1;
                    tris[t + 2] = i + n;
                    tris[t + 3] = i + n;
                    tris[t + 4] = i + 1;
                    tris[t + 5] = i + n + 1;
                    t += 6;
                }
                tris[t] = n - 1;
                tris[t + 1] = 0;
                tris[t + 2] = n - 1 + n;
                tris[t + 3] = n - 1 + n;
                tris[t + 4] = 0;
                tris[t + 5] = n;

            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uv;

            return mesh;
        }

        /// <summary>
        /// Creates a rectangular mesh
        /// </summary>
        /// <param name="width">with of the rect</param>
        /// <param name="height">height of the rect</param>
        /// <param name="hollowPercent">cutout in percent (0..1)</param>
        public static Mesh CreateRectangleMesh(float width, float height, float hollowPercent)
        {
            if (width <= 0 || height < 0) return null;
            Mesh mesh = new Mesh();

            float w2 = width / 2;
            float h2 = height / 2;

            if (hollowPercent <= 0) {

                mesh.vertices = new Vector3[4] { new Vector3(-w2, h2, 0), new Vector3(w2, h2, 0),
                                                 new Vector3(w2, -h2, 0), new Vector3(-w2, -h2, 0) };

                mesh.uv = new Vector2[4] { new Vector2(0, 1), new Vector2(1, 1),
                                       new Vector2(1, 0), new Vector2(0, 0)};
                mesh.triangles = new int[6] { 0,1,2,
                                          2,3,0};
            }
            else {
                float wh = w2 * hollowPercent;
                float hh = h2 * hollowPercent;
                float p2 = hollowPercent * 0.5f;
                mesh.vertices = new Vector3[8] { new Vector3(-w2, h2, 0), new Vector3(w2, h2, 0),
                                                 new Vector3(w2, -h2, 0), new Vector3(-w2, -h2, 0),
                                                 new Vector3(-wh, hh, 0), new Vector3(wh, hh, 0),
                                                 new Vector3(wh, -hh, 0), new Vector3(-wh, -hh, 0)};
                mesh.uv = new Vector2[8] { new Vector2(0, 1), new Vector2(1, 1),
                                           new Vector2(1, 0), new Vector2(0, 0),
                                           new Vector2(0.5f-p2, 0.5f+p2), new Vector2(0.5f+p2,0.5f+p2),
                                           new Vector2(0.5f+p2,0.5f-p2), new Vector2(0.5f-p2,0.5f-p2)};
                mesh.triangles = new int[24] { 0,1,5,
                                              5,4,0,
                                              5,1,6,
                                              1,2,6,
                                              2,7,6,
                                              7,2,3,
                                              3,4,7,
                                              3,0,4};

            }
            return mesh;
        }

        /// <summary>
        /// Builds a mesh from a spline (2D), taking interpolated points based on curvation angle
        /// </summary>
        /// <param name="spline">the spline to use</param>
        /// <param name="ignoreAxis">the axis to ignore (0=x,1=y,2=z)</param>
        /// <param name="close">True to create a mesh with triangles, False to create a vertex line mesh</param>
        /// <param name="angleDiff">the curvation angle used to interpolate points</param>
        public static Mesh CreateSplineMesh(CurvySpline spline, int ignoreAxis, bool close, float angleDiff)
        {
            float tf = 0;
            int dir = 1;
            List<Vector3> verts = new List<Vector3>();
            verts.Add(spline.Transform.worldToLocalMatrix.MultiplyPoint3x4(spline.Interpolate(0)));
            while (tf < 1) {
                verts.Add(spline.Transform.worldToLocalMatrix.MultiplyPoint3x4(spline.MoveByAngle(ref tf, ref dir, angleDiff, CurvyClamping.Clamp, 0.005f)));
            }

            return buildSplineMesh(verts.ToArray(), ignoreAxis, !close);
        }

        /// <summary>
        /// Builds a mesh from a spline's approximation points (2D)
        /// </summary>
        /// <param name="spline">the spline to use</param>
        /// <param name="ignoreAxis">the axis to ignore (0=x,1=y,2=z)</param>
        /// <param name="close">True to create a mesh with triangles, False to create a vertex line mesh</param>
        public static Mesh CreateSplineMesh(CurvySpline spline, int ignoreAxis, bool close)
        {
            Vector3[] verts = spline.GetApproximation(true);
            if (spline.Closed)
                System.Array.Resize<Vector3>(ref verts, verts.Length - 1);

            return buildSplineMesh(verts, ignoreAxis, !close);
        }

        static Mesh buildSplineMesh(Vector3[] vertices, int ignoreAxis, bool noTrisAtAll)
        {
            // Tris
            int[] Tris = (noTrisAtAll) ? new int[0] : new Triangulator(vertices, ignoreAxis).Triangulate();

            Mesh msh = new Mesh();

            msh.vertices = vertices;
            msh.triangles = Tris;
            msh.RecalculateBounds();
            // UV
            Vector2[] UV = new Vector2[vertices.Length];
            float minU, sizeU;
            float minV, sizeV;
            Bounds B = msh.bounds;
            if (ignoreAxis == 0) { // Y/Z-Axis
                minU = B.min.y; sizeU = B.size.y;
                minV = B.min.z; sizeV = B.size.z;
                for (int i = 0; i < vertices.Length; i++)
                    UV[i] = new Vector2((vertices[i].y - minU) / sizeU, (vertices[i].z - minV) / sizeV);
            }
            else if (ignoreAxis == 1) { // X/Z-Axis
                minU = B.min.x; sizeU = B.size.x;
                minV = B.min.z; sizeV = B.size.z;
                for (int i = 0; i < vertices.Length; i++)
                    UV[i] = new Vector2((vertices[i].x - minU) / sizeU, (vertices[i].z - minV) / sizeV);
            }
            else { // X/Y-Axis
                minU = B.min.x; sizeU = B.size.x;
                minV = B.min.y; sizeV = B.size.y;
                for (int i = 0; i < vertices.Length; i++)
                    UV[i] = new Vector2((vertices[i].x - minU) / sizeU, (vertices[i].y - minV) / sizeV);
            }

            msh.uv = UV;
            msh.RecalculateNormals();
            return msh;
        }

    }

    /// <summary>
    /// Curvy Utility class
    /// </summary>
    public class CurvyUtility
    {
        /// <summary>
        /// Checks if the curve is planar
        /// </summary>
        /// <param name="spline">a CurvySpline</param>
        /// <param name="ignoreAxis">returns the axis that can be ignored (0=x,1=y,2=z)</param>
        /// <returns>true if a planar axis was found</returns>
        public static bool isPlanar(CurvySpline spline, out int ignoreAxis)
        {
            bool xp, yp, zp;
            bool res = isPlanar(spline, out xp, out yp, out zp);
            if (xp)
                ignoreAxis = 0;
            else if (yp)
                ignoreAxis = 1;
            else
                ignoreAxis = 2;
            return res;
        }
        /// <summary>
        /// Checks if the curve is planar
        /// </summary>
        /// <param name="spline">a CurvySpline</param>
        /// <param name="xplanar">whether the x-axis is planar</param>
        /// <param name="yplanar">whether the y-axis is planar</param>
        /// <param name="zplanar">whether the z-axis is planar</param>
        /// <returns>true if at least on axis is planar</returns>
        public static bool isPlanar(CurvySpline spline, out bool xplanar, out bool yplanar, out bool zplanar)
        {
            xplanar = true;
            yplanar = true;
            zplanar = true;
            if (spline.ControlPointCount == 0) return false;
            Vector3 p = spline.ControlPoints[0].Position;
            for (int i = 1; i < spline.ControlPointCount; i++) {
                if (!Mathf.Approximately(spline.ControlPoints[i].Position.x, p.x))
                    xplanar = false;
                if (!Mathf.Approximately(spline.ControlPoints[i].Position.y, p.y))
                    yplanar = false;
                if (!Mathf.Approximately(spline.ControlPoints[i].Position.z, p.z))
                    zplanar = false;

                if (xplanar == false && yplanar == false && zplanar == false)
                    return false;

            }
            return true;
        }

        /// <summary>
        /// Equalize one axis of a spline to match the first control points's value
        /// </summary>
        /// <param name="spline">a CurvySpline</param>
        /// <param name="axis">the axis to equalize (0=x,1=y,2=z)</param>
        public static void makePlanar(CurvySpline spline, int axis)
        {
            Vector3 p = spline.ControlPoints[0].Position;
            for (int i = 1; i < spline.ControlPointCount; i++) {
                Vector3 pi = spline.ControlPoints[i].Position;
                switch (axis) {
                    case 0: pi.x = p.x; break;
                    case 1: pi.y = p.y; break;
                    case 2: pi.z = p.z; break;
                }
                spline.ControlPoints[i].Position = pi;
            }
            spline.Refresh();
        }

        /// <summary>
        /// Centers Control Points around Spline transform
        /// </summary>
        /// <param name="spline">The spline to address</param>
        public static void centerPivot(CurvySpline spline)
        {
            Bounds b = spline.GetBounds(false);
            Vector3 off = spline.Transform.position - b.center;

            foreach (CurvySplineSegment cp in spline.ControlPoints)
                cp.Transform.position += off;

            spline.transform.position -= off;
            spline.Refresh();
        }

        /// <summary>
        /// Grows/Shrinks a spline or a spline group
        /// </summary>
        /// <param name="spline">the spline or group</param>
        /// <param name="center">the center of the effect</param>
        /// <param name="growByPercent">the percentage to grow/shrink</param>
        internal static void Grow(CurvySplineBase spline, Vector3 center, float growByPercent)
        {
            if (spline is CurvySplineGroup) {
                CurvySplineGroup grp = (CurvySplineGroup)spline;
                foreach (CurvySpline spl in grp.Splines)
                    Grow(spl, center, growByPercent);
            }
            else {
                CurvySpline spl = (CurvySpline)spline;
                foreach (CurvySplineSegment cp in spl.ControlPoints) {
                    Vector3 dir = center - cp.Position;
                    float dist = dir.magnitude;
                    cp.Position = center + dir.normalized * dist * growByPercent;
                }
            }
        }

        /// <summary>
        /// Rearrange the spline to have a new first Control Point.
        /// </summary>
        /// <param name="newStartCP">the Control Point to become the first Control Point</param>
        public static void setFirstCP(CurvySplineSegment newStartCP)
        {
            CurvySpline spl = newStartCP.Spline;
            if (newStartCP.ControlPointIndex <= 0)
                return;

            CurvySplineSegment[] toMove = new CurvySplineSegment[newStartCP.ControlPointIndex];
            for (int i = 0; i < newStartCP.ControlPointIndex; i++)
                toMove[i] = spl.ControlPoints[i];

            foreach (CurvySplineSegment seg in toMove) {
                spl.ControlPoints.Remove(seg);
                spl.ControlPoints.Add(seg);
            }
            spl._RenameControlPointsByIndex();
            spl.RefreshImmediately(true, true, false);
        }

        /// <summary>
        /// Flips the direction of a spline, i.e. the first Control Point will become the last and vice versa.
        /// </summary>
        public static void FlipSpline(CurvySpline spline)
        {
            spline.ControlPoints.Reverse();
            spline._RenameControlPointsByIndex();
            spline.RefreshImmediately(true, true, false);
        }

        /// <summary>
        /// Splits a spline
        /// </summary>
        /// <param name="firstCP">the first Control Point of the new spline</param>
        /// <returns>the new spline</returns>
        public static CurvySpline SplitSpline(CurvySplineSegment firstCP)
        {
            CurvySpline old=firstCP.Spline;
            CurvySpline spl = CurvySpline.Create(old);
            spl.name = old.name + "_parted";

            // Move CPs
            var affected = old.ControlPoints.GetRange(firstCP.ControlPointIndex, old.ControlPointCount - firstCP.ControlPointIndex);
            for (int i = 0; i < affected.Count; i++) {
                affected[i].Transform.parent = spl.Transform;
                affected[i]._ReSettle();
            }
            old.ControlPoints.Clear();
            old.RefreshImmediately(true, true, false);
            spl._RenameControlPointsByIndex();
            spl.RefreshImmediately(true, true, false);
            return spl;
        }

        /// <summary>
        /// Join a spline by inserting all source Control Points after a destination Control Point 
        /// </summary>
        /// <param name="sourceCP">a Control Point of the source spline</param>
        /// <param name="destCP">the Control Point of the destination spline</param>
        public static void JoinSpline(CurvySplineSegment sourceCP, CurvySplineSegment destCP)
        {
            if (!sourceCP || !destCP)
                return;
            CurvySpline src = sourceCP.Spline;
            CurvySpline dst = destCP.Spline;

            if (src == dst) 
                return;
            for (int i = 0; i < src.ControlPointCount; i++) {
                src.ControlPoints[i].Transform.parent = dst.Transform;
                src.ControlPoints[i]._ReSettle();
            }
            
            dst.ControlPoints.InsertRange(destCP.ControlPointIndex+1, src.ControlPoints);
            dst._RenameControlPointsByIndex();
            dst.RefreshImmediately(true, true, false);
            src.Destroy();
        }

        /// <summary>
        /// Sets Bezier Handles by interpolating between Control Points
        /// </summary>
        /// <param name="interpolation">the interpolation to use</param>
        /// <param name="offset">the offset in F (0..1)</param>
        /// <param name="freeMoveHandles">whether Handles should be moved individually</param>
        /// <param name="controlPoints">the Control Points to set the Handles for</param>
        public static void InterpolateBezierHandles(CurvyInterpolation interpolation, float offset, bool freeMoveHandles, params CurvySplineSegment[] controlPoints)
        {
            if (controlPoints.Length == 0) return;
            offset = Mathf.Clamp01(offset);
            foreach (CurvySplineSegment cp in controlPoints) {
                cp.FreeHandles = freeMoveHandles;
                CurvySplineSegment other = cp.PreviousSegment;
                if (other)
                    cp.HandleInPosition = other.Interpolate(1 - offset, interpolation);
                else
                    cp.HandleInPosition = cp.Interpolate(0, interpolation);
                if (freeMoveHandles) {
                    if (cp.IsValidSegment)
                        cp.HandleOutPosition = cp.Interpolate(offset, interpolation);
                    else
                        cp.HandleInPosition = Vector3.zero;
                        
                }
            }
            controlPoints[0].Spline.Refresh();
        }

        /// <summary>
        /// Fixes NaN for Vector3
        /// </summary>
        /// <param name="v">a Vector3</param>
        /// <returns>the "cleaned up" vector</returns>
        public static Vector3 FixNaN(Vector3 v)
        {
            if (float.IsNaN(v.x)) {
                v.x = 0;
            }
            if (float.IsNaN(v.y)) {
                v.y = 0;
            }
            if (float.IsNaN(v.z)) {
                v.z = 0;
            }
            return v;
        }

        /// <summary>
        /// Much like HandleUtility.GetHandleSize(), but works for gizmos
        /// </summary>
        public static float GetHandleSize(Vector3 position)
        {
            float s;
            if (Camera.current.orthographic) {
                s = Camera.current.orthographicSize * 2.5f;
            }
            else {
                Plane screen = new Plane(Camera.current.transform.forward, Camera.current.transform.position);
                screen.Raycast(new Ray(position, Camera.current.transform.forward), out s);
            }
            return s;
        }

     
       
    }

    #region ### Internals ###

    /*! \cond PRIVATE */

    [System.Serializable]
    internal class Edge
    {
        public int[] vertexIndex = new int[2];
        // The index into the face.
        // (faceindex[0] == faceindex[1] means the edge connects to only one triangle)
        public int[] faceIndex = new int[2];

        public override string ToString()
        {
            return vertexIndex[0].ToString() + "-" + vertexIndex[1].ToString();
        }
    }
    
    [System.Serializable]
    internal class EdgeLoop
    {
        public int[] vertexIndex;
        public int vertexCount { get { return vertexIndex.Length-1; } }  // last vertex=first vertex
        public int TriIndexCount { get { return vertexCount * 6; } }

        public EdgeLoop(List<int>verts)
        {
            vertexIndex = verts.ToArray();
        }

        public void ShiftIndices(int by)
        {
            for (int i = 0; i < vertexIndex.Length; i++)
                vertexIndex[i] += by;
        }

        public override string ToString()
        {
            string s="";
            foreach (int v in vertexIndex)
                s += v.ToString() + ", ";
            return s;
        }
    }
    
    /*
   http://www.unifycommunity.com/wiki/index.php?title=Triangulator
   */
    internal class Triangulator
    {
        private List<Vector2> m_points = new List<Vector2>();

        public Triangulator(Vector2[] points)
        {
            m_points = new List<Vector2>(points);
        }

        public Triangulator(Vector3[] points, int ignoreAxis)
        {
            m_points = new List<Vector2>(points.Length);
            switch (ignoreAxis) {
                case 0:
                    foreach (Vector3 p in points)
                        m_points.Add(new Vector2(p.y, p.z));
                    break;
                case 1:
                    foreach (Vector3 p in points)
                        m_points.Add(new Vector2(p.x, p.z));
                    break;
                default:
                    foreach (Vector3 p in points)
                        m_points.Add(new Vector2(p.x, p.y));
                    break;
            }
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if (Area() > 0) {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for (int m = 0, v = nv - 1; nv > 2; ) {
                if ((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;

                if (Snip(u, v, w, nv, V)) {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private float Area()
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++) {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private bool Snip(int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++) {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }

    /// <summary>
    /// internal helper
    /// </summary>
    internal struct CurvyMeshSegmentInfo
    {
        public CurvySplineBase Target;
        public float TF;
        float mDistance;
        public float Distance
        {
            get
            {
                return (mDistance == -1) ? Target.TFToDistance(TF) : mDistance;
            }
        }
        public Matrix4x4 Matrix;

        public CurvyMeshSegmentInfo(SplinePathMeshBuilder mb, float tf, Vector3 scale)
        {
            Target = mb.Spline;
            TF = tf;
            mDistance = -1;

            Vector3 p = (mb.FastInterpolation) ? Target.InterpolateFast(TF) : Target.Interpolate(TF);

            if (mb.UseWorldPosition)
                Matrix = Matrix4x4.TRS(mb.Transform.InverseTransformPoint(p), Target.GetOrientationFast(TF), scale);
            else
                Matrix = Matrix4x4.TRS(Target.Transform.InverseTransformPoint(p), Target.GetOrientationFast(TF), scale);

        }

        public CurvyMeshSegmentInfo(SplinePathMeshBuilder mb, float tf, float distance, Vector3 scale)
        {
            Target = mb.Spline;
            TF = tf;
            mDistance = distance;

            Vector3 p = (mb.FastInterpolation) ? Target.InterpolateFast(TF) : Target.Interpolate(TF);

            if (mb.UseWorldPosition)
                Matrix = Matrix4x4.TRS(mb.Transform.InverseTransformPoint(p), Target.GetOrientationFast(TF), scale);
            else
                Matrix = Matrix4x4.TRS(Target.Transform.InverseTransformPoint(p), Target.GetOrientationFast(TF), scale);
        }

    }
    
    /*! \endcond */
    #endregion

   
}