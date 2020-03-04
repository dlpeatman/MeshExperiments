using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private struct CubePoint {
        public Vector3 vertex;
        public float value;

        public CubePoint(Vector3 vertex, float value) {
            this.vertex = vertex;
            this.value = value;
        }
    }

    private Mesh mesh;

    private bool drawGizmos;
    private int width;
    private int length;
    private int height;
    private float xNoiseOffset;
    private float yNoiseOffset;
    private float zNoiseOffset;
    private float scale;
    private float isoLevel;

    public bool DrawGizmos { get => drawGizmos; set => drawGizmos = value; }
    public int Width { get => width; }
    public int Length { get => length; }
    public int Height { get => height; set => height = value; }
    public float XNoiseOffset { get => xNoiseOffset; set => xNoiseOffset = value; }
    public float YNoiseOffset { get => yNoiseOffset; set => yNoiseOffset = value; }
    public float ZNoiseOffset { get => zNoiseOffset; set => zNoiseOffset = value; }
    public float Scale { get => scale; set => scale = value; }
    public float IsoLevel { get => isoLevel; set => isoLevel = value; }

    public Cubes(bool drawGizmos, int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
        Reset(drawGizmos, width, length, height, xNoiseOffset, yNoiseOffset, zNoiseOffset, scale, isoLevel);
    }

    void Start() {
        Generate();
    }

    void Update() {
        Generate();
    }

    public void Reset(bool drawGizmos, int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
        this.drawGizmos = drawGizmos;
        this.width = width;
        this.length = length;
        this.Height = height;
        this.xNoiseOffset = xNoiseOffset;
        this.yNoiseOffset = yNoiseOffset;
        this.zNoiseOffset = zNoiseOffset;
        this.scale = scale;
        this.isoLevel = isoLevel;

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
        int trianglesPerCube = 4 * 3;
        var black = new Color32(0, 0, 0, 255);
        var white = new Color32(255, 255, 255, 255);

        Vector3[] vertices = new Vector3[width * length * height * 12];
        // Color32[] colors = new Color32[width * length * height * 8];
        int[] triangles = new int[width * length * height * trianglesPerCube];

        int cube = 0;
        int currentTriangle = 0;
        for (int y = 0; y < height; y++) {
            for (int z = 0; z < length; z++) {
                for (int x = 0; x < width; x++, cube++) {
                    Vector3 origin = new Vector3(-width / 2f + x, -height / 2f + y, -length / 2f + z);
                    int vertexOffset = cube * 12;

                    Vector3[] cubeVertices = CreateCubeVertices(origin);
                    CubePoint[] cubePoints = new CubePoint[8];
                    for (int i = 0; i < 8; i++) {
                        Vector3 vertex = cubeVertices[i];
                        float perlinValue = Mathf.PerlinNoise(xNoiseOffset + (vertex.x + .1f) * scale, zNoiseOffset + (vertex.z + .1f) * scale);
                        cubePoints[i] = new CubePoint(vertex, perlinValue * (1 - vertex.y / (float)height));
                        // Debug.Log("CubePoint " + i + " value " + cubePoints[i].value);
                        // colors[vertexOffset + i] = Color32.Lerp(black, white, perlinValue * (1 - y / (float)height));
                    }

                    int cubeIndex = 0;
                    if (cubePoints[0].value < isoLevel) cubeIndex |= 1;
                    if (cubePoints[1].value < isoLevel) cubeIndex |= 2;
                    if (cubePoints[2].value < isoLevel) cubeIndex |= 4;
                    if (cubePoints[3].value < isoLevel) cubeIndex |= 8;
                    if (cubePoints[4].value < isoLevel) cubeIndex |= 16;
                    if (cubePoints[5].value < isoLevel) cubeIndex |= 32;
                    if (cubePoints[6].value < isoLevel) cubeIndex |= 64;
                    if (cubePoints[7].value < isoLevel) cubeIndex |= 128;

                    /* Cube is entirely in/out of the surface */
                    int indexValue = Triangulation.edgeTable[cubeIndex];
                    if (indexValue == 0) {
                        continue;
                    }

                    /* Find the vertices where the surface intersects the cube */
                    if ((indexValue & 1) > 0)
                        vertices[vertexOffset + 0] = Interpolate(isoLevel, cubePoints[0], cubePoints[1]);
                    if ((indexValue & 2) > 0)
                        vertices[vertexOffset + 1] = Interpolate(isoLevel, cubePoints[1], cubePoints[2]);
                    if ((indexValue & 4) > 0)
                        vertices[vertexOffset + 2] = Interpolate(isoLevel, cubePoints[2], cubePoints[3]);
                    if ((indexValue & 8) > 0)
                        vertices[vertexOffset + 3] = Interpolate(isoLevel, cubePoints[3], cubePoints[0]);
                    if ((indexValue & 16) > 0)
                        vertices[vertexOffset + 4] = Interpolate(isoLevel, cubePoints[4], cubePoints[5]);
                    if ((indexValue & 32) > 0)
                        vertices[vertexOffset + 5] = Interpolate(isoLevel, cubePoints[5], cubePoints[6]);
                    if ((indexValue & 64) > 0)
                        vertices[vertexOffset + 6] = Interpolate(isoLevel, cubePoints[6], cubePoints[7]);
                    if ((indexValue & 128) > 0)
                        vertices[vertexOffset + 7] = Interpolate(isoLevel, cubePoints[7], cubePoints[4]);
                    if ((indexValue & 256) > 0)
                        vertices[vertexOffset + 8] = Interpolate(isoLevel, cubePoints[0], cubePoints[4]);
                    if ((indexValue & 512) > 0)
                        vertices[vertexOffset + 9] = Interpolate(isoLevel, cubePoints[1], cubePoints[5]);
                    if ((indexValue & 1024) > 0)
                        vertices[vertexOffset + 10] = Interpolate(isoLevel, cubePoints[2], cubePoints[6]);
                    if ((indexValue & 2048) > 0)
                        vertices[vertexOffset + 11] = Interpolate(isoLevel, cubePoints[3], cubePoints[7]);

                    int j;
                    for (j = 0; Triangulation.triTable[cubeIndex, j] != -1; j += 3) {
                        triangles[currentTriangle + j] = Triangulation.triTable[cubeIndex, j] + vertexOffset;
                        triangles[currentTriangle + j + 1] = Triangulation.triTable[cubeIndex, j + 1] + vertexOffset;
                        triangles[currentTriangle + j + 2] = Triangulation.triTable[cubeIndex, j + 2] + vertexOffset;
                    }
                    currentTriangle += j;
                }
            }
        }

        mesh.vertices = vertices;
        // mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    Vector3 Interpolate(float isoLevel, CubePoint p1, CubePoint p2) {
        if (Mathf.Abs(isoLevel - p1.value) < 0.00001) {
            return p1.vertex;
        } else if (Mathf.Abs(isoLevel - p2.value) < 0.00001) {
            return p2.vertex;
        } else if (Mathf.Abs(p1.value - p2.value) < 0.00002) {
            return p1.vertex;
        } else {
            float mu = (isoLevel - p1.value) / (p2.value - p1.value);
            float x = p1.vertex.x + mu * (p2.vertex.x - p1.vertex.x);
            float y = p1.vertex.y + mu * (p2.vertex.y - p1.vertex.y);
            float z = p1.vertex.z + mu * (p2.vertex.z - p1.vertex.z);
            return new Vector3(x, y, z);
        }
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
        if (mesh == null || !drawGizmos) {
            return;
        }

        for (int j = 0; j < mesh.vertices.Length; j++) {
            Gizmos.DrawSphere(mesh.vertices[j], .05f);
        }
    }
}
