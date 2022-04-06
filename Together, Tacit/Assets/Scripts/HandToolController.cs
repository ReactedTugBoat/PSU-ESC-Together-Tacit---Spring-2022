using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandToolController : MonoBehaviour
{
    public GameObject blockManager;
    int frame = 0;

    public void Carve() {
        // Only carve and update the mesh every tenth frame, to lower latency on the user's side.
        if (frame == 1) {
            // When called, pass the current position of the controller onto the block marcher to adjust the mesh.
            Vector3 currentPosition = transform.position;
            blockManager.GetComponent<VoxelManager>().CarveAndUpdate(currentPosition);
            frame = 0;
        } else {
            // Otherwise, simply increase the frame counter.
            frame++;
            // Debug.Log("Increasing frame count");
        }
    }

    public void Add() {
        // When called, pass the current position of the controller onto the block marcher to adjust the mesh.
        Vector3 currentPosition = transform.position;
        blockManager.GetComponent<VoxelManager>().AddAndUpdate(currentPosition);
    }
}
