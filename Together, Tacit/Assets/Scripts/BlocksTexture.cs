using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksTexture : MonoBehaviour
{
    public Material materialChosen;

    // Start is called before the first frame update
    void Start()
    {
        // Upon runtime, set the texture for every child object to texture.
        // Check each Block Plane
        foreach (Transform plane in transform)
        {
            // Check each Block Row
            foreach (Transform row in plane)
            {
                // Check each Block Part
                foreach (Transform block in row)
                {
                    block.gameObject.GetComponent<Renderer>().material = materialChosen;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
