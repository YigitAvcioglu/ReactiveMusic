using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public enum Views
{
    Landing = 0,
    Playlist = 1,
    Search = 2,
    LikedSongs = 3,
}

public class AppMainContentController : MonoBehaviour
{
    public Action<Views> OnViewChanged;
    /// <summary>
    /// All prefabs for every view. Need to be ordered by their number in Views enum
    /// </summary>
    public List<GameObject> ViewPrefabs;

    // Parent the views should be children of
    [SerializeField]
    private Transform _viewsParent;

    // Current view's controller
    private ViewControllerBase _currentViewController;

    private void Start()
    {
        // Destroy any initial children
        if (_viewsParent.transform.childCount > 0)
        {
            foreach(Transform child in _viewsParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Set default view
        SetViewFromEnum(Views.Landing);
    }

    public void SetContent(object expectedObject)
    {
        if (expectedObject is SimplePlaylist playlist)
        {
            MonoBehaviour viewController = SetViewFromEnum(Views.Playlist, (view) =>
            {
                (view as PlaylistViewController).SetPlaylist(playlist);
            });
        }
        else if (expectedObject is Views setView)
        {
            SetViewFromEnum(setView, null);
        }
        else
        {
            // Else if unknonwn object or null, display home screen
            SetViewFromEnum(Views.Landing);
        }
    }

    private MonoBehaviour SetViewFromEnum(Views viewEnum, Action<ViewControllerBase> intermediateActn = null)
    {
        OnViewChanged?.Invoke(viewEnum);
        if (_currentViewController != null)
        {
            Destroy(_currentViewController.gameObject);
            _currentViewController = null;
        }

        GameObject prefab = ViewPrefabs[(int)viewEnum];
        if (prefab)
        {
            // Set main view to playlist
            GameObject viewGO = Instantiate(prefab, _viewsParent);

            // Get view controller base, invoke any action
            _currentViewController = viewGO.GetComponent<ViewControllerBase>();
            if (!_currentViewController)
            {
                Debug.LogError($"View '{viewEnum}' doesn't inherit from ViewControllerBase!");
            }
            intermediateActn?.Invoke(_currentViewController);

            // --- Dinamik İçeriklere VR Lazer Desteği Ekle ---
            // Spawn edilen UI içerisindeki standart buton ve girdi alanlarına linkleri ekle.
            UnityEngine.UI.Button[] buttons = viewGO.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach(var b in buttons)
            {
                if (b.GetComponent<VRButtonLinker>() == null)
                {
                    b.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                    b.gameObject.AddComponent<VRButtonLinker>();
                }
            }

            UnityEngine.UI.InputField[] inputFields = viewGO.GetComponentsInChildren<UnityEngine.UI.InputField>(true);
            foreach(var i in inputFields)
            {
                if (i.GetComponent<VRButtonLinker>() == null)
                {
                    i.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                    i.gameObject.AddComponent<VRButtonLinker>();
                }
            }

            return _currentViewController;
        }

        return null;
    }
}
