using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MenuAutoSetup : MonoBehaviour
{
    void Start()
    {
        SetupButton("MainVRButton", "Main VR Scene");
        SetupButton("ClubButton", "Club");
    }

    void SetupButton(string name, string scene)
    {
        GameObject go = GameObject.Find(name);
        if (go == null) return;

        Button btn = go.GetComponent<Button>();
        if (btn == null) return;

        // Visual Darkening (Fallback since VRButtonLinker is stuck)
        Image img = go.GetComponent<Image>();
        Color originalColor = img != null ? img.color : Color.white;

        XRSimpleInteractable interactable = go.GetComponent<XRSimpleInteractable>();
        if (interactable == null) interactable = go.AddComponent<XRSimpleInteractable>();

        interactable.hoverEntered.AddListener((args) => {
            if (img != null) img.color = originalColor * 0.5f;
        });
        interactable.hoverExited.AddListener((args) => {
            if (img != null) img.color = originalColor;
        });

        // Click logic
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            Debug.Log("Loading Scene: " + scene);
            SceneManager.LoadScene(scene);
        });

        // Collider
        BoxCollider col = go.GetComponent<BoxCollider>();
        if (col == null) col = go.AddComponent<BoxCollider>();
        RectTransform rt = go.GetComponent<RectTransform>();
        col.size = new Vector3(rt.rect.width, rt.rect.height, 10f);
        col.center = Vector3.zero;
    }
}
