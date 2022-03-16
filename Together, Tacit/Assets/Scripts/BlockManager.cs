using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    // PUBLIC VARIABLES.
    // Prefab of the blocks used in the scuplting.
    public GameObject blockPrefab;
    // Public values for the x, y, and z resolution of the final structure.
    public int xResolution;
    public int yResolution;
    public int zResolution;
    // Public values for the final width, length, and height of the structure (in meters).
    // These are used later in calculations for the size of each individual block.
    public float lengthInMeters;
    public float widthInMeters;
    public float heightInMeters;

    // PRIVATE LOCAL VARIABLES.
    // Internal values for each block's x, y, and z scale.
    private float xPosition;
    private float yPosition;
    private float zPosition;
    // Internal values for each block's x, y, and z scale.
    private float xScale;
    private float yScale;
    private float zScale;
    // Internal offset values to lift the final structure above ground level.
    private float lengthOffsetInMeters;
    private float widthOffsetInMeters;
    private float heightOffsetInMeters;
    // Tracker for GenerateSculpture to ensure it only runs once, and can only
    // be run once the program has finished running its initialization.
    private bool sculptureCanBeGenerated = true;
    public bool hasSculptureBeenGenerated = false;


    // Method to generate the sculpture.
    // Currently, this is limited so that it will only run one time.
    public void GenerateSculpture(Vector3 currentHandPosition)
    {
        Debug.Log("Starting Generation");
        // If the sculpture has not been generated yet, run the below code.
        if (sculptureCanBeGenerated && !hasSculptureBeenGenerated) {
            // Calculate the width, length, and height of each block in the structure.
            xScale = lengthInMeters / xResolution;
            yScale = heightInMeters / yResolution;
            zScale = widthInMeters / zResolution;

            // Calculate the offset for the sculpture from the x, y and z values of the current hand.
            lengthOffsetInMeters = currentHandPosition.x - lengthInMeters;
            heightOffsetInMeters = currentHandPosition.y - heightInMeters;
            widthOffsetInMeters = currentHandPosition.z - widthInMeters;

            // Set the first block placement in space, based off of the total width/length and a
            // set offset calculated from the position of the right hand at runtime.
            xPosition = (lengthInMeters / 2) + lengthOffsetInMeters;
            yPosition = (heightInMeters / 2) + heightOffsetInMeters;
            zPosition = (widthInMeters / 2) + widthOffsetInMeters;

            // INSTANTIATE THE PREFAB STRUCTURE.
            // Iterate through each dimension, creating blocks to the specified resolution.
            for (int x = 0; x < xResolution; x++) {

                for (int y = 0; y < yResolution; y++) {

                    for (int z = 0; z < zResolution; z++) {
                        // Create a vector for the position of the block, using the resolution to calculate
                        // its offset from the starting x, y, and z positions.
                        Vector3 blockVector = new Vector3(
                            xPosition + (x * xScale),
                            yPosition + (y * yScale),
                            zPosition + (z * zScale)
                        );

                        // Instantiate the prefab at the given location in space. 
                        GameObject block = Instantiate(blockPrefab, blockVector, Quaternion.identity);

                        // Set the scale of the created block as needed
                        block.transform.localScale = new Vector3(xScale, yScale, zScale);

                        // Set the new block as a child of the Block Manager parent.
                        block.transform.parent = this.gameObject.transform;
                    }
                }
            }

            // Once complete, note that the sculpture has been generated.
            hasSculptureBeenGenerated = true;
        }

    }

    // Public method to enable sculpture generation.
    public void EnableSculptureGeneration() {
        sculptureCanBeGenerated = true;
    }
}
