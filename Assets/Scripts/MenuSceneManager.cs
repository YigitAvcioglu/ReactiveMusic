using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneManager : MonoBehaviour
{
    public void LoadMainVRScene()
    {
        SceneManager.LoadScene("Main VR Scene");
    }

    public void LoadClubScene()
    {
        SceneManager.LoadScene("Club");
    }
}