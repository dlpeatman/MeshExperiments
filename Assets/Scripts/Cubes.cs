using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private Mesh mesh;


    private int width;
    private int length;
    private float xNoiseOffset;
    private float zNoiseOffset;
    private float scale;

    public int Width { get => width; }
    public int Length { get => length; }
    public float XNoiseOffset { get => xNoiseOffset; set => xNoiseOffset = value; }
    public float ZNoiseOffset { get => zNoiseOffset; set => zNoiseOffset = value; }
    public float Scale { get => scale; set => scale = value; }

    public Cubes(int width, int length, float xNoiseOffset, float zNoiseOffset, float scale) {
        Reset(width, length, xNoiseOffset, zNoiseOffset, scale);
    }

    void Start() {
        Generate();
    }

    void Update() {
        Generate();
    }

    public void Reset(int width, int length, float xNoiseOffset, float zNoiseOffset, float scale) {
        this.width = width;
        this.length = length;
        this.xNoiseOffset = xNoiseOffset;
        this.zNoiseOffset = zNoiseOffset;
        this.scale = scale;

        Generate();
    }

    public void Generate() {
        if (mesh == null) {
            mesh = new Mesh();
        } else {
            mesh.Clear();
        }
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMesh();
    }

    void CreateMesh() {
        int trianglesPerCube = 12 * 3;
        var black = new Color32(0, 0, 0, 255);
        var white = new Color32(255, 255, 255, 255);

        Vector3[] vertices = new Vector3[width * length * 8];
        Color32[] colors = new Color32[width * length * 8];
        int[] triangles = new int[width * length * trianglesPerCube];

        int cubeIndex = 0;
        for (int i = 0; i < length; i++) {
            for (int j = 0; j < width; j++, cubeIndex++) {
                Vector3 origin = new Vector3(-width / 2f + j, 0, -length / 2f + i);
                int vertexOffset = cubeIndex * 8;
                int triangleOffset = cubeIndex * trianglesPerCube;

                Vector3[] cubeVertices = CreateCubeVertices(origin);
                float perlinValue = Mathf.PerlinNoise(xNoiseOffset + j * scale, zNoiseOffset + i * scale);
                for (int x = 0; x < 8; x++) {
                    vertices[vertexOffset + x] = cubeVertices[x];
                    colors[vertexOffset + x] = Color32.Lerp(black, white, perlinValue);
                }

                int[] cubeTriangles = CreateCubeTriangles(vertexOffset);
                for (int x = 0; x < trianglesPerCube; x++) {
                    triangles[triangleOffset + x] = cubeTriangles[x];
                }
            }
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    Vector3[] CreateCubeVertices(Vector3 origin) {
        Vector3[] vertices = new Vector3[]{
            new Vector3(origin.x, origin.y, origin.z),
            new Vector3(origin.x, origin.y, origin.z+1),
            new Vector3(origin.x+1, origin.y, origin.z+1),
            new Vector3(origin.x+1, origin.y, origin.z),
            new Vector3(origin.x, origin.y+1, origin.z),
            new Vector3(origin.x, origin.y+1, origin.z+1),
            new Vector3(origin.x+1, origin.y+1, origin.z+1),
            new Vector3(origin.x+1, origin.y+1, origin.z)
        };
        return vertices;
    }

    int[] CreateCubeTriangles(int offset) {
        return new int[]{
            offset+0,offset+2,offset+1,
            offset+0,offset+3,offset+2,
            offset+0,offset+1,offset+4,
            offset+1,offset+5,offset+4,
            offset+1,offset+2,offset+5,
            offset+2,offset+6,offset+5,
            offset+2,offset+3,offset+6,
            offset+3,offset+7,offset+6,
            offset+3,offset+0,offset+7,
            offset+0,offset+4,offset+7,
            offset+4,offset+5,offset+6,
            offset+4,offset+6,offset+7
        };
    }

    // void OnDrawGizmos() {
    //     if (mesh == null) {
    //         return;
    //     }

    //     for (int j = 0; j < mesh.vertices.Length; j++) {
    //         Gizmos.DrawSphere(mesh.vertices[j], .05f);
    //     }
    // }
}
