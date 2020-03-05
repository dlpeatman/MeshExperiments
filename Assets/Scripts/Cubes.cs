using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private Dictionary<string, Chunk> chunkDictionary;

    public int width;
    public int length;
    public int height;
    public float xNoiseOffset;
    public float yNoiseOffset;
    public float zNoiseOffset;
    public float scale;
    public float isoLevel;

    public int octaves;
    public float lacunarity;
    public float persistance;

    public Cubes(int width, int length, int height, float xNoiseOffset, float yNoiseOffset, float zNoiseOffset, float scale, float isoLevel) {
        Reset();
    }

    void Start() {
        Generate();
    }

    void Update() {
        Generate();
    }

    public void Reset() {
        if (chunkDictionary == null) {
            chunkDictionary = new Dictionary<string, Chunk>();
        }

        CleanUp();

        Generate();
    }

    public void CleanUp() {
        Component[] meshFilters = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFilters.Length; i++) {
            ((MeshFilter)meshFilters[i]).sharedMesh.Clear();
            meshFilters[i].gameObject.SetActive(false);
        }
    }

    public void Generate() {
        for (int z = 0; z < 3; z++) {
            for (int x = 0; x < 3; x++) {
                var name = "Chunk [" + x + ", " + z + "]";
                var location = new Vector3(x * width, 0, z * length);
                var material = GetComponent<MeshRenderer>().sharedMaterial;

                Chunk chunk;
                if (chunkDictionary.ContainsKey(name)) {
                    chunk = chunkDictionary[name];
                    if (chunk == null || chunk.GameObject == null) {
                        chunkDictionary.Remove(name);
                        var gameObject = new GameObject(name);
                        chunk = new Chunk(gameObject, material, transform);
                        chunkDictionary.Add(name, chunk);
                    }
                } else {
                    var gameObject = new GameObject(name);
                    chunk = new Chunk(gameObject, material, transform);
                    chunkDictionary.Add(name, chunk);
                }
                chunk.GameObject.SetActive(true);
                CreateMesh(location, chunk);
            }
        }
    }

    void CreateMesh(Vector3 origin, Chunk chunk) {
        var mesh = chunk.Mesh;
        mesh.Clear();

        int trianglesPerCube = 4 * 3;
        var black = new Color32(0, 0, 0, 255);
        var white = new Color32(255, 255, 255, 255);

        Vector3[] vertices = new Vector3[width * length * height * 12];
        Color32[] colors = new Color32[vertices.Length];
        int[] triangles = new int[width * length * height * trianglesPerCube];

        int cube = 0;
        int currentTriangle = 0;
        for (int y = 0; y < height; y++) {
            for (int z = 0; z < length; z++) {
                for (int x = 0; x < width; x++, cube++) {
                    Vector3 cubeOrigin = origin + new Vector3(x, y, z);
                    int vertexOffset = cube * 12;

                    Vector3[] cubeVertices = CreateCubeVertices(cubeOrigin);
                    Triangulation.CubePoint[] cubePoints = new Triangulation.CubePoint[8];
                    for (int i = 0; i < 8; i++) {
                        Vector3 vertex = cubeVertices[i];
                        float isoValue = CalculateIsoValue(vertex);
                        cubePoints[i] = new Triangulation.CubePoint(vertex, isoValue);
                    }

                    int cubeIndex = Triangulation.CalculateCubeIndex(isoLevel, cubePoints);

                    /* Cube is entirely in/out of the surface */
                    if (Triangulation.IsTrivial(cubeIndex)) {
                        continue;
                    }

                    Vector3[] interpolatedVertices = Triangulation.GetVertices(isoLevel, cubeIndex, cubePoints);
                    for (int v = 0; v < interpolatedVertices.Length; v++) {
                        colors[vertexOffset + v] = Color32.Lerp(black, white, interpolatedVertices[v].y / height);
                        vertices[vertexOffset + v] = interpolatedVertices[v];
                    }

                    int[] interpolatedTriangles = Triangulation.GetTriangles(cubeIndex);
                    for (int t = 0; t < interpolatedTriangles.Length; t++) {
                        triangles[currentTriangle + t] = interpolatedTriangles[t] + vertexOffset;
                    }
                    currentTriangle += interpolatedTriangles.Length;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.colors32 = colors;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    float CalculateIsoValue(Vector3 vertex) {
        System.Random prng = new System.Random(2);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + xNoiseOffset;
            float offsetZ = prng.Next(-100000, 100000) + zNoiseOffset;
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++) {
            float sampleX = vertex.x * scale * frequency + octaveOffsets[i].x;
            float sampleY = vertex.z * scale * frequency + octaveOffsets[i].y;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }
        return noiseHeight * (1 - vertex.y / height);
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
}
