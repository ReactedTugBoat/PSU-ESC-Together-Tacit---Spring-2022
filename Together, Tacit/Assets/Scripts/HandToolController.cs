using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandToolController : MonoBehaviour
{
    public GameObject blockManager;

    public void Carve() {
        // When called, pass the current position of the controller onto the block marcher to adjust the mesh.
        Vector3 currentPosition = transform.position;
        blockManager.GetComponent<MarchingCubesTest>().CarveAndUpdate(currentPosition);
    }
}
