using UnityEngine;

public class SpotifyUIToggler : MonoBehaviour
{
    public GameObject targetCanvas;

    public void ToggleSpotifyUI()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(!targetCanvas.activeSelf);
        }
    }
}
