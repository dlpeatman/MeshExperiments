using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour {


    public Mesh mesh;
    private int xQuads;
    private int zQuads;
    private int xVerts;
    private int zVerts;
    private float xScale = 1;
    private float zScale = 1;
    private float xFactor = 1;
    private float zFactor = 1;
    private float xyFactor = 1;
    private float zyFactor = 1;
    private Vector3[] vertices;
    private int[] triangles;

    public int XQuads { get => xQuads; }
    public int ZQuads { get => zQuads; }
    public float XScale { get => xScale; }
    public float ZScale { get => zScale; }
    public float XFactor { get => xFactor; }
    public float ZFactor { get => zFactor; }
    public float XyFactor { get => xyFactor; }
    public float ZyFactor { get => zyFactor; }

    public MeshGenerator(Mesh mesh, int xSize, int zSize) {
        this.mesh = mesh;
        Resize(xSize, zSize, xScale, zScale, XFactor, ZFactor, xyFactor, zyFactor);
    }

    public void Resize(int xSize, int zSize, float xScale, float zScale, float xFactor, float zFactor, float xyFactor, float zyFactor) {
        this.xQuads = xSize;
        this.zQuads = zSize;
        this.xVerts = xSize + 1;
        this.zVerts = zSize + 1;
        this.xScale = xScale;
        this.zScale = zScale;
        this.xFactor = xFactor;
        this.zFactor = zFactor;
        this.xyFactor = xyFactor;
        this.zyFactor = zyFactor;
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

        CreateVertices();
        CreateTriangles();
        UpdateMesh();
    }

    void CreateVertices() {
        vertices = new Vector3[xVerts * zVerts];

        for (int z = 0; z < zVerts; z++) {
            for (int x = 0; x < xVerts; x++) {
                float xBase = xVerts / 2f - x;
                float zBase = zVerts / 2f - z;
                float xFinal = xBase * XScale * (float)Math.Pow(xFactor, Math.Abs(xBase));
                float zFinal = zBase * zScale * (float)Math.Pow(zFactor, Math.Abs(zBase));
                float xRange = (float)Math.PI / (xVerts / 2f);
                float zRange = (float)Math.PI / (zVerts / 2f);
                float xy = (float)Math.Cos(xRange * xBase) * xScale * xyFactor;
                float zy = (float)Math.Cos(zRange * zBase) * zScale * zyFactor;
                vertices[z * xVerts + x] = new Vector3(xFinal, xy + zy, zFinal);
            }
        }

        mesh.vertices = vertices;
    }

    void CreateTriangles() {
        triangles = new int[xQuads * zQuads * 6];

        int quad = 0;
        for (int z = 0; z < zQuads; z++) {
            for (int x = 0; x < xQuads; x++) {
                triangles[quad + 0] = x + z * xVerts;
                triangles[quad + 1] = x + (z + 1) * xVerts;
                triangles[quad + 2] = x + z * xVerts + 1;
                triangles[quad + 3] = x + (z + 1) * xVerts;
                triangles[quad + 4] = x + (z + 1) * xVerts + 1;
                triangles[quad + 5] = x + z * xVerts + 1;

                quad += 6;
            }
        }
    }

    void UpdateMesh() {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void OnDrawGizmos() {
        if (vertices == null) {
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
            Gizmos.DrawSphere(vertices[i], .05f);
    }
}
