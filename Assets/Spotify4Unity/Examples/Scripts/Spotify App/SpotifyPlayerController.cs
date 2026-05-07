using SpotifyAPI.Web;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Spotify Player Controller script. Script to emulate the bottom player bar in Spotify
/// This is an example script of how/what you could/should implement
/// </summary>
public class SpotifyPlayerController : SpotifyPlayerListener
{
    // Main track image
    [SerializeField]
    private Image _trackIcon;
    // Current track name & artist text
    [SerializeField]
    private Text _trackName, _artistsNames;
    // Button to add/remove track to user's library
    [SerializeField]
    private Button _addToLibraryButton;

    // Player middle track progress left & right text
    [SerializeField]
    private Text _currentProgressText, _totalProgressText;
    // Player middle track progress bar
    [SerializeField]
    private Slider _currentProgressSlider;

    // Player middle media controls
    [SerializeField]
    private Button _playPauseButton, _previousButton, _nextButton, _shuffleButton, _repeatButton;

    // Player right volume slider
    [SerializeField]
    private Slider _volumeSlider;
    // Player right volume mute button
    [SerializeField]
    private Button _muteButton;

    [SerializeField]
    private Sprite _playSprite, _pauseSprite, _muteSprite, _unmuteSprite;

    // Is the current track in the user's library?
    private bool _currentItemIsInLibrary;
    // Did the user mouse down on the progress slider to edit the progress
    private bool _progressStartDrag = false;
    private bool _volumeStartDrag = false;
    // Current progress value when user is sliding the progress
    private float _progressDragNewValue = -1.0f;
    private float _volumeDragNewValue = -1.0f;
    // Last volume value before mute/unmute
    private int _volumeLastValue = -1;

    protected override void Awake()
    {
        base.Awake();

        // Listen to needed events on Awake
        this.OnPlayingItemChanged += this.PlayingItemChanged;
    }

    private void Start()
    {
        if (_playPauseButton != null)
        {
            _playPauseButton.onClick.AddListener(() => this.OnPlayPauseClicked());
            Debug.Log("[SpotifyPlayerController] PlayPause listener eklendi.");
        }
        else Debug.LogError("[SpotifyPlayerController] _playPauseButton NULL! Listener eklenemedi.");

        if (_previousButton != null)
        {
            _previousButton.onClick.AddListener(() => this.OnPreviousClicked());
            Debug.Log("[SpotifyPlayerController] Previous listener eklendi.");
        }
        else Debug.LogError("[SpotifyPlayerController] _previousButton NULL! Listener eklenemedi.");

        if (_nextButton != null)
        {
            _nextButton.onClick.AddListener(() => this.OnNextClicked());
            Debug.Log("[SpotifyPlayerController] Next listener eklendi.");
        }
        else Debug.LogError("[SpotifyPlayerController] _nextButton NULL! Listener eklenemedi.");

        if (_shuffleButton != null)
        {
            _shuffleButton.onClick.AddListener(() => this.OnToggleShuffle());
        }
        if (_repeatButton != null)
        {
            _repeatButton.onClick.AddListener(() => this.OnToggleRepeat());
        }

        if (_muteButton != null)
        {
            _muteButton.onClick.AddListener(() => this.OnToggleMute());
        }

        if (_addToLibraryButton != null)
        {
            _addToLibraryButton.onClick.AddListener(() => this.OnToggleAddToLibrary());
        }

        // Configure progress slider
        if (_currentProgressSlider != null)
        {
            // Enable only whole numbers, interaction
            _currentProgressSlider.wholeNumbers = true;
            _currentProgressSlider.interactable = true;

            // Listen to value change on slider
            _currentProgressSlider.onValueChanged.AddListener(this.OnProgressSliderValueChanged);
            // Add EventTrigger component, listen to mouse up/down events
            EventTrigger eventTrigger = _currentProgressSlider.gameObject.AddComponent<EventTrigger>();
            // Mouse Down event
            EventTrigger.Entry entry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener(this.OnProgressSliderMouseDown);
            eventTrigger.triggers.Add(entry);
            // Mouse Up event
            entry = new EventTrigger.Entry()
            {
                eventID = EventTriggerType.PointerUp
            };
            entry.callback.AddListener(this.OnProgressSliderMouseUp);
            eventTrigger.triggers.Add(entry);
        }

        if (_volumeSlider != null)
        {
            _volumeSlider.minValue = 0;
            _volumeSlider.maxValue = 100;
            _volumeSlider.wholeNumbers = true;
            _volumeSlider.interactable = true;
            _volumeSlider.onValueChanged.AddListener(this.OnVolumeSliderValueChanged);

            EventTrigger eventTrigger = _volumeSlider.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null) eventTrigger = _volumeSlider.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener(this.OnVolumeSliderMouseDown);
            eventTrigger.triggers.Add(entryDown);

            EventTrigger.Entry entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener(this.OnVolumeSliderMouseUp);
            eventTrigger.triggers.Add(entryUp);
        }

