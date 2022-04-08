using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Currently, only four types of collision boxes are needed - three fingers and a general controller.
// In the future, if additional fingers are added, this can be expanded upon by adding more entries.
public enum CollisionBoxType {
    THUMB,
    INDEX,
    MIDDLE,
    OCULUS
}

public class SculptureCollisionBox : MonoBehaviour
{
    public ControllerState collisionState;
    private bool isCollidingWithSculpture;
    private bool prevCollidingWithSculpture;
    [SerializeField] private CollisionBoxType collisionBoxType;
    private GameObject voxelManager;

    void Start()
    {
        // Upon startup, initialize  the stored state to outside.
        // This is to try and prevent any unnecessary haptic feedback for the user.
        collisionState = ControllerState.Outside;

        // Find the Voxel Manager for use in determining inside/outside status.
        voxelManager = GameObject.Find("Voxel Manager");
    }

    void Update()
    {
        // UPDATE CURRENT STATE BASED ON CURRENT AND PREVIOUS COLLISIONS.
        // If currently colliding but previously not, set state to entering.
        if (isCollidingWithSculpture && !prevCollidingWithSculpture) {
            collisionState = ControllerState.Entering;
        }
        // If currently not colliding but previously were, set state to leaving.
        else if (!isCollidingWithSculpture && prevCollidingWithSculpture) {
            collisionState = ControllerState.Leaving;
        }
        // Otherwise, perform additional calculations to determine inside/outside position.
        else {
            // During development, some issues were found with these collision boxes passing fully within
            // the mesh of the sculpture. As a result, an additional test is needed to determine the actual
            // position if no collisions are currently found. The best way we found to do this was to
            // use raycasting from the position of this transform outward - if a mesh is found in all directions,
            // the collider is inside, otherwise, it is outside.
            if (IsInCollider(voxelManager.GetComponentInChildren<MeshCollider>(), transform.position)) {
                collisionState = ControllerState.Inside;
                isCollidingWithSculpture = true;
            } else {
                collisionState = ControllerState.Outside;
                isCollidingWithSculpture = false;
            }
        }

        // Store the status of the current collision as the prev collision.
        prevCollidingWithSculpture = isCollidingWithSculpture;

        // Set the 
    }

    public void Entering() {
        // Method called by an object which enters the sculpture.
        isCollidingWithSculpture = true;
    }

    public void Leaving() {
        // Method called by an object which leaves the sculpture.
        isCollidingWithSculpture = false;
    }
    
    // INSIDE/OUTSIDE HELPER FUNCTIONS.
    public bool IsInCollider(MeshCollider other, Vector3 point) {
         Vector3 from = (Vector3.up * 5000f);
         Vector3 dir = (point - from).normalized;
         float dist = Vector3.Distance(from, point);        
         //fwd      
         int hit_count = Cast_Till(from, point, other);
         //back
         dir = (from - point).normalized;
         hit_count += Cast_Till(point, point + (dir * dist), other);
 
         if (hit_count % 2 == 1) {
             return (true);
         }
         return (false);
     }
 
    int Cast_Till(Vector3 from, Vector3 to, MeshCollider other) {
        int counter = 0;
        Vector3 dir = (to - from).normalized;
        float dist = Vector3.Distance(from, to);
        bool Break = false;
        while (!Break) {
            Break = true;
            RaycastHit[] hit = Physics.RaycastAll(from, dir, dist);
            for (int tt = 0; tt < hit.Length; tt++) {
                if (hit[tt].collider == other) {
                    counter++;
                    from = hit[tt].point+dir.normalized*.001f;
                    dist = Vector3.Distance(from, to);
                    Break = false;
                    break;                    
                }
            }
        }
        return (counter);
    }
}
