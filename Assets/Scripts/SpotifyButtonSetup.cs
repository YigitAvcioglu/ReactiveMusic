using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;

public class SpotifyButtonSetup
{
    [MenuItem("Tools/Setup Spotify VR Toggle Button")]
    public static void SetupButton()
    {
        // Find existing SpotifyCanvas
        GameObject spotifyCanvas = GameObject.Find("SpotifyCanvas");
        if (spotifyCanvas == null)
        {
            Debug.LogError("SpotifyCanvas not found!");
            return;
        }

        // Create new Canvas
        GameObject canvasObj = new GameObject("SpotifyToggleButtonCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRt = canvasObj.GetComponent<RectTransform>();
        canvasRt.sizeDelta = new Vector2(300, 100);
        
        // Position it somewhat near the player or above the Spotify Canvas
        // Assuming SpotifyCanvas is at some position, put this button slightly above/left of it.
        canvasObj.transform.position = spotifyCanvas.transform.position + new Vector3(0, 1.5f, 0);
        canvasObj.transform.rotation = spotifyCanvas.transform.rotation;
        
        // Scale it down to VR proportions
        canvasRt.localScale = new Vector3(0.005f, 0.005f, 0.005f);

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // ADD VR RAYCASTER! VERY IMPORTANT FOR VR LASER INTERACTION!
        canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Create Button
        GameObject btnObj = new GameObject("ToggleBtn");
        btnObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.sizeDelta = new Vector2(280, 80);
        btnRt.anchoredPosition = Vector2.zero;

        Image bgImage = btnObj.AddComponent<Image>();
        bgImage.color = new Color(0.11f, 0.84f, 0.38f); // Spotify Green

        Button btn = btnObj.AddComponent<Button>();

        // Optional: Text
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(280, 80);
        
        Text text = textObj.AddComponent<Text>();
        text.text = "Toggle Spotify UI";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        // Add the Toggler Script
        SpotifyUIToggler toggler = canvasObj.AddComponent<SpotifyUIToggler>();
        toggler.targetCanvas = spotifyCanvas;

        // Wire up the button to the toggler
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, toggler.ToggleSpotifyUI);

        EditorGUIUtility.PingObject(canvasObj);
        Debug.Log("VR Toggle Button setup complete!");
    }
}
