using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    public struct MeshData {
        public Vector3[] vertices;
        public Color32[] colors;
        public int[] triangles;

        public MeshData(Vector3[] vertices, Color32[] colors, int[] triangles) {
            this.vertices = vertices;
            this.colors = colors;
            this.triangles = triangles;
        }
    }
    private static Color32 green = new Color32(0, 180, 60, 255);
    private static Color32 grey = new Color32(80, 80, 80, 255);
    private static Color32 white = new Color32(255, 255, 255, 255);


    private Dictionary<string, Chunk> chunkDictionary;

    private Vector2[] octaveOffsets;
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
        System.Random prng = new System.Random(2);
        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + xNoiseOffset;
            float offsetZ = prng.Next(-100000, 100000) + zNoiseOffset;
            octaveOffsets[i] = new Vector2(offsetX, offsetZ);
        }

        var chunkTasks = new Dictionary<Chunk, Task<MeshData>>();

        for (int z = 0; z < 4; z++) {
            for (int x = 0; x < 4; x++) {
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

                chunkTasks.Add(chunk, Task<MeshData>.Run(() => CreateMesh(location)));
            }
        }

        Task.WaitAll(chunkTasks.Values.ToArray());

        foreach (KeyValuePair<Chunk, Task<MeshData>> entry in chunkTasks) {
            var meshData = entry.Value.Result;
            var mesh = entry.Key.Mesh;

            mesh.Clear();
            mesh.vertices = meshData.vertices;
            mesh.colors32 = meshData.colors;
            mesh.triangles = meshData.triangles;
            mesh.RecalculateNormals();
        }
    }

    MeshData CreateMesh(Vector3 origin) {
        int trianglesPerCube = 4 * 3;

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
                        colors[vertexOffset + v] = CalculateColor(interpolatedVertices[v].y / height);
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
        return new MeshData(vertices, colors, triangles);
    }

    float CalculateIsoValue(Vector3 vertex) {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;
        float yValue = 0;

        for (int i = 0; i < octaves; i++) {
            float sampleX = vertex.x * scale * frequency + octaveOffsets[i].x;
            float sampleZ = vertex.z * scale * frequency + octaveOffsets[i].y;
            float sampleY = vertex.y * scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
            noiseHeight += perlinValue * amplitude;

            float yPerlin = Mathf.PerlinNoise(sampleY, sampleY);
            yValue += yPerlin * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }
        // return (noiseHeight + yValue) / 2;
        return 1 - (noiseHeight / 2 + Mathf.Sin(vertex.y / height * Mathf.PI / 2) / 2);
    }

    Color32 CalculateColor(float y) {
        if (y < .5) {
            return Color32.Lerp(green, grey, y * 2);
        } else {
            return Color32.Lerp(grey, white, y * 2 - 1);
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
}
