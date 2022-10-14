using UnityEngine;
/*
    Since the Boundaries are a Scriptable Object
    it's easily swapped or modified in code
*/

public class CelesteCameraMovement : MonoBehaviour
{
    Camera _camera;
    public LevelBoundaries_SO thisLevelsBoundaries;
    public bool ShowGizmos;

    Vector3[] frustumCorners;


    [Tooltip("Left and Right Boundaries")] public Vector2 currentCameraHorizontalBoundaries;
    [Tooltip("Bottom and Top Boundaries")] public Vector2 currentCameraVerticalBoundaries;


    public Transform targetToFollow;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        frustumCorners = new Vector3[4];
    }

    private void Update()
    {
        // First try to follow the character
        transform.position = new Vector3(targetToFollow.position.x, targetToFollow.position.y, transform.position.z);


        // But then do the thing to not go out of the limits



        // For Debug :
        // Show the line from the camera to the corners
        // Has a small difference of around 0.05? but not much

        // This apparently gives the points of the 4 corners of the Frustum
        _camera.CalculateFrustumCorners(_camera.rect, _camera.farClipPlane, _camera.stereoActiveEye, frustumCorners);

        if (ShowGizmos)
        {
            var worldSpaceCorner = _camera.transform.TransformVector(frustumCorners[0]);
            Debug.DrawRay(_camera.transform.position, worldSpaceCorner, Color.blue);

            worldSpaceCorner = _camera.transform.TransformVector(frustumCorners[1]);
            Debug.DrawRay(_camera.transform.position, worldSpaceCorner, Color.red);

            worldSpaceCorner = _camera.transform.TransformVector(frustumCorners[2]);
            Debug.DrawRay(_camera.transform.position, worldSpaceCorner, Color.yellow);

            worldSpaceCorner = _camera.transform.TransformVector(frustumCorners[3]);
            Debug.DrawRay(_camera.transform.position, worldSpaceCorner, Color.green);
        }

        // Only take Blue & yellow since they are the Bottom-left & Top-Right corners
        // So index 0 for Min and index 2 for Max


        // Offset everything with transform.position
        // since the values are local (relative to camera position)
        // but don't overwrite it because we'll need it later

        // Those are the minimum Left and minimum Bottom
        currentCameraHorizontalBoundaries.x = frustumCorners[0].x + transform.position.x;
        currentCameraVerticalBoundaries.x = frustumCorners[0].y + transform.position.y;

        // Those are the Maximum Right and the Maximum Top
        currentCameraHorizontalBoundaries.y = frustumCorners[2].x + transform.position.x;
        currentCameraVerticalBoundaries.y = frustumCorners[2].y + transform.position.y;




        // Check if we're too much to the left
        if (currentCameraHorizontalBoundaries.x < thisLevelsBoundaries.HorizontalBoundaries.x)
        {
            // Set value to "Left Level Limit + Offset"
            transform.position = new Vector3(thisLevelsBoundaries.HorizontalBoundaries.x + Mathf.Abs(frustumCorners[0].x), transform.position.y, transform.position.z);
        }
        // Otherwise check if we're too much to the right
        else if (currentCameraHorizontalBoundaries.y > thisLevelsBoundaries.HorizontalBoundaries.y)
        {
            // Set value to "Right level Limit - Offset"
            transform.position = new Vector3(thisLevelsBoundaries.HorizontalBoundaries.y - Mathf.Abs(frustumCorners[2].x), transform.position.y, transform.position.z);
        }


        // Ditto but with Bottom
        if (currentCameraVerticalBoundaries.x < thisLevelsBoundaries.VerticalBoundaries.x)
        {
            transform.position = new Vector3(transform.position.x, thisLevelsBoundaries.VerticalBoundaries.x + Mathf.Abs(frustumCorners[0].y), transform.position.z);
        }
        // Ditto but with Top
        else if (currentCameraVerticalBoundaries.y > thisLevelsBoundaries.VerticalBoundaries.y)
        {
            transform.position = new Vector3(transform.position.x, thisLevelsBoundaries.VerticalBoundaries.y - Mathf.Abs(frustumCorners[2].y), transform.position.z);
        }

    }


































    void OnDrawGizmos()
    {
        if (ShowGizmos)
        {
            Vector3 pointToChangeOne = Vector3.zero;
            Vector3 pointToChangeTwo = Vector3.zero;

            Gizmos.color = Color.cyan;


            // Draw Left Boundary
            pointToChangeOne.x = pointToChangeTwo.x = thisLevelsBoundaries.HorizontalBoundaries.x;
            pointToChangeOne.y = -100;
            pointToChangeTwo.y = 100;
            pointToChangeOne.z = pointToChangeTwo.z = 0;

            Gizmos.DrawLine(pointToChangeOne, pointToChangeTwo);



            // Draw Right Boundary
            pointToChangeOne.x = pointToChangeTwo.x = thisLevelsBoundaries.HorizontalBoundaries.y;

            Gizmos.DrawLine(pointToChangeOne, pointToChangeTwo);



            // Draw Up Boundary
            pointToChangeOne.y = pointToChangeTwo.y = thisLevelsBoundaries.VerticalBoundaries.x;
            pointToChangeOne.x = -100;
            pointToChangeTwo.x = 100;
            pointToChangeOne.z = pointToChangeTwo.z = 0;

            Gizmos.DrawLine(pointToChangeOne, pointToChangeTwo);



            // Draw Left Boundary
            pointToChangeOne.y = pointToChangeTwo.y = thisLevelsBoundaries.VerticalBoundaries.y;

            Gizmos.DrawLine(pointToChangeOne, pointToChangeTwo);
        }
        
    }
}