        // --- Lazerle tiklanabilmesi icin butonlara ve sliderlara VR destegi ekle ---
        Button[] allButtons = new Button[] { _playPauseButton, _previousButton, _nextButton, _shuffleButton, _repeatButton, _muteButton, _addToLibraryButton };
        foreach (var b in allButtons)
        {
            if (b != null && b.GetComponent<VRButtonLinker>() == null)
            {
                b.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                b.gameObject.AddComponent<VRButtonLinker>();
            }
        }
        
        Slider[] allSliders = new Slider[] { _currentProgressSlider, _volumeSlider };
        foreach (var s in allSliders)
        {
            if (s != null && s.GetComponent<VRSliderLinker>() == null)
            {
                s.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                s.gameObject.AddComponent<VRSliderLinker>();
            }
        }
    }

    private void Update()
    {
        CurrentlyPlayingContext context = GetCurrentContext();
        if (context != null)
        {
            // Update current position to context position when user is not dragging
            if (_currentProgressText != null && !_progressStartDrag)
            {
                _currentProgressText.text = S4UUtility.MsToTimeString(context.ProgressMs);
            }

            // Update Volume slider
            if (_volumeSlider != null)
            {
                _volumeSlider.minValue = 0;
                _volumeSlider.maxValue = 100;
                if (!_volumeStartDrag && context.Device.VolumePercent.HasValue)
                {
                    _volumeSlider.value = context.Device.VolumePercent.Value;
                }
            }

            // Update play/pause btn sprite with correct play/pause sprite
            if (_playPauseButton != null)
            {
                Transform iconTrans = _playPauseButton.transform.GetChild(0);
                Image playPauseImg = iconTrans.GetComponent<Image>();
                Text playPauseTxt = iconTrans.GetComponentInChildren<Text>();

                if (context.IsPlaying)
                {
                    if (_pauseSprite != null) playPauseImg.sprite = _pauseSprite;
                    if (playPauseTxt != null) playPauseTxt.text = "||";
                }
                else
                {
                    if (_playSprite != null) playPauseImg.sprite = _playSprite;
                    if (playPauseTxt != null) playPauseTxt.text = ">";
                }
            }

            FullTrack track = context.Item as FullTrack;
            if (track != null)
            {
                if (_totalProgressText != null)
                {
                    _totalProgressText.text = S4UUtility.MsToTimeString(track.DurationMs);
                }
                if (_currentProgressSlider != null)
                {
                    _currentProgressSlider.minValue = 0;
                    _currentProgressSlider.maxValue = track.DurationMs;

                    // Update position when user is not dragging slider
                    if (!_progressStartDrag)
                        _currentProgressSlider.value = context.ProgressMs;
                }
            }
        }
    }

    protected override async void PlayingItemChanged(IPlayableItem newPlayingItem)
    {
        if (newPlayingItem == null)
        {
            // No new item playing, reset UI
            UpdatePlayerInfo("No track playing", "No track playing", "");
            SetLibraryBtnIsLiked(false);

            _currentProgressSlider.value = 0;
            _totalProgressText.text = _currentProgressText.text = "00:00";
        }
        else
        {
            if (newPlayingItem.Type == ItemType.Track)
            {
                if (newPlayingItem is FullTrack track)
                {
                    // Update player information with track info
                    string allArtists = S4UUtility.ArtistsToSeparatedString(", ", track.Artists);
                    SpotifyAPI.Web.Image image = S4UUtility.GetLowestResolutionImage(track.Album.Images);
                    UpdatePlayerInfo(track.Name, allArtists, image?.Url);

                    // Make request to see if track is part of user's library if scope is authorized
                    var client = SpotifyService.Instance.GetSpotifyClient();
                    if (SpotifyService.Instance.AreScopesAuthorized(Scopes.UserLibraryRead))
                    {
                        try 
                        {
                            LibraryCheckTracksRequest request = new LibraryCheckTracksRequest(new List<string>() { track.Id });
                            var result = await client.Library.CheckTracks(request);
                            if (result.Count > 0)
                            {
                                SetLibraryBtnIsLiked(result[0]);
                            }
                        } 
                        catch (System.Exception e) 
                        {
                            // If it still fails, suppress the 403 or Forbidden warning to keep console clean
                            if (!e.Message.Contains("403") && !e.Message.Contains("Forbidden"))
                            {
                                Debug.LogWarning($"Could not check if track is in library: {e.Message}");
                            }
                            SetLibraryBtnIsLiked(false);
                        }
                    }
                    else
                    {
                        SetLibraryBtnIsLiked(false);
                    }
                }
            }
            else if (newPlayingItem.Type == ItemType.Episode)
            {
                if (newPlayingItem is FullEpisode episode)
                {
                    string creators = episode.Show.Publisher;
                    SpotifyAPI.Web.Image image = S4UUtility.GetLowestResolutionImage(episode.Images);
                    UpdatePlayerInfo(episode.Name, creators, image?.Url);
                }
            }
        }
    }

    // Updates the left hand side of the player (Artwork, track name, artists)
    private void UpdatePlayerInfo(string trackName, string artistNames, string artUrl)
    {
        if (_trackName != null)
        {
            _trackName.text = trackName;
        }
        if (_artistsNames != null)
        {
            _artistsNames.text = artistNames;
        }
        if (_trackIcon != null)
        {
            // Load sprite from url
            if (string.IsNullOrEmpty(artUrl))
            {
                _trackIcon.sprite = null;
            }
            else
            {
                // StartCoroutine yalnızca aktif GameObject'te çalışır.
                // PlayerBar veya SpotifyCanvas kapalıyken şarkı değişirse bu koruma devreye girer.
                if (gameObject.activeInHierarchy)
                {
                    StartCoroutine(S4UUtility.LoadImageFromUrl(artUrl, (loadedSprite) =>
                    {
                        _trackIcon.sprite = loadedSprite;
                    }));
                }
                else
                {
                    // Canvas açıldığında doğru artwork'ü göstermek için URL'yi sakla
                    _pendingArtUrl = artUrl;
                }
            }
        }
    }

    // Canvas kapalıyken gelen artwork URL'sini tutmak için
    private string _pendingArtUrl = null;

    private void OnEnable()
    {
        // Canvas tekrar açıldığında bekleyen artwork varsa yükle
        if (!string.IsNullOrEmpty(_pendingArtUrl) && _trackIcon != null)
        {
            string url = _pendingArtUrl;
            _pendingArtUrl = null;
            StartCoroutine(S4UUtility.LoadImageFromUrl(url, (loadedSprite) =>
            {
                _trackIcon.sprite = loadedSprite;
            }));
        }
    }

    
    private async void OnPlayPauseClicked()
    {
        Debug.Log("[SpotifyPlayerController] OnPlayPauseClicked çağrıldı!");
        SpotifyClient client = SpotifyService.Instance?.GetSpotifyClient();
        if (client == null)
        {
            Debug.LogError("[SpotifyPlayerController] Play/Pause: SpotifyClient null! Spotify bağlı değil.");
            return;
        }

        CurrentlyPlayingContext context = GetCurrentContext();
        Image playPauseImg = _playPauseButton.transform.GetChild(0).GetComponent<Image>();
        try
        {
            // Context null ise (henüz fetch edilmedi) çalmaya başlamayı dene
            bool isPlaying = context != null && context.IsPlaying;
            if (isPlaying)
            {
                Debug.Log("[SpotifyPlayerController] Pause gönderiliyor...");
                await client.Player.PausePlayback();
                if (playPauseImg != null) playPauseImg.sprite = _playSprite;
            }
            else
            {
                Debug.Log("[SpotifyPlayerController] Resume gönderiliyor...");
                await client.Player.ResumePlayback();
                if (playPauseImg != null) playPauseImg.sprite = _pauseSprite;
            }
        }
        catch (System.Exception e)
        {
            if (e.Message.Contains("Unexpected character encountered while parsing value"))
                Debug.Log($"[SpotifyPlayerController] Play/Pause API parsing error ignored: {e.Message}");
            else
                Debug.LogError($"[SpotifyPlayerController] Play/Pause başarısız (Premium gerekli veya aktif cihaz yok): {e.Message}");
        }
    }

    private async void OnPreviousClicked()
    {
        Debug.Log("[SpotifyPlayerController] OnPreviousClicked çağrıldı!");
        SpotifyClient client = SpotifyService.Instance?.GetSpotifyClient();
        if (client != null)
        {
            try { await client.Player.SkipPrevious(); }
            catch (System.Exception e) 
            { 
                if (e.Message.Contains("Unexpected character encountered"))
                    Debug.Log($"[SpotifyPlayerController] Previous API parsing error ignored: {e.Message}");
                else
                    Debug.LogError($"[SpotifyPlayerController] Previous başarısız: {e.Message}"); 
            }
        }
        else Debug.LogError("[SpotifyPlayerController] Previous: SpotifyClient null!");
    }

    private async void OnNextClicked()
    {
        Debug.Log("[SpotifyPlayerController] OnNextClicked çağrıldı!");
        SpotifyClient client = SpotifyService.Instance?.GetSpotifyClient();
        if (client != null)
        {
            try { await client.Player.SkipNext(); }
            catch (System.Exception e) 
            { 
                if (e.Message.Contains("Unexpected character encountered"))
                    Debug.Log($"[SpotifyPlayerController] Next API parsing error ignored: {e.Message}");
                else
                    Debug.LogError($"[SpotifyPlayerController] Next başarısız: {e.Message}"); 
            }
        }
        else Debug.LogError("[SpotifyPlayerController] Next: SpotifyClient null!");
    }

    private async void OnToggleShuffle()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        if (client != null)
        {
            try 
            {
                // get current shuffle state
                bool currentShuffleState = GetCurrentContext().ShuffleState;
                // Create request, invert state
                PlayerShuffleRequest request = new PlayerShuffleRequest(!currentShuffleState);

                await client.Player.SetShuffle(request);
            }
            catch (System.Exception e) { Debug.LogError($"Toggle Shuffle failed: {e.Message}"); }
        }
    }

    private async void OnToggleRepeat()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        CurrentlyPlayingContext context = this.GetCurrentContext();
        if(client != null && context != null)
        {
            try 
            {
                // Get current shuffle state
                string currentShuffleState = context.RepeatState;

                // Determine next shuffle state
                PlayerSetRepeatRequest.State newState = PlayerSetRepeatRequest.State.Off;
                switch (currentShuffleState)
                {
                    case "off":
                        newState = PlayerSetRepeatRequest.State.Track;
                        break;
                    case "track":
                        newState = PlayerSetRepeatRequest.State.Context;
                        break;
                    case "context":
                        newState = PlayerSetRepeatRequest.State.Off;
                        break;
                    default:
                        Debug.LogError($"Unknown Shuffle State '{currentShuffleState}'");
                        break;
                }

                // Build request and send
                PlayerSetRepeatRequest request = new PlayerSetRepeatRequest(newState);
                await client.Player.SetRepeat(request);
            }
            catch (System.Exception e) { Debug.LogError($"Toggle Repeat failed: {e.Message}"); }
        }
    }

    private async void OnToggleMute()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();
        var context = GetCurrentContext();
        if (context != null && client != null)
        {
            try
            {
                int? volume = context.Device.VolumePercent;
                int targetVolume;
                Image muteImg = _muteButton.transform.GetChild(0).GetComponent<Image>();
                if (volume.HasValue && volume > 0)
                {
                    // Set target volume to 0, sprite to muted
                    targetVolume = 0;
                    muteImg.sprite = _muteSprite;
                    // Save current volume for unmute press
                    _volumeLastValue = volume.Value;
                }
                else
                {
                    // Set target to last volume value before mute
                    if (_volumeLastValue > 0)
                    {
                        targetVolume = _volumeLastValue;
                        _volumeLastValue = -1;
                    }
                    else
                    {
                        // If no value, use default value
                        targetVolume = 25;
                    }

                    // Update sprite
                    muteImg.sprite = _unmuteSprite;
                }

                // Send request
                PlayerVolumeRequest request = new PlayerVolumeRequest(targetVolume);
                await client.Player.SetVolume(request);
            }
            catch (System.Exception e) { Debug.LogError($"Toggle Mute failed: {e.Message}"); }
        }
    }

    private async void OnToggleAddToLibrary()
    {
        SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

        // Get current context and check any are null
        CurrentlyPlayingContext context = this.GetCurrentContext();
        if (client != null && context != null)
        {
            List<string> ids = new List<string>();
            try
            {
                // Cast Item to correct type, add it's URI add make request
                if (context.Item.Type == ItemType.Track)
                {
                    FullTrack track = context.Item as FullTrack;
                    ids.Add(track.Id);

                    if (_currentItemIsInLibrary)
                    {
                        // Is in library, remove
                        LibraryRemoveTracksRequest removeRequest = new LibraryRemoveTracksRequest(ids);
                        await client.Library.RemoveTracks(removeRequest);

                        SetLibraryBtnIsLiked(false);
                    }
                    else
                    {
                        // Not in library, add to user's library
                        LibrarySaveTracksRequest removeRequest = new LibrarySaveTracksRequest(ids);
                        await client.Library.SaveTracks(removeRequest);

                        SetLibraryBtnIsLiked(true);
                    }
                }
                else if (context.Item.Type == ItemType.Episode)
                {
                    FullEpisode episode = context.Item as FullEpisode;
                    ids.Add(episode.Id);

                    if (_currentItemIsInLibrary)
                    {
                        LibraryRemoveShowsRequest request = new LibraryRemoveShowsRequest(ids);
                        await client.Library.RemoveShows(request);

                        SetLibraryBtnIsLiked(false);
                    }
                    else
                    {
                        LibrarySaveShowsRequest request = new LibrarySaveShowsRequest(ids);
                        await client.Library.SaveShows(request);

                        SetLibraryBtnIsLiked(true);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not update library status (403 Forbidden expected for unverified apps): {e.Message}");
            }
        }
    }

    private void OnProgressSliderMouseDown(BaseEventData arg0)
    {
        _progressStartDrag = true;
    }

    private void OnProgressSliderValueChanged(float newValueMs)
    {
        _progressDragNewValue = newValueMs;

        _currentProgressText.text = S4UUtility.MsToTimeString((int)_progressDragNewValue);
    }

    private void OnProgressSliderMouseUp(BaseEventData arg0)
    {
        if (_progressStartDrag && _progressDragNewValue > 0)
        {
            SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

            // Build request to set new ms position
            PlayerSeekToRequest request = new PlayerSeekToRequest((long)_progressDragNewValue);
            client.Player.SeekTo(request);

            // Set value in slider
            _currentProgressSlider.value = _progressDragNewValue;

            // Reset variables
            _progressStartDrag = false;
            _progressDragNewValue = -1.0f;
        }
    }

    private void OnVolumeSliderMouseDown(BaseEventData arg0)
    {
        _volumeStartDrag = true;
    }

    private void OnVolumeSliderValueChanged(float newValue)
    {
        _volumeDragNewValue = newValue;
    }

    private void OnVolumeSliderMouseUp(BaseEventData arg0)
    {
        if (_volumeStartDrag && _volumeDragNewValue >= 0)
        {
            SpotifyClient client = SpotifyService.Instance.GetSpotifyClient();

            PlayerVolumeRequest request = new PlayerVolumeRequest((int)_volumeDragNewValue);
            client.Player.SetVolume(request);

            _volumeSlider.value = _volumeDragNewValue;
            _volumeStartDrag = false;
            _volumeDragNewValue = -1.0f;
            Debug.Log($"[SpotifyPlayerController] Volume set to {_volumeSlider.value}");
        }
    }

    private void SetLibraryBtnIsLiked(bool isLiked)
    {
        _currentItemIsInLibrary = isLiked;

        if (_addToLibraryButton != null)
        {
            Image img = _addToLibraryButton.GetComponent<Image>();
            img.color = isLiked ? Color.green : Color.white;
        }
    }
}
