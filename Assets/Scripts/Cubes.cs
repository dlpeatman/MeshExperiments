using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private Mesh mesh;


    private int width;
    private int length;

    public int Width { get => width; }
    public int Length { get => length; }

    public Cubes(int width, int length) {
        Reset(width, length);
    }

    void Start() {
        Reset(width, length);
    }

    void Update() {
        Reset(width, length);
    }

    public void Reset(int width, int length) {
        this.width = width;
        this.length = length;
        if (mesh == null) {
            mesh = new Mesh();
        } else {
            mesh.Clear();
        }
        GetComponent<MeshFilter>().mesh = mesh;

        Generate();
    }

    public void Generate() {
        CreateMesh();
    }

    public void CleanUp() {
    }

    void CreateMesh() {
        Vector3[] vertices = new Vector3[width * length * 8];
        int trianglesPerCube = 12 * 3;
        int[] triangles = new int[width * length * trianglesPerCube];

        int cubeIndex = 0;
        for (int i = 0; i < length; i++) {
            for (int j = 0; j < width; j++) {
                Vector3 origin = new Vector3(-width / 2f + j, 0, -length / 2f + i);

                Vector3[] cubeVertices = CreateCubeVertices(origin);
                for (int x = 0; x < 8; x++) {
                    vertices[cubeIndex * 8 + x] = cubeVertices[x];
                }

                int[] cubeTriangles = CreateCubeTriangles(cubeIndex * 8);
                for (int x = 0; x < trianglesPerCube; x++) {
                    triangles[cubeIndex * trianglesPerCube + x] = cubeTriangles[x];
                }

                cubeIndex++;
            }
        }
        mesh.vertices = vertices;
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

    void OnDrawGizmos() {
        if (mesh == null) {
            return;
        }

        for (int j = 0; j < mesh.vertices.Length; j++) {
            Gizmos.DrawSphere(mesh.vertices[j], .05f);
        }
    }
}
