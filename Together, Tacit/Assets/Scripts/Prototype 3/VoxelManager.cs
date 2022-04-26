using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MarchingCubesProject;
using UnityEditor.Formats.Fbx.Exporter;

public enum MARCHING_MODE {
    CUBES,
    TETRAHEDRON
}

public enum STARTING_MODEL {
    CUBE,
    SPHERE
}

public class VoxelManager : MonoBehaviour
{
    // PUBLIC VARIABLES.
    // Public sculpture parameters.
    public Material material;
    private MARCHING_MODE mode;
    private STARTING_MODEL startingModel;
    // Dimension variables.
    public float playAreaDimensions = 2.0f;
    public float sculptureDimensions = 0.02f;
    public int toolRadiusInBlocks = 4;
    public int resolutionInVoxels = 80;
    // Haptic managers.
    public HandHapticManager leftHaptics;
    public HandHapticManager rightHaptics;

    // PRIVATE VARIABLES.
    // Dimension variables for mesh generation/adjustment.
    private int width;
    private int height;
    private int length;
    private float blockSideLength;
    private float scale;
    private float heightOffset;
    // Storage for voxel marcher and all generated voxels.
    private float[] voxels;
    private Marching marching;
    private bool sculptureGenerated;
    private Mesh mesh;
    private GameObject meshObject;
    private List<Vector3> verts = new List<Vector3>();
    private List<int> indices = new List<int>();
    private bool isToolModeCarving;

    public void Start() {
        // Upon startup, set the sculpture to a default set of settings.
        // Currently, this is as follows:
        //   - Cube sculpture shape
        //   - 80 voxel resolution in x, y, and z dimensions
        //   - Cube-based voxel marching
        //   - 2m x 2m x 2m play area
        // In the future, these could be stored and adjusted in a constants file, but for now
        // they are simply set and stored locally within this file.
        mode = MARCHING_MODE.CUBES;
        startingModel = STARTING_MODEL.SPHERE;
        resolutionInVoxels = 80;

        // Generate the mesh stored within this manager.
        mesh = new Mesh();

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
        // For simplicity, this is set to the same value for each dimension when populating space.
        width = resolutionInVoxels;
        height = resolutionInVoxels;
        length = resolutionInVoxels;

        // Calculate the size of a voxel block's dimensions.
        blockSideLength = playAreaDimensions / resolutionInVoxels;

        // Calculate scale and height offsets to match the vertices created to the play area.
        // For now, the play area is assumed to be a 2m x 2m x 2m cube, centered on the playmat.
        scale = playAreaDimensions / resolutionInVoxels;
        heightOffset = playAreaDimensions / 2;

        // Once all settings have been generated, create the sculpture for the first time to begin the scene.
        GenerateSculpture();

        // Start with the tool mode set to carve from the structure.
        isToolModeCarving = true;
    }

    public void RegenerateSculpture() {
        // Restore the voxel array to the current sculpture shape.
        PopulateVoxels();

        // Update the sculpture with the new voxel values.
        UpdateMeshes();
    }

    private void GenerateSculpture()
    {
        // Fill the voxel array with a starting sculpture.
        voxels = new float[width * height * length];
        PopulateVoxels();

        //The mesh produced is not optimal. There is one vert for each index.
        //Would need to weld vertices for better quality mesh.
        marching.Generate(voxels, width, height, length, verts, indices, scale, heightOffset);

        // TEST: Generate a mesh with an index format of 32 bit. This should allow up to 4 billion vertices,
        // which will realisticly never be reached within the bounds of the current program.
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Generate a new gameobject for the created mesh.
        meshObject = new GameObject("Mesh");
        meshObject.transform.parent = transform;
        meshObject.transform.localPosition = new Vector3(-(width*scale) / 2, (-(height*scale) / 2) + heightOffset, -(length*scale) / 2);
        // Apply needed components to new mesh to allow for XR interactions.
        meshObject.AddComponent<MeshFilter>();
        meshObject.AddComponent<MeshRenderer>();
        meshObject.AddComponent<MeshCollider>();
        meshObject.AddComponent<XRSimpleInteractable>();

        meshObject.GetComponent<Renderer>().material = material;
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        meshObject.GetComponent<Rigidbody>().useGravity = false;
        meshObject.GetComponent<Rigidbody>().isKinematic = true;

        // If this is the first time generating the sculpture, mark that the sculpture has been generated before.
        if (!sculptureGenerated) {
            sculptureGenerated = true;
        }
    }

