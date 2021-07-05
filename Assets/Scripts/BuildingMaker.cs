﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class BuildingMaker : InfrastructureBehaviour
{


    public Material building;

    IEnumerator Start()
    {
        while (!map.IsReady)
        {
            yield return null;
        }

        foreach (var way in map.ways.FindAll( (w) => { return w.IsBuilding && w.NodeIDs.Count > 1;  }))
        {
            GameObject go = new GameObject();
            GameObject roof = new GameObject();

            go.tag = "Building";
            roof.tag = "Roof";
            Vector3 localOrigin = GetCentre(way);
            go.transform.position = localOrigin - map.bounds.Centre;
            roof.transform.position = localOrigin - map.bounds.Centre;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshFilter mf1 = roof.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            MeshRenderer mr1 = roof.AddComponent<MeshRenderer>();

            mr.material = building;
            mr1.material = building;

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> rvectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indicies = new List<int>();

            Vector3[] roofVectors;
            int[] roofTriangles;
            Vector2[] roofUV;

            int verticesCount = way.NodeIDs.Count;

            for (int i = 0; i < verticesCount; i++)
            {
                OsmNode vertex = map.nodes[way.NodeIDs[i]];

                Vector3 low_vertex3 = new Vector3(vertex.X, way.Height, vertex.Y) - localOrigin;

                rvectors.Add(low_vertex3);
            }

            Triangulation.GetResult(rvectors, false, Vector3.up, out roofVectors, out roofTriangles, out roofUV);

            for (int i = 1; i < way.NodeIDs.Count; i++)
            {
                OsmNode p1 = map.nodes[way.NodeIDs[i - 1]];
                OsmNode p2 = map.nodes[way.NodeIDs[i]];

                Vector3 pp1 = new Vector3(p1.X, 0, p1.Y);
                Vector3 pp2 = new Vector3(p2.X, 0, p2.Y);

                Vector3 v1 = pp1 - localOrigin;
                Vector3 v2 = pp2 - localOrigin;
                Vector3 v3 = v1 + new Vector3(0, way.Height, 0);
                Vector3 v4 = v2 + new Vector3(0, way.Height, 0);

                vectors.Add(v1);
                vectors.Add(v2);
                vectors.Add(v3);
                vectors.Add(v4);

                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(1, 0));
                uvs.Add(new Vector3(0, 1));
                uvs.Add(new Vector3(1, 1));

                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);

                int idx1, idx2, idx3, idx4;
                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;
                idx2 = vectors.Count - 3;
                idx1 = vectors.Count - 4;

                // Первый треугольник v1, v3, v2
                indicies.Add(idx1);
                indicies.Add(idx3);
                indicies.Add(idx2);

                // Первый треугольник v3, v4, v2
                indicies.Add(idx3);
                indicies.Add(idx4);
                indicies.Add(idx2);

                // Первый треугольник v2, v3, v1
                indicies.Add(idx2);
                indicies.Add(idx3);
                indicies.Add(idx1);

                // Первый треугольник v2, v4, v3
                indicies.Add(idx2);
                indicies.Add(idx4);
                indicies.Add(idx3);

            }
            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indicies.ToArray();
            mf.mesh.uv = uvs.ToArray();

            mf1.mesh.vertices = roofVectors;
            mf1.mesh.RecalculateNormals();
            mf1.mesh.RecalculateBounds();
            mf1.mesh.triangles = roofTriangles;
            mf1.mesh.uv = roofUV;

            go.AddComponent<MeshCollider>();
        }
    }

    
}
