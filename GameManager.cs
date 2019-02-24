using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    private void Awake() { if (instance == null) instance = this; }

    public bool IsPaused { get; private set; }
    public static void PauseGame() { Instance.IsPaused = true; }
    public static void UnpauseGame() { Instance.IsPaused = false; }


    void Start()
    {
        TextureData.LoadTextures();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 500, Color.cyan);

        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, 500))
        {
            //Debug.Log(hit.transform.position.x + ", " + hit.transform.position.y + ", " + hit.transform.position.z);
            SliceMesh sliceMesh = hit.transform.GetComponent<SliceMesh>();
            //Debug.Log("Constituent in 0,0 , 0 : " + slice.Voxels[0, 0].Constituents[0].Name);
            if (sliceMesh != null)
            {
                Slice slice = sliceMesh.Slice;
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    Debug.Log("Slice clicked: " + slice.X + ", " + slice.Y + ", " + slice.Z);
                    if (slice.IsMeshVisible()) slice.SetHidden();
                }
            }
        }
    }
}
