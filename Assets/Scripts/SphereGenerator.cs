using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SphereGenerator : MonoBehaviour {
    public Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    private int edges = 3;
    private int layers = 2;
    private int[] layerEdges;

    public int Edges { get => edges; }
    public int Layers { get => layers; }

    public SphereGenerator(Mesh mesh, int edges, int layers) {
        this.mesh = mesh;
        Resize(edges, layers);
    }

    public void Resize(int edges, int layers) {
        this.edges = edges;
        if (layers % 2 == 0) {
            this.layers = layers;
        } else {
            this.layers = layers - 1;
        }
        Generate();
    }

    // Start is called before the first frame update
    void Start() {
        Generate();
    }

    // Update is called once per frame
    void Update() {
        UpdateMesh();
    }

    public void Generate() {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CalculateLayerEdges();
        CreateVertices();
        CreateTriangles();
        UpdateMesh();
    }

    void CalculateLayerEdges() {
        layerEdges = new int[layers - 1];

        layerEdges[0] = edges;

        for (int i = 1; i < layers / 2; i++) {
            layerEdges[i] = layerEdges[i - 1] * 2;
        }
        for (int i = layers / 2; i < layers - 1; i++) {
            layerEdges[i] = layerEdges[layers - 2 - i];
        }
    }

    void CreateVertices() {
        int totalVerts = 2;
        for (int i = 0; i < layerEdges.Length; i++) {
            totalVerts += layerEdges[i];
        }
        vertices = new Vector3[totalVerts];

        // Top vertex
        vertices[0] = new Vector3(0, 1, 0);

        float layerFraction = 2f / layers;

        int currentVert = 1;
        for (int layer = 0; layer < layers - 1; layer++) {
            float range = (float)Math.PI * 2 / layerEdges[layer];
            float fraction = 1 - layerFraction * (layer + 1);
            float y = (float)Math.Sin(Math.PI / 2 * fraction);
            float centrism = (float)Math.Cos(Math.PI / 2 * fraction);
            for (int i = 0; i < layerEdges[layer]; i++) {
                vertices[currentVert + i] = new Vector3((float)Math.Sin(i * range) * centrism, y, (float)Math.Cos(i * range) * centrism);
            }
            currentVert += layerEdges[layer];
        }

        // Bottom vertex
        vertices[vertices.Length - 1] = new Vector3(0, -1, 0);

        mesh.vertices = vertices;
    }

    void CreateTriangles() {

        int[] completeEdges = new int[2 + layerEdges.Length];
        completeEdges[0] = 0;
        completeEdges[completeEdges.Length - 1] = 0;
        for (int i = 1; i < completeEdges.Length - 1; i++) {
            completeEdges[i] = layerEdges[i - 1];
        }

        int totalFaces = 0;
        for (int i = 1; i <= layers / 2; i++) {
            totalFaces += completeEdges[i - 1];
            totalFaces += completeEdges[i];
        }
        totalFaces *= 2;
        triangles = new int[totalFaces * 3];

        // Skip first

        // For each face
        int face = 0;
        int vertexOffset = 0;

        for (int layer = 0; layer < Layers; layer++) {
            int currEdges = completeEdges[layer];
            int nextEdges = completeEdges[layer + 1];

            // Down triangles
            for (int i = 0; i < currEdges; i++) {
                int anchor = vertexOffset + i + 1;
                int bottomVert;
                if (nextEdges == 0) {
                    bottomVert = vertices.Length - 1;
                } else if (layer < layers / 2) {
                    bottomVert = anchor + currEdges + i + 1;
                } else {
                    bottomVert = anchor + (currEdges - i / 2);
                    if (i == currEdges - 1) {
                        bottomVert -= nextEdges;
                    }
                }
                triangles[face + 0] = anchor + ((i + 1) % currEdges) - i;
                triangles[face + 1] = anchor;
                triangles[face + 2] = bottomVert;
                face += 3;
            }

            // Up triangles
            for (int i = 0; i < nextEdges; i++) {
                int anchor = vertexOffset + currEdges + 1 + i;
                int topVert;
                if (layer == 0) {
                    topVert = 0;
                } else if (layer < layers / 2) {
                    topVert = anchor - (i / 2 + currEdges);
                    if (i == nextEdges - 1) {
                        topVert -= currEdges;
                    }
                } else {
                    topVert = anchor - (currEdges - i - 1);
                }
                triangles[face + 0] = topVert;
                triangles[face + 1] = anchor;
                triangles[face + 2] = anchor + ((i + 1) % nextEdges) - i;

                face += 3;
            }
            vertexOffset += currEdges;
        }

        mesh.triangles = triangles;
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }
    // void OnDrawGizmos() {
    //     if (vertices == null) {
    //         return;
    //     }

    //     for (int i = 0; i < vertices.Length; i++)
    //         Gizmos.DrawSphere(vertices[i], .05f);
    // }
}
