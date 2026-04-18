using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneController : MonoBehaviour
{
    public void LoadMainVR() => SceneManager.LoadScene("Main VR Scene");
    public void LoadClub() => SceneManager.LoadScene("Club");
}
