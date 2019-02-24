using UnityEngine;
using System.Collections;

// TODO: CHECK ABOVE FOCUS POINT AND SEE WHERE NEXT VOXEL OBSTRUCTS VERTICALLY.
//       PUT ALL SLICES ON ALL THOSE Y-LEVELS INTO VisibleSlices and make visible.
//       HIDE ALL SLICES MARKED AS HIDDEN AFTER ALL SLICES HAVE BEEN RENDERED.

public class CameraMovementManager : MonoBehaviour
{
    // Singleton
    private static CameraMovementManager instance;
    public static CameraMovementManager Instance { get { return instance; } }
    private void Awake() { if (instance == null) instance = this; }

    public enum ViewMode { Top, Iso }
    public static ViewMode mode = ViewMode.Top;

    public Camera mainCamera;
    public GameObject cameraAnchor;
    public GameManager gameManager;

    public float panSpeed = 20.0f;
    float minXZ, maxXZ, minY, maxY;
    public float zoomSpeed = 1000.0f;
    public int maxZoomIn = 50;
    public int maxZoomOut = 120;
    public int degreesToRotate = 45;
    public float FasterYSliceSpeed = 10.0f;
    public KeyCode rotateCounterClockwiseKey = KeyCode.Q;
    public KeyCode rotateClockwiseKey = KeyCode.E;
    public KeyCode zoomKey = KeyCode.LeftShift;
    public KeyCode fastYTraversalKey = KeyCode.LeftControl;

    public static int CurrentYPosition { get; private set; }
    public static int CurrentYUpperBound { get; private set; } // relative to CurrentYPosition, e.g. 1 == +1, 2 == +2, etc...

    void Start()
    {
        minXZ = -(Map.XZOffset * Map.VoxelsInXZ);
        maxXZ = Mathf.Abs(minXZ) - 1;
        minY = -(Map.YOffset) + Map.VoxelFloorHeight;
        maxY = Mathf.Abs(minY) - 1 + ((Map.SlicesInY + 1) * Map.VoxelFloorHeight);
        CurrentYPosition = 0;
        CurrentYUpperBound = 0;

        transform.position = new Vector3(0, minY + (Map.YOffset * Map.VoxelFloorHeight + Map.YOffset), 0);
    }

