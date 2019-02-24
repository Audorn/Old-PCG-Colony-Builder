using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDInputManager : MonoBehaviour
{
    public GameManager gameManager;

    public Canvas mainMenuCanvas;
    public void ActivateMainMenu() { mainMenuCanvas.enabled = true; }
    public void DeactivateMainMenu() { mainMenuCanvas.enabled = false; }

    public Canvas playerHUDCanvas;
    public void ActivateHUD() { playerHUDCanvas.enabled = true; }
    public void DeactivateHUD() { playerHUDCanvas.enabled = false; }

    public Text cameraModeButtonText;
    public Text sliceModeButtonText;
    public Text adaptiveSliceModeButtonText;
    public Text fullMapModeButtonText;

    public Image ghostedSlicesCheckBoxImage;
    public Text ghostedSlicesLabelText;
    public Button increaseGhostedSlicesButton;
    public Text numberOfGhostedSlicesText;
    public Button decreaseGhostedSlicesButton;


    private void Update() // Keep track of keyboard inputs that are shortcuts for button presses.
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!mainMenuCanvas.enabled)
                OpenMainMenu();
            else
                CloseMainMenu();
        }
    }

    private void OpenMainMenu()
    {
        GameManager.PauseGame();
        mainMenuCanvas.enabled = true;
    }
    private void CloseMainMenu()
    {
        GameManager.UnpauseGame();
        mainMenuCanvas.enabled = false;
    }

    // Main Menu
    public bool MissionStarted { get; private set; }
    public void GenerateMissionMap()
    {
        if (MissionStarted) // Stop it from regenerating a generated map.
            return;

        MissionStarted = true;
        StartCoroutine(Map.CreateMap());
        CloseMainMenu();
        ActivateHUD();
    }

    // Player HUD - Camera Control
    public void SwitchCameraMode()
    {
        if (CameraMovementManager.mode == CameraMovementManager.ViewMode.Top)
        {
            cameraModeButtonText.text = "Iso";
            CameraMovementManager.SwitchToIsoView();
        }
        else
        {
            cameraModeButtonText.text = "Top";
            CameraMovementManager.SwitchToTopView();
        }
    }
    public void SwitchToSliceMode()
    {
        Map.SetSliceRenderMode(SliceRenderMode.Slice);
    }
    public void SwitchToAdaptiveSliceMode()
    {
        Map.SetSliceRenderMode(SliceRenderMode.Adaptive);
    }
    public void SwitchToFullMapMode()
    {
        Map.SetSliceRenderMode(SliceRenderMode.Full);
    }
    public void ActivateGhostedLayers()
    {

    }


}
