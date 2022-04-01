using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MarchingCubesProject;

public enum MARCHING_MODE {
    CUBES,
    TETRAHEDRON
}

public enum STARTING_MODEL {
    CUBE,
    SPHERE
}

public readonly struct marchedVoxel {
    public marchedVoxel(int id) {
        voxelId = id;
        vertIds = new List<int>();
        indicesIds = new List<int>();
    }

    public int voxelId { get; }
    public List<int> vertIds { get; }
    public List<int> indicesIds { get; }
}

public class MarchingCubesTest : MonoBehaviour
{
    public Material material;
    public MARCHING_MODE mode = MARCHING_MODE.CUBES;
    public STARTING_MODEL startingModel = STARTING_MODEL.CUBE;
    List<GameObject> meshes = new List<GameObject>();
    public float playAreaDimensions = 2.0f;
    public float sculptureDimensions = 0.02f;
    public float toolRadius = 0.05f;
    private int width;
    private int height;
    private int length;
    private float scale;
    private float heightOffset;
    private float[] voxels;
    private Marching marching;
    private bool sculptureGenerated;

    public void Start()
    {
        // Set the mode used to create the mesh.
        // Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface.
        marching = null;
        if(mode == MARCHING_MODE.TETRAHEDRON) {
            marching = new MarchingTertrahedron();
        } else {
            marching = new MarchingCubes();
        }

        // Surface is the value that represents the surface of mesh.
        // For the purposes of this project, 1 represents 'inside' the sculpture
        // and 0 represents 'outside', so we can use a value between the two to
        // define the edges without much additional complexity.
        marching.Surface = 0.5f;

        // The size of the voxel array.
        // For simplicity atm, this is set to the same value for each dimension when populating space.
        // int width = 200;
        // int height = 200;
        // int length = 200;
        int resolution = 100;
        width = resolution;
        height = resolution;
        length = resolution;

        // Calculate scale and height offsets to match the vertices created to the play area.
        // For now, the play area is assumed to be a 2m x 2m x 2m cube, centered on the playmat.
        scale = playAreaDimensions / resolution;
        heightOffset = playAreaDimensions / 2;

        voxels = new float[width * height * length];

        // Fill voxels with values.
        // The starting model can either be a cube or a sphere, depending on user preference.
        // Value defaults to a cube. The loops go through each voxel's (x,y,z) global
        // position in space for these calculations.
        int drawnVoxels = 0;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int z = 0; z < length; z++) {
                    
                    // Calculate the global x,y,z positions using the unadjusted values.
                    float xAdj = (x * scale) - (width * scale / 2);
                    float yAdj = (y * scale) - (height * scale / 2) + heightOffset;
                    float zAdj = (z * scale) - (length * scale / 2);

                    int idx = x + y * width + z * width * height;

                    // Choose which voxels are in/out based on the chosen generation mode.
                    if (startingModel == STARTING_MODEL.CUBE) {
                        // Check to see if a given voxel is within the chosen dimensions from 0.
                        bool voxelAboveWidthMin = (xAdj > -(sculptureDimensions / 2));
                        bool voxelBelowWidthMax = (xAdj < (sculptureDimensions / 2));
                        bool voxelAboveHeightMin = (yAdj > -(sculptureDimensions / 2) + heightOffset);
                        bool voxelBelowHeightMax = (yAdj < (sculptureDimensions / 2) + heightOffset);
                        bool voxelAboveLengthMin = (zAdj > -(sculptureDimensions / 2));
                        bool voxelBelowLengthMax = (zAdj < (sculptureDimensions / 2));

                        // If a voxel fulfulls all requirements to be inside, set it to 1.
                        if (voxelAboveWidthMin && voxelBelowWidthMax && voxelAboveHeightMin && voxelBelowHeightMax && voxelAboveLengthMin && voxelBelowLengthMax) {
                            voxels[idx] = 1f;
                            drawnVoxels++;
                        }
                    } else {
                        // Check to see if the adjusted x,y,z values are within a sphere's radius.
                        if (Vector3.Distance(new Vector3(xAdj, yAdj, zAdj), new Vector3(0f, heightOffset, 0f)) < (sculptureDimensions / 2)) {
                            voxels[idx] = 1f;
                            drawnVoxels++;
                        }
                    }
                }
            }
        }

        Debug.Log("Voxels drawn: " + drawnVoxels);

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels, width, height, length, verts, indices, scale, heightOffset);

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            //go.AddComponent<HandCarvingMarching>();
            //go.AddComponent<HandButton>();
            go.AddComponent<MeshCollider>();
            //go.AddComponent<MeshCollider>();
            go.AddComponent<Rigidbody>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            //MeshCollider[] colliders = go.GetComponents<MeshCollider>();
            //foreach (MeshCollider collider in colliders) {
            //    collider.sharedMesh = mesh;
            //    collider.convex = true;
            //}
            go.GetComponent<MeshCollider>().sharedMesh = mesh;
            go.GetComponent<Rigidbody>().useGravity = false;
            go.GetComponent<Rigidbody>().isKinematic = true;
            go.AddComponent<XRSimpleInteractable>();
            go.transform.localPosition = new Vector3(-(width*scale) / 2, (-(height*scale) / 2) + heightOffset, -(length*scale) / 2);

            meshes.Add(go);
        }

        // If this is the first time generating the sculpture, show that is has been.
        if (!sculptureGenerated) {
            sculptureGenerated = true;
        }
    }

    private void UpdateMeshes() {
        // Destroy all previously made meshes so that they can be remade.
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        List<Vector3> verts = new List<Vector3>();
        List<int> indices = new List<int>();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels, width, height, length, verts, indices, scale, heightOffset);

        //A mesh in unity can only be made up of 65000 verts.
        //Need to split the verts between multiple meshes.
        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = verts.Count / maxVertsPerMesh + 1;

        for (int i = 0; i < numMeshes; i++)
        {

            List<Vector3> splitVerts = new List<Vector3>();
            List<int> splitIndices = new List<int>();

            for (int j = 0; j < maxVertsPerMesh; j++)
            {
                int idx = i * maxVertsPerMesh + j;

                if (idx < verts.Count)
                {
                    splitVerts.Add(verts[idx]);
                    splitIndices.Add(j);
                }
            }

            if (splitVerts.Count == 0) continue;

            Mesh mesh = new Mesh();
            mesh.SetVertices(splitVerts);
            mesh.SetTriangles(splitIndices, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            //go.AddComponent<HandCarvingMarching>();
            //go.AddComponent<HandButton>();
            go.AddComponent<MeshCollider>();
            //go.AddComponent<MeshCollider>();
            go.AddComponent<Rigidbody>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            //MeshCollider[] colliders = go.GetComponents<MeshCollider>();
            //foreach (MeshCollider collider in colliders) {
            //    collider.sharedMesh = mesh;
            //    collider.convex = true;
            //}
            go.GetComponent<MeshCollider>().sharedMesh = mesh;
            go.GetComponent<Rigidbody>().useGravity = false;
            go.GetComponent<Rigidbody>().isKinematic = true;
            go.AddComponent<XRSimpleInteractable>();
            go.transform.localPosition = new Vector3(-(width*scale) / 2, (-(height*scale) / 2) + heightOffset, -(length*scale) / 2);

            meshes.Add(go);
        }
    }

    public void CarveAndUpdate(Vector3 handPosition) {
        if (sculptureGenerated) {
            // First, reiterate through each entry inside voxels and see if any of them are within a fixed distance from the hand.
            // This is used to determine which voxels need to be removed from the array before re-running the marching.
            // Any changed voxels are tracked in a variable to determine if recreating the meshes are needed.
            // TODO: Store any changes made during a single button press in an array, acting as a sort of 'Undo' for later.
            int changedVoxelsCount = 0;
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    for (int z = 0; z < length; z++) {
                        // Calculate the global x,y,z positions using the unadjusted values.
                        float xAdj = (x * scale) - (width * scale / 2);
                        float yAdj = (y * scale) - (height * scale / 2) + heightOffset;
                        float zAdj = (z * scale) - (length * scale / 2);

                        int idx = x + y * width + z * width * height;

                        // Check if voxel is within a radius-length distance from the given hand position.
                        if (Vector3.Distance(new Vector3(xAdj, yAdj, zAdj), handPosition) < toolRadius)
                        {
                            // If so, and that vector is currently set to 'inside', change its value to 'outside' and mark the change.
                            if (voxels[idx] == 1.0f) {
                                voxels[idx] = 0.0f;
                                changedVoxelsCount++;
                            }
                        }
                    }
                }
            }

            // If any voxels were changed, update the meshes to match the new values.
            if (changedVoxelsCount > 0) {
                UpdateMeshes();
            }
        }
    }
}