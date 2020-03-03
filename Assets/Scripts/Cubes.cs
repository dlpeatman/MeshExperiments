using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Cubes : MonoBehaviour {

    private Mesh[] meshes;

    private Dictionary<string, GameObject> cubeObjects;

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
        //UpdateMeshes();
    }

    public void Reset(int width, int length) {
        if (cubeObjects == null) {
            cubeObjects = new Dictionary<string, GameObject>();
        }

        foreach (KeyValuePair<string, GameObject> entry in cubeObjects) {
            if (entry.Value != null) {
                entry.Value.SetActive(false);
            }
        }

        this.width = width;
        this.length = length;

        meshes = new Mesh[length * width];

        Generate();
    }

    public void Generate() {
        CleanUp();
        CreateObjects();
        UpdateMeshes();
        Combine();
    }

    public void CleanUp() {
        Component[] meshFilters = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFilters.Length; i++) {
            ((MeshFilter)meshFilters[i]).sharedMesh.Clear();
        }
        GetComponent<MeshFilter>().sharedMesh.Clear();
    }

    void CreateObjects() {
        for (int i = 0; i < length; i++) {
            for (int j = 0; j < width; j++) {
                int index = i * width + j;
                string name = "Cube-" + i + "-" + j;
                GameObject cube;
                Mesh mesh = new Mesh();
                if (!cubeObjects.ContainsKey(name) || cubeObjects[name] == null) {
                    cube = new GameObject(name);
                    cube.transform.SetParent(this.transform);
                    cube.AddComponent<MeshFilter>().mesh = mesh;
                    cubeObjects.Add(name, cube);
                } else {
                    cube = cubeObjects[name];
                    cube.GetComponent<MeshFilter>().mesh = mesh;
                    cube.SetActive(true);
                }
                meshes[index] = mesh;
            }
        }
    }

    void UpdateMeshes() {
        for (int i = 0; i < length; i++) {
            for (int j = 0; j < width; j++) {
                int index = i * width + j;
                Mesh mesh = meshes[index];
                mesh.Clear();
                Vector3 origin = new Vector3(-width / 2f + j, 0, -length / 2f + i);
                mesh.vertices = CreateCubeVertices(origin);
                mesh.triangles = CreateCubeTriangles(0);
            }
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

    void Combine() {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
        transform.gameObject.SetActive(true);
    }

    void OnDrawGizmos() {
        if (meshes == null || !Application.isPlaying) {
            return;
        }

        for (int i = 0; i < meshes.Length; i++) {
            Mesh mesh = meshes[i];
            for (int j = 0; j < meshes[i].vertices.Length; j++) {
                Gizmos.DrawSphere(mesh.vertices[j], .05f);
            }
        }
    }
}
