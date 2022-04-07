using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerRotationTest : MonoBehaviour
{
    public float rotationAngle;
    public FingerRotation thumb;
    public FingerRotation index;
    public FingerRotation middle;

    void Update() {
        thumb.SetFingerFlexValue(rotationAngle);
        index.SetFingerFlexValue(rotationAngle);
        middle.SetFingerFlexValue(rotationAngle);
    }

    
}
