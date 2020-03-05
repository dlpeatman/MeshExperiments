using UnityEngine;

class Chunk {
    private GameObject gameObject;
    private Mesh mesh;

    public Chunk(GameObject gameObject, Material material, Transform parent) {
        this.gameObject = gameObject;
        gameObject.transform.parent = parent;
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        mesh = new Mesh();
        meshFilter.mesh = mesh;
    }

    public GameObject GameObject { get => gameObject; }
    public Mesh Mesh { get => mesh; }
}