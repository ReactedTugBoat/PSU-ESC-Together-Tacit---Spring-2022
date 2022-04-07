using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerRotation : MonoBehaviour
{
    // Objects for each portion of the finger (3 in total, each numbered).
    public GameObject finger1;
    public GameObject finger2;
    public GameObject finger3;
    // Storage for the initial Z rotation values of each of the finger's sections.
    private float initialFinger1Rotation;
    private float initialFinger2Rotation;
    private float initialFinger3Rotation;

    void Start()
    {
        // Store the initial rotation for each portion of the finger.
        initialFinger1Rotation = finger1.transform.localEulerAngles.z;
        initialFinger2Rotation = finger2.transform.localEulerAngles.z;
        initialFinger3Rotation = finger3.transform.localEulerAngles.z;
    }

    public void SetFingerFlexValue(float fingerFlexInDegrees)
    {
        // Method to take in flex values from a flex sensor and adjust finger joints.
        // Values coming in must take a value between 0 and 40 degrees. Any values above
        // or below this range need to be cut off to prevent errors.
        float degreeAdjustment;
        if (fingerFlexInDegrees < 0) {
            degreeAdjustment = 0;
        } else if (fingerFlexInDegrees > 40) {
            degreeAdjustment = 40;
        } else {
            degreeAdjustment = fingerFlexInDegrees;
        }

        // Set the Z rotation of each portion of the finger to an offset of the original rotation.
        // To do this, rotate each finger by the difference between its current rotation and the new one.
        finger1.transform.localEulerAngles = new Vector3(
            finger1.transform.localEulerAngles.x,
            finger1.transform.localEulerAngles.y,
            initialFinger1Rotation - degreeAdjustment
        );
        finger2.transform.localEulerAngles = new Vector3(
            finger2.transform.localEulerAngles.x,
            finger2.transform.localEulerAngles.y,
            initialFinger2Rotation - degreeAdjustment
        );
        finger3.transform.localEulerAngles = new Vector3(
            finger3.transform.localEulerAngles.x,
            finger3.transform.localEulerAngles.y,
            initialFinger3Rotation - degreeAdjustment
        );
    }
    
}