    void Update()
    {

        if (!gameManager.IsPaused)
        {
            // Panning.
            float translationX = Input.GetAxis("Horizontal") * panSpeed * Time.deltaTime;
            float translationZ = Input.GetAxis("Vertical") * panSpeed * Time.deltaTime;
            int previousYPosition = CurrentYPosition;
            int previousYUpperBound = CurrentYUpperBound; // For SliceRenderMode.Adaptive
            if (translationX != 0 || translationZ != 0)
            {
                transform.Translate(translationX, 0, translationZ, Space.Self);

                float posX = transform.position.x;
                float posZ = transform.position.z;

                // Constrain to map.
                if (transform.position.x < minXZ)       posX = minXZ;
                else if (transform.position.x > maxXZ)  posX = maxXZ;
                if (transform.position.z < minXZ)       posZ = minXZ;
                else if (transform.position.z > maxXZ)  posZ = maxXZ;

                transform.position = new Vector3(posX, transform.position.y, posZ);
                if (Map.SliceRenderMode == SliceRenderMode.Adaptive)
                {
                    Voxel v = Map.GetVoxelCameraFocusCoordinates(posX, CurrentYPosition, posZ);
                    CurrentYUpperBound = Map.FindDistanceToFirstNonTransparentVoxel(Direction.Up, v);
                }

                // Rendering in slice mode only forces adjustments when traversing y levels.
                if (Map.SliceRenderMode == SliceRenderMode.Slice && previousYPosition != CurrentYPosition)
                {
                    Map.SetSlicesAboveYHidden(CurrentYPosition + Map.YOffset);
                    Map.SetSlicesOnAndBelowYVisible(CurrentYPosition + Map.YOffset);
                    Map.RebuildQueuedSlicesOnY(CurrentYPosition + Map.YOffset); // Rebuild all visible instead.

                }
                // Rendering in adaptive mode forces adjustments when traversing y levels or moving on the x,z.
                if (Map.SliceRenderMode == SliceRenderMode.Adaptive && previousYPosition != CurrentYPosition || previousYUpperBound != CurrentYUpperBound)
                {
                    Map.SetSlicesAboveYHidden(CurrentYPosition + CurrentYUpperBound + Map.YOffset);
                    Map.SetSlicesOnAndBelowYVisible(CurrentYPosition + CurrentYUpperBound + Map.YOffset);
                    Map.RebuildQueuedSlicesOnY(CurrentYPosition + CurrentYUpperBound + Map.YOffset);
                }
                if (Map.SliceRenderMode == SliceRenderMode.Full) // SOMETHING IS CAUSING A VISUAL PULSATION BUG HERE.
                {
                    if (Map.VisibleSlices.Count != Map.Instance.Slices.Length)
                    {
                        Map.SetSlicesOnAndBelowYVisible((int)maxY);
                    }
                    Map.RebuildQueuedSlices(); // This should be left to the regular update call for rebuilding.
                }

            }

            // Rotating.
            if (Input.GetKeyUp(rotateCounterClockwiseKey))
                transform.Rotate(0, degreesToRotate, 0, Space.World);
            if (Input.GetKeyUp(rotateClockwiseKey))
                transform.Rotate(0, -degreesToRotate, 0, Space.World);

            // Zooming and slice traversal.
            float translationY = Input.GetAxis("Mouse ScrollWheel");
            if (translationY != 0)
            {
                // Zooming.
                if (Input.GetKey(zoomKey))
                {
                    translationY *= zoomSpeed * Time.deltaTime;
                    float posY = mainCamera.transform.localPosition.y - translationY;

                    // Constrain to zoom limits.
                    if (posY > maxZoomOut)
                        posY = maxZoomOut;
                    else if (posY < maxZoomIn)
                        posY = maxZoomIn;

                    mainCamera.transform.localPosition = new Vector3(0, posY, 0);
                }

                // Fast slice traversal.
                else if (Input.GetKey(fastYTraversalKey))
                {
                    if (translationY > 0)
                    {
                        translationY = (FasterYSliceSpeed * Map.VoxelFloorHeight) + FasterYSliceSpeed;
                        CurrentYPosition += (int)FasterYSliceSpeed;
                    }
                    else
                    {
                        translationY = -(FasterYSliceSpeed * Map.VoxelFloorHeight) - FasterYSliceSpeed;
                        CurrentYPosition -= (int)FasterYSliceSpeed;
                    }

                    // Constrain to map.
                    float posY = transform.position.y + translationY;
                    if (posY < minY)
                    {
                        posY = minY;
                        CurrentYPosition = -Map.YOffset;
                    }
                    else if (posY > maxY)
                    {
                        posY = maxY;
                        CurrentYPosition = Map.YOffset - 1;
                    }

                    transform.position = new Vector3(transform.position.x, posY, transform.position.z);

                    // For SliceRenderMode.Adaptive mode.
                    if (Map.SliceRenderMode == SliceRenderMode.Adaptive)
                    {
                        Voxel v = Map.GetVoxelCameraFocusCoordinates(transform.position.x, CurrentYPosition, transform.position.z);
                        CurrentYUpperBound = Map.FindDistanceToFirstNonTransparentVoxel(Direction.Up, v);
                    }
                }

                // Regular slice traversal.
                else
                {
                    if (translationY > 0)
                    {
                        translationY = 1.0f + Map.VoxelFloorHeight;
                        CurrentYPosition += 1;
                    }
                    else
                    {
                        translationY = -1.0f - Map.VoxelFloorHeight;
                        CurrentYPosition -= 1;
                    }

                    // Constrain to map.
                    float posY = transform.position.y + translationY;
                    if (posY < minY)
                    {
                        posY = minY;
                        CurrentYPosition = -Map.YOffset;
                    }
                    else if (posY > maxY)
                    {
                        posY = maxY;
                        CurrentYPosition = Map.YOffset - 1;
                    }

                    transform.position = new Vector3(transform.position.x, posY, transform.position.z);

                    if (Map.SliceRenderMode == SliceRenderMode.Adaptive)
                    {
                        // For SliceRenderMode.Adaptive mode.
                        Voxel v = Map.GetVoxelCameraFocusCoordinates(transform.position.x, CurrentYPosition, transform.position.z);
                        CurrentYUpperBound = Map.FindDistanceToFirstNonTransparentVoxel(Direction.Up, v);
                    }
                }
                Debug.Log(CurrentYPosition);


                // Rendering in slice mode only forces adjustments when traversing y levels.
                if (Map.SliceRenderMode == SliceRenderMode.Slice)
                {
                    Map.SetSlicesAboveYHidden(CurrentYPosition + Map.YOffset);
                    Map.SetSlicesOnAndBelowYVisible(CurrentYPosition + Map.YOffset);
                    Map.RebuildQueuedSlicesOnY(CurrentYPosition + Map.YOffset);

                }
                // Rendering in adaptive mode forces adjustments when traversing y levels or moving on the x,z.
                if (Map.SliceRenderMode == SliceRenderMode.Adaptive &&
                    previousYPosition != CurrentYPosition || previousYUpperBound != CurrentYUpperBound)
                {
                    Map.SetSlicesAboveYHidden(CurrentYPosition + CurrentYUpperBound + Map.YOffset);
                    Map.SetSlicesOnAndBelowYVisible(CurrentYPosition + CurrentYUpperBound + Map.YOffset);
                    Map.RebuildQueuedSlicesOnY(CurrentYPosition + CurrentYUpperBound + Map.YOffset);
                }
                if (Map.SliceRenderMode == SliceRenderMode.Full)
                {
                    if (Map.VisibleSlices.Count != Map.Instance.Slices.Length)
                    {
                        Map.SetSlicesOnAndBelowYVisible((int)maxY);
                    }
                    Map.RebuildQueuedSlices(); // This should be left to the regular update call for rebuilding.
                }
            }

        }
    }

    public static void SwitchToTopView()
    {
        Instance.cameraAnchor.transform.rotation = new Quaternion(0, 0, 0, 0);
        mode = ViewMode.Top;
    }
    public static void SwitchToIsoView()
    {
        Instance.cameraAnchor.transform.Rotate(-45, 0, 0, Space.Self);
        mode = ViewMode.Iso;
    }
}
