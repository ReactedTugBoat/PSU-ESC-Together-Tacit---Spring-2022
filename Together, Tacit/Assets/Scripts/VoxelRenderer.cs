using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelRenderer : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    public int width;
    public int height;
    public int depth;
    public float scale;
    public float xStartingOffset;
    public float yStartingOffset;
    public float zStartingOffset;


    public float sideLengthInMeters = 1f;
    public float heightIncreaseInMeters = 1f;
    float adjScale;

    // Use this for initialization
    void Awake() {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start() {
        GenerateVoxelMesh(new VoxelData());
        UpdateMesh();
    }

    void GenerateVoxelMesh(VoxelData data) {
        // Store values for width, height, and depth of the sphere.
        width = data.Width;
        height = data.Height;
        depth = data.Depth;

        // Calculate the scale from the given side length of the cube and the width/height/depth.
        // Any of the three values could be used, so width is used for convenience.
        // If these numbers even end up taking on different values, this will need to be adjusted.
        if (width != height && height != depth && depth != width) {
            // Width, height, and depth are expected to all be the same value.
            // If width, height, and depth take on different values, return an error.
            Debug.LogError("Values for sculpture x, y, and z were not equal");
        }
        scale = sideLengthInMeters / width;
        adjScale = scale * 0.5f;

        // Calculate the offset of the starting point from the origin.
        xStartingOffset = sideLengthInMeters / 2f;
        yStartingOffset = sideLengthInMeters / 2f;
        zStartingOffset = sideLengthInMeters / 2f;

        // Populate the voxel array with data for a sphere.
        float radius = sideLengthInMeters / 2f;
        bool drawCube = true;
        for (int z = 0; z < data.Depth; z++) {
            for (int y = 0; y < data.Height; y++) {
                for (int x = 0; x < data.Width; x++) {
                    // For each Vector within the data, check to see if it is within a defined sphere.
                    // If so, set its value to 1. All other elements are initialized to 0 already.
                    // Vector3 sphereCenterRef = new Vector3(-scale/2f, -scale/2f, -scale/2f);
                    // if (Vector3.Distance(new Vector3(
                    //     (float)x * scale - xStartingOffset,
                    //     (float)y * scale - yStartingOffset + heightIncreaseInMeters,
                    //     (float)z * scale - zStartingOffset),
                    //     Vector3.zero) < radius)
                    // {
                    //     data.voxelData[x,y,z] = 1;
                    // }
                    if (drawCube) {
                        data.voxelData[x,y,z] = 1;
                        drawCube = false;
                    } else {
                        drawCube = true;
                    }


                }
            }
        }
        
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int z = 0; z < data.Depth; z++) {
            for (int y = 0; y < data.Height; y++) {
                for (int x = 0; x < data.Width; x++) {
                    if (data.GetCell(x, y, z) == 0) {
                        continue;
                    }
                    if ((x, y, z) == (0, 0, 0)) {
                        Debug.Log((float)x * scale - xStartingOffset);
                        Debug.Log((float)y * scale - yStartingOffset + heightIncreaseInMeters);
                        Debug.Log((float)z * scale - zStartingOffset);
                    }
                    MakeCube(adjScale, new Vector3(
                        (float)x * scale - xStartingOffset, 
                        (float)y * scale - yStartingOffset + heightIncreaseInMeters, 
                        (float)z * scale - zStartingOffset),
                        x, y, z, data);
                }
            }
        }

        Debug.Log("Mesh finished");
    }

    void MakeCube (float cubeScale, Vector3 cubePos, int x, int y, int z, VoxelData data) {
        for (int dir = 0; dir < 6; dir++) {
            if (data.GetNeighbor(x, y, z, (Direction)dir) == 0) {
                MakeFace((Direction)dir, cubeScale, cubePos);
            }
        }
    }

    void MakeFace (Direction dir, float faceScale, Vector3 facePos) {
        vertices.AddRange(CubeMeshData.faceVertices(dir, faceScale, facePos));
        int vCount = vertices.Count;

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 1);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 4 + 2);
        triangles.Add(vCount - 4 + 3);
    }

    void UpdateMesh() {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    // Create a vector using provided (x,y,z) values and the set scale and resolution in the class object.
    private void CreateVector() {

    }
}