    private void UpdateMeshes() {
        // NEW METHOD

        // Generate a new mesh, based on the changes made to the voxels prior to calling UpdateMeshes()
        List<Vector3> newVerts = new List<Vector3>();
        List<int> newIndices = new List<int>();
        marching.Generate(voxels, width, height, length, newVerts, newIndices, scale, heightOffset);

        // Update the mesh based on the verts and indices returned from generate.
        verts = newVerts;
        indices = newIndices;
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        // Set this new mesh as the mesh stored within the child Mesh GameObject.
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void CarveAndUpdate(Vector3 handPosition) {
        if (sculptureGenerated) {
            // Convert position to be within the bounds of the voxel array - from 0 to resolution.
            Vector3 adjustedPosition = new Vector3(
                ((handPosition.x / scale) + (width / 2)),
                (((handPosition.y - heightOffset) / scale) + (height / 2)),
                ((handPosition.z / scale) + (length / 2))
            );

            // Find all voxels within the radius of adjusted position.
            List<int> voxelsToChange = new List<int>();
            for (int i = -toolRadiusInBlocks; i < toolRadiusInBlocks; i++) {
                for (int j = -toolRadiusInBlocks; j < toolRadiusInBlocks; j++) {
                    for (int k = -toolRadiusInBlocks; k < toolRadiusInBlocks; k++) {
                        // Calculate the voxel being checked, offset from the original position.
                        int x = Mathf.FloorToInt(adjustedPosition.x + i);
                        int y = Mathf.FloorToInt(adjustedPosition.y + j);
                        int z = Mathf.FloorToInt(adjustedPosition.z + k);

                        // If the position of the voxel is within the tool, mark it to be changed.
                        if (Vector3.Distance(adjustedPosition, new Vector3(x, y, z)) < toolRadiusInBlocks) {
                            voxelsToChange.Add(x + y * width + z * width * height);
                        }
                    }
                }
            }

            // For every voxel marked to be changed, set it's value to 'outside'.
            for (int voxelIndex = 0; voxelIndex < voxelsToChange.Count; voxelIndex++) {
                voxels[voxelsToChange[voxelIndex]] = 0.0f;
            }

            // If any voxels were changed, update the meshes to match the new values.
            if (voxelsToChange.Count > 0) {
                UpdateMeshes();
            }
        }
    }

    public void AddAndUpdate(Vector3 handPosition) {
        if (sculptureGenerated) {
            // Convert position to be within the bounds of the voxel array - from 0 to resolution.
            Vector3 adjustedPosition = new Vector3(
                ((handPosition.x / scale) + (width / 2)),
                (((handPosition.y - heightOffset) / scale) + (height / 2)),
                ((handPosition.z / scale) + (length / 2))
            );

            // Find all voxels within the radius of adjusted position.
            List<int> voxelsToChange = new List<int>();
            for (int i = -toolRadiusInBlocks; i < toolRadiusInBlocks; i++) {
                for (int j = -toolRadiusInBlocks; j < toolRadiusInBlocks; j++) {
                    for (int k = -toolRadiusInBlocks; k < toolRadiusInBlocks; k++) {
                        // Calculate the voxel being checked, offset from the original position.
                        int x = Mathf.FloorToInt(adjustedPosition.x + i);
                        int y = Mathf.FloorToInt(adjustedPosition.y + j);
                        int z = Mathf.FloorToInt(adjustedPosition.z + k);

                        // If any of the dimensions are outside the bounds of the play area, pass over it.
                        bool isXOutOfBounds = (x < 0 || x > resolutionInVoxels);
                        bool isYOutOfBounds = (y < 0 || y > resolutionInVoxels);
                        bool isZOutOfBounds = (z < 0 || z > resolutionInVoxels);
                        if (isXOutOfBounds || isYOutOfBounds || isZOutOfBounds) {
                            continue;
                        }

                        // If the position of the voxel is within the tool, mark it to be changed.
                        if (Vector3.Distance(adjustedPosition, new Vector3(x, y, z)) < toolRadiusInBlocks) {
                            voxelsToChange.Add(x + y * width + z * width * height);
                        }
                    }
                }
            }

            // For every voxel marked to be changed, set it's value to 'inside'.
            for (int voxelIndex = 0; voxelIndex < voxelsToChange.Count; voxelIndex++) {
                voxels[voxelsToChange[voxelIndex]] = 1.0f;
            }

            // If any voxels were changed, update the meshes to match the new values.
            if (voxelsToChange.Count > 0) {
                UpdateMeshes();
            }
        }
    }

    public void HideSculpture()
    {
        // Toggle the visibility of the mesh gameobject.
        meshObject.SetActive(false);

        // Disable the input managers for interactions with the sculpture while it is hidden.
        foreach (XRInput inputManager in GetComponents<XRInput>()) {
            inputManager.enabled = false;
        }
    }

    public void ShowSculpture()
    {
        // Toggle the visibility of the mesh gameobject.
        meshObject.SetActive(true);

        // Enable the input managers for interactions with the sculpture while it is visible.
        foreach (XRInput inputManager in GetComponents<XRInput>()) {
            inputManager.enabled = true;
        }
    }

    private void PopulateVoxels()
    {
        // Fill voxels with default starting values.
        // The starting model can either be a cube or a sphere, depending on user preference.
        // Value defaults to a cube. The loops go through each voxel's (x,y,z) global
        // position in space for these calculations.
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
                        } else {
                            voxels[idx] = 0f;
                        }
                    } else {
                        // Check to see if the adjusted x,y,z values are within a sphere's radius.
                        if (Vector3.Distance(new Vector3(xAdj, yAdj, zAdj), new Vector3(0f, heightOffset, 0f)) < (sculptureDimensions / 2)) {
                            voxels[idx] = 1f;
                        } else {
                            voxels[idx] = 0f;
                        }
                    }
                }
            }
        }
    }

    public void ChangeToolMode()
    {
        // Changes the tool mode stored within the manager.
        if (isToolModeCarving) {
            isToolModeCarving = false;
            leftHaptics.SendAddingHaptics();
            rightHaptics.SendAddingHaptics();
        } else {
            isToolModeCarving = true;
            leftHaptics.SendCarvingHaptics();
            rightHaptics.SendCarvingHaptics();
        }
    }

    public bool IsToolSetToCarving() {
        if (isToolModeCarving) {
            return true;
        } else {
            return false;
        }
    }

    public void SetCurrentModelType(STARTING_MODEL modelType) {
        // Changes the currently set starting model, which will update when the sculpture is reset.
        startingModel = modelType;
    }

    public void SaveSculpture()
    {
        // Method to save the current sculpture to a local folder "Saved Sculptures".
        // Uses the Unity FBX Exporter package.
        // Files are saved using the current date and time, to distinguish them from each other, in the following format:
        //   MM-dd-yyyy_HH-mm-ss (ex. 04-25-2022_04-34-12)
        string filePath = Path.Combine((Application.dataPath + "/Saved Sculptures"), $"{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}.fbx");
        Debug.Log(filePath);
        ModelExporter.ExportObject(filePath, meshObject);
    }
}
