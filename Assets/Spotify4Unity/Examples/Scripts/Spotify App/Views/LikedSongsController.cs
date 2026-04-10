using SpotifyAPI.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LikedSongsController : ViewControllerBase
{
    public float HeaderHeight = 300;
    public int MaximumLength = 1000;

    [SerializeField]
    private Transform _listViewParent;

    [SerializeField]
    private Transform _songsParent;

    [SerializeField]
    private GameObject _songPrefab;

    [SerializeField]
    private Text _subtitleText;

    [SerializeField]
    private Transform _headerParent, _loadParent;

    private List<SavedTrack> _allSavedTracks = new List<SavedTrack>();
    private string _creator;

    private List<Action> _dispatcher = new List<Action>();

    public async void Start()
    {
        _loadParent.gameObject.SetActive(true);
        _headerParent.gameObject.SetActive(false);
        _songsParent.gameObject.SetActive(false);

        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            Paging<SavedTrack> paging = await client.Library.GetTracks();

            // Load all saved tracks
            _allSavedTracks = await S4UUtility.GetAllOfPagingAsync(client, paging, MaximumLength) as List<SavedTrack>;

            // Only show cwer
            if (_allSavedTracks.Count > MaximumLength) {
                _allSavedTracks.RemoveRange(MaximumLength, (_allSavedTracks.Count - 1) - MaximumLength);
            }

            // Load current user to display creator
            PrivateUser profile = await SpotifyService.Instance.GetSpotifyClient().UserProfile.Current();
            _creator = profile.DisplayName;

            _dispatcher.Add(() =>
            {
                UpdateUI();
            });
        }
    }

    private void Update()
    {
        if (_dispatcher.Count > 0)
        {
            foreach(Action actn in _dispatcher)
            {
                actn.Invoke();
            }
            _dispatcher.Clear();
        }
    }

    void UpdateUI()
    {
        _loadParent.gameObject.SetActive(false);
        _headerParent.gameObject.SetActive(true);
        _songsParent.gameObject.SetActive(true);

        UpdateHeader();
        UpdateTracksList();
    }

    private void UpdateHeader()
    {
        if (_subtitleText != null)
        {
            _subtitleText.text = $"{_creator} | Recent {_allSavedTracks.Count} songs";
        }
    }

    private void UpdateTracksList()
    {
        // Clear any existing songs
        if (_songsParent.transform.childCount > 0)
        {
            foreach (Transform child in _songsParent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // Pre-calculate URIs array to support queueing for Next/Previous functionality
        System.Collections.Generic.List<string> allUris = new System.Collections.Generic.List<string>();
        for (int i = 0; i < _allSavedTracks.Count; i++) {
            allUris.Add(_allSavedTracks[i].Track.Uri);
        }

        for (int i = 0; i < _allSavedTracks.Count; i++)
        {
            SavedTrack savedTrack = _allSavedTracks[i];
            GameObject go = Instantiate(_songPrefab, _songsParent);

            SinglePlaylistSelectableTrack trackController = go.GetComponent<SinglePlaylistSelectableTrack>();
            
            // Spotify API allows maximum 100 URIs in a play request.
            // Setup a window of 50 previous and 50 next tracks so Next/Prev buttons work.
            int startIndex = System.Math.Max(0, i - 50);
            int count = System.Math.Min(100, allUris.Count - startIndex);
            var passedUris = allUris.GetRange(startIndex, count);

            trackController.SetTrack(savedTrack.Track, "", passedUris);

            // --- Lazerle Tiklanabilmesi Icin Liked Songs Sarkilarina VR Destegi Ekle ---
            UnityEngine.UI.Button[] buttons = go.GetComponentsInChildren<UnityEngine.UI.Button>(true);
            foreach(var b in buttons)
            {
                if (b.GetComponent<VRButtonLinker>() == null)
                {
                    b.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                    b.gameObject.AddComponent<VRButtonLinker>();
                }
            }
        }

        // Get height of prefab
        RectTransform t = _songPrefab.transform.GetComponent<RectTransform>();
        float singlePrefabHeight = t.rect.height;

        // Determine total new height of parent
        float totalHeight = (_allSavedTracks.Count * singlePrefabHeight) + HeaderHeight;

        RectTransform parentRect = _listViewParent.GetComponent<RectTransform>();
        // Set parent's new height for scrolling
        parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
    }
}
