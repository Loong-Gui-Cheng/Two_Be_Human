using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*********************************************************************************
Written by: Loong Gui Cheng
Description: This class manages the music player played in the infotainment screen.
The user interface portion is separated into 'Mini' and 'Main':
1. 'Mini' (Prefixed by m_M) is a small panel where you can play Music and Choose favourite songs.
2. 'Main' is a control panel that stores all the in-depth and complex features of the player settings. (Usually as a separate page.)

Features:
- Play/Stop/Loop/Restart/Fast Foward/Rewind/Favourite Music 
- Switch Music Tracks
- Music Timeline
- Adjust Music Player Volume

Class relation:
Manipulates Music Track Data Scriptable Object class (Display Music Track & Audio Clip)
Manipulates Music Playlist Data Scriptable Object class (*Favourite Playlist)

NOTE: My apologies if some of the code here are confusing. 
*********************************************************************************/

/// <summary>
/// Manages the music played in the infotainment screen.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    #region Select Track Button
    [System.Serializable]
    private class SelectTrackButton
    {
        public Button button;
        public Image cover;

        public SelectTrackButton(Button button, Image cover)
        {
            this.button = button;
            this.cover = cover;
        }
    }
    #endregion

    // Identify which player is the initiator.
    public enum ID
    {
        NONE = -1,
        MINI_PLAYER = 0,
        MAIN_PLAYER = 1
    }

    // This class contains all the music features a player needs.
    // Note: Code may be messy due to the sheer number of UI variables that need to be updated.
    public ID m_PlayerID;

    [Header("User Interface Track (UI)")]
    [SerializeField] private RectTransform m_Panel;
    [SerializeField] private TextMeshProUGUI m_TrackTitleTMP;
    [SerializeField] private TextMeshProUGUI m_TrackCurrentLengthTMP;
    [SerializeField] private TextMeshProUGUI m_TrackTotalLengthTMP;
    [SerializeField] private TextMeshProUGUI m_MusicPlayerStatusTMP;
    [SerializeField] private TextMeshProUGUI m_FavoruitePageTextTMP;
    [SerializeField] private Image m_TrackCover;

    [Header("Mini-Player (UI)")]
    [SerializeField] private RectTransform m_MPanel;
    [SerializeField] private RectTransform m_MListPanel;
    [SerializeField] private TextMeshProUGUI m_MNumFavouriteSongsTMP;
    [SerializeField] private TextMeshProUGUI m_MTrackTitleTMP;
    [SerializeField] private TextMeshProUGUI m_MTrackCurrentLengthTMP;
    [SerializeField] private TextMeshProUGUI m_MTrackTotalLengthTMP;
    [SerializeField] private Image m_MTrackCover;
    [SerializeField] private Image m_MPauseIcon;

    [Header("User Interface Controller (UI)")]
    [SerializeField] private Image m_PauseIcon;
    [SerializeField] private Image m_FavouritedIcon;
    [SerializeField] private RectTransform m_VolumePanel;
    [SerializeField] private RectTransform m_FavouritePanel;
    [SerializeField] private RectTransform m_FavouriteScrollbar;

    [Header("Albums (BGM)")]
    [SerializeField] private List<MusicTrackData> tracks;
    private readonly List<MusicTrackData> m_LoadedTracks = new();

    [Header("User Favourites")]
    [SerializeField, Range(3f, 5f)] private int m_NumFavTracksPerPage;
    [SerializeField] private RectTransform m_FavouriteListParent;
    [SerializeField] private GameObject selectTrackPrefab;
    [SerializeField] private MusicPlaylistData favouritePlaylistData;
    private readonly List<SelectTrackButton> m_SelectFavourites = new();
    private readonly Dictionary<MusicTrackData.ID, Button> m_MiniFavourites = new();

    [Header("Top Controllers")]
    [SerializeField] private Button m_FavouriteSongButton;
    [SerializeField] private Toggle m_PlayToggle;

    [Header("Bottom Controllers")]
    [SerializeField] private Toggle m_LoopToggle;
    [SerializeField] private Slider m_TimelineSlider;
    [SerializeField] private Slider m_VolumeSlider;

    [Header("Other Controllers")]
    [SerializeField] private Toggle m_VolumePanelToggle;
    [SerializeField] private Toggle m_FavouriteListToggle;
    [SerializeField] private Button m_NextFavouriteButton;
    [SerializeField] private Button m_PreviousFavouriteButton;

    [Header("Mini-Player Controllers")]
    [SerializeField] private GameObject trackPrefab;
    [SerializeField] private Toggle m_MPlayToggle;
    [SerializeField] private Slider m_MTimelineSlider;


    // Traversal
    private int iCurrentTrack = -1;
    private int iCurrentFavouritePage = 0;
    private int iMaxFavouritePage = 1;
    private int iFavTracksPerPage = -1;

    private readonly Dictionary<MusicTrackData.ID, int> musicLibrary = new();

    // Animation Data
    private Vector3 m_VolumePanelOrigin;
    private Vector3 m_FavouritePanelOrigin;

    // State
    private AudioSource bgmSource;
    private bool isSoundPlaying;
    private bool isSongFinished;
    private float beforeSamples;

    private void Start()
    {
        bgmSource = GetComponents<AudioSource>()[1];
        iFavTracksPerPage = m_NumFavTracksPerPage;

        // Instantiate Favourite Track Prefabs
        for (int i = 0; i < iFavTracksPerPage; i++)
        {
            GameObject go = Instantiate(selectTrackPrefab, m_FavouriteListParent);
            if (!go.TryGetComponent(out Button button))
                break;

            Image cover = go.GetComponentInChildren<Image>();
            if (cover == null)
                break;

            SelectTrackButton STB = new(button, cover);
            m_SelectFavourites.Add(STB);
        }

        m_VolumePanelOrigin = new Vector3(m_VolumePanel.localPosition.x, m_VolumePanel.localPosition.y, m_VolumePanel.localPosition.z);
        m_FavouritePanelOrigin = new Vector3(m_FavouritePanel.localPosition.x, m_FavouritePanel.localPosition.y, m_FavouritePanel.localPosition.z);
        LoadSongs();
    }

    private void Update()
    {
        if (isSoundPlaying)
        {
            // BGM is still playing
            if (beforeSamples < bgmSource.timeSamples)
            {
                SetPlaybackTime(bgmSource.time);
            }
            // BGM is ending
            else if (beforeSamples >= bgmSource.timeSamples)
            {
                SetPlaybackTime(bgmSource.time);

                if (bgmSource.loop)
                    return;

                m_PlayToggle.isOn = false;
                m_MPlayToggle.isOn = false;
                bgmSource.Stop();
                isSongFinished = true;
            }
        }
    }

    private void LoadSongs()
    {
        if (tracks.Count <= 0) return;
        if (favouritePlaylistData == null) return;

        // Add music tracks to player & library.
        for (int i = tracks.Count - 1; i >= 0; i--)
        {
            // Auto-remove tracks from list when its invalid.
            MusicTrackData trackData = tracks[i];
            if (trackData == null || trackData.clip == null)
            {
                tracks.RemoveAt(i);
                Debug.LogWarning("[MP]: Invalid track data.");
                continue;
            }
            if (musicLibrary.ContainsKey(trackData.id))
            {
                tracks.RemoveAt(i);
                Debug.LogWarning("[MP]: Duplicated Music ID track data.");
                continue;
            }
            m_LoadedTracks.Add(trackData);

        }

        m_LoadedTracks.Reverse();
        for (int i = 0; i < m_LoadedTracks.Count; i++)
            musicLibrary.Add(m_LoadedTracks[i].id, i);

        iCurrentTrack = 0;
        SwitchTrack(m_LoadedTracks[iCurrentTrack]);

        iCurrentFavouritePage = 0;
        UpdateFavouritePage();

        for (int i = 0; i < favouritePlaylistData.tracks.Count; i++)
            AddFavouriteToMiniList(favouritePlaylistData.tracks[i]);
    }

    private void SwitchTrack(MusicTrackData track)
    {
        if (isSoundPlaying)
            bgmSource.Stop();

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        bgmSource.clip = track.clip;

        // Check if this track exists, if so, set current to this track.
        if (musicLibrary.TryGetValue(track.id, out int i))
            iCurrentTrack = i;

        // Calibrate track timeline to new length.
        float currentSeconds = 0f;

        if (m_PlayerID == ID.MAIN_PLAYER)
            currentSeconds = m_TimelineSlider.value * bgmSource.clip.length;

        else if (m_PlayerID == ID.MINI_PLAYER)
            currentSeconds = m_MTimelineSlider.value * bgmSource.clip.length;

        SetPlaybackTime(currentSeconds);

        // Update Track UI
        m_TrackTitleTMP.text = string.Format("{0} - {1}", track.artist, track.title);
        m_TrackCover.sprite = track.cover;
        m_TrackTotalLengthTMP.text = SecondsToTrackDuration(bgmSource.clip.length);
        m_MusicPlayerStatusTMP.text = string.Format("Playing: {0} - {1}", track.artist, track.title);

        // Update Mini-Player
        m_MTrackTitleTMP.text = m_TrackTitleTMP.text;
        m_MTrackCover.sprite = m_TrackCover.sprite;
        m_MTrackTotalLengthTMP.text = m_TrackTotalLengthTMP.text;

        if (m_TrackTitleTMP.text.Length > 15)
        {
            m_TrackTitleTMP.fontSize = 7f;
            m_MTrackTitleTMP.fontSize = 3.5f;
        }


        // Update Favourite Button UI
        if (favouritePlaylistData.DoesTrackIDExist(track.id))
            m_FavouritedIcon.gameObject.SetActive(true);
        else
            m_FavouritedIcon.gameObject.SetActive(false);


        if (isSoundPlaying) bgmSource.Play();
        else isSongFinished = true;
    }
    public void PreviousTrack()
    {
        if (m_LoadedTracks.Count <= 0) return;

        // Clamp current iterator to appropriate range.
        iCurrentTrack--;
        if (iCurrentTrack < 0)
            iCurrentTrack = m_LoadedTracks.Count - 1;

        SwitchTrack(m_LoadedTracks[iCurrentTrack]);
    }
    public void NextTrack()
    {
        if (m_LoadedTracks.Count <= 0) return;

        // Clamp current iterator to appropriate range.
        iCurrentTrack++;
        if (iCurrentTrack >= m_LoadedTracks.Count)
            iCurrentTrack = 0;

        SwitchTrack(m_LoadedTracks[iCurrentTrack]);
    }


    // Top Row
    public void FavouriteTrack()
    {
        MusicTrackData current = m_LoadedTracks[iCurrentTrack];
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        if (favouritePlaylistData.DoesTrackIDExist(current.id))
            RemoveFavourite(current);
        else
            AddFavourite(current);
    }
    private void AddFavourite(MusicTrackData track)
    {
        if (favouritePlaylistData == null) return;

        // Initialise favourite track prefab.
        // - Used to select favourite track from playlist.
        favouritePlaylistData.AddTrack(track);
        m_FavouritedIcon.gameObject.SetActive(true);
        UpdateFavouritePage();
        AddFavouriteToMiniList(track);
    }
    private void RemoveFavourite(MusicTrackData track)
    {
        if (favouritePlaylistData == null) return;

        // Remove favourite track prefab.
        favouritePlaylistData.RemoveTrack(track);
        m_FavouritedIcon.gameObject.SetActive(false);
        UpdateFavouritePage();
        RemoveFavouriteFromMiniList(track);
    }
    private void UpdateFavouritePage()
    {
        // Callback when adding or removing music track
        // 1. Update Total Page Count
        // 2. Update Interactable buttons
        // 3. Update UI in Music Page
        // 4. Update UI in Mini List

        #region 1. Update Total Page Count
        int iOldMaxPages = iMaxFavouritePage;
        iMaxFavouritePage = Mathf.CeilToInt((float)favouritePlaylistData.tracks.Count / (float)iFavTracksPerPage);

        if (iMaxFavouritePage == 0)
            iMaxFavouritePage = 1;

        // Clamps current page to new maximum page if its at the end.
        if (iCurrentFavouritePage == iOldMaxPages - 1)
            iCurrentFavouritePage = iMaxFavouritePage - 1;
        #endregion


        #region 2. Update Interactable buttons
        // Clamp page to appropriate range. Disable button if invalid index.
        if (iCurrentFavouritePage <= 0) iCurrentFavouritePage = 0;
        else if (iCurrentFavouritePage >= iMaxFavouritePage - 1) iCurrentFavouritePage = iMaxFavouritePage - 1;

        // Toggle page buttons based on current page.
        if (iCurrentFavouritePage == 0) m_PreviousFavouriteButton.interactable = false;
        else m_PreviousFavouriteButton.interactable = true;

        if (iCurrentFavouritePage == iMaxFavouritePage - 1) m_NextFavouriteButton.interactable = false;
        else m_NextFavouriteButton.interactable = true;
        #endregion


        #region 3. Update UI in Main
        // Convert from 2D index to 1D array format.
        int startingIndex = iCurrentFavouritePage * iFavTracksPerPage;
        int endIndex = (iCurrentFavouritePage * iFavTracksPerPage) + iFavTracksPerPage;
        int iSongButton = 0;

        for (int it = startingIndex; it < endIndex; it++)
        {
            SelectTrackButton STB = m_SelectFavourites[iSongButton];
            STB.button.onClick.RemoveAllListeners();
            STB.cover.enabled = false;

            // Terminate update to prevent invalid index.
            if (it >= favouritePlaylistData.tracks.Count)
            {
                iSongButton++;
                continue;
            }

            MusicTrackData favouriteTrack = favouritePlaylistData.tracks[it];
            if (favouriteTrack == null) continue;

            if (!musicLibrary.ContainsKey(favouriteTrack.id))
            {
                Debug.LogWarning(string.Format("[MP]: Please load {0} - {1} into the player beforehand!", favouriteTrack.artist, favouriteTrack.title));
                continue;
            }

            if (m_SelectFavourites[iSongButton] == null) break;

            STB.button.onClick.AddListener(() => { SwitchTrack(favouriteTrack); });
            STB.cover.sprite = favouriteTrack.cover;
            STB.cover.color = Color.white;
            STB.cover.enabled = true;
            iSongButton++;
        }
        #endregion

        m_FavoruitePageTextTMP.text = string.Format("{0} / {1}", iCurrentFavouritePage + 1, iMaxFavouritePage);
        m_MNumFavouriteSongsTMP.text = string.Format("{0} Song(s)", favouritePlaylistData.tracks.Count);
    }
    public void IterateFavouritePage(int i)
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        iCurrentFavouritePage += i;
        UpdateFavouritePage();
    }

    public void Rewind()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        if (bgmSource.clip == null) return;

        // Clamp music length to appropriate range.
        if (bgmSource.time <= 10f) SetPlaybackTime(0f);
        else SetPlaybackTime(bgmSource.time - 10f);
    }
    public void FastForward()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        if (bgmSource.clip == null) return;

        // Clamp music length to appropriate range.
        if (bgmSource.time >= bgmSource.clip.length - 10f) SetPlaybackTime(bgmSource.clip.length - 0.5f);
        else SetPlaybackTime(bgmSource.time + 10f);
    }
    public void ToggleMusic()
    {
        // 5 Cases
        // 1. Switch to new music
        // 2. Pause Music 
        // 3. Resume Music
        // Toggle.isOn = false/true (Activates function callback immediately)

        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        if (m_PlayerID == ID.MINI_PLAYER)
            m_PlayToggle.isOn = m_MPlayToggle.isOn;

        else if (m_PlayerID == ID.MAIN_PLAYER)
            m_MPlayToggle.isOn = m_PlayToggle.isOn;

        beforeSamples = bgmSource.timeSamples - 1f;

        // 1. Play New Music
        if (isSongFinished)
        {
            bgmSource.Play();
            isSongFinished = false;
            isSoundPlaying = true;
            m_PauseIcon.gameObject.SetActive(false);
            m_MPauseIcon.gameObject.SetActive(false);
            return;
        }

        // 2. Pause Existing Music
        if (isSoundPlaying)
        {
            bgmSource.Pause();
            isSoundPlaying = false;
            m_PauseIcon.gameObject.SetActive(true);
            m_MPauseIcon.gameObject.SetActive(true);
            return;
        }

        // 3. Resume Existing Music
        bgmSource.UnPause();
        isSoundPlaying = true;
        m_PauseIcon.gameObject.SetActive(false);
        m_MPauseIcon.gameObject.SetActive(false);
    }
    public void Replay()
    {
        // Restart Player
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        bgmSource.Stop();

        // Update Slider UI
        SetPlaybackTime(0f);

        if (m_PlayToggle.isOn)
        {
            m_PauseIcon.gameObject.SetActive(false);
            bgmSource.Play();
            return;
        }

        isSongFinished = true;
    }



    // Bottom Row
    public void Loop()
    {
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);
        bgmSource.loop = !bgmSource.loop;
    }
    public void AdjustTimeline()
    {
        if (bgmSource.clip == null) return;

        float currentTime = 0f;

        if (m_PlayerID == ID.MINI_PLAYER)
            currentTime = m_MTimelineSlider.value * bgmSource.clip.length;

        else if (m_PlayerID == ID.MAIN_PLAYER)
            currentTime = m_TimelineSlider.value * bgmSource.clip.length;

        SetPlaybackTime(currentTime);
    }
    public void AdjustVolume()
    {
        bgmSource.volume = m_VolumeSlider.value;
    }


    private void SetPlaybackTime(float position)
    {
        if (bgmSource.clip == null) return;
        bool IsWithinRange = position >= 0f && position < bgmSource.clip.length && position != bgmSource.time;

        if (IsWithinRange)
            bgmSource.time = position;

        beforeSamples = bgmSource.timeSamples - 1f;

        // Auto-Update Timeline UI, if panel is shown.
        if (m_Panel.gameObject.activeSelf || m_MPanel.gameObject.activeSelf)
        {
            float percentage = position / bgmSource.clip.length;
            m_TimelineSlider.value = percentage;
            m_MTimelineSlider.value = m_TimelineSlider.value;

            string currentTime = SecondsToTrackDuration(position);

            // Only update text when needed to avoid dirtying canvas as much as possible.
            if (string.Compare(currentTime, m_TrackCurrentLengthTMP.text) != 0)
            {
                m_TrackCurrentLengthTMP.text = currentTime;
                m_MTrackCurrentLengthTMP.text = m_TrackCurrentLengthTMP.text;
            }
        }
    }

    /// <summary>
    /// Converts Raw Track Length from Seconds to Track Duration (Minutes : Seconds). 
    /// </summary>
    private string SecondsToTrackDuration(float totalSeconds)
    {
        float UnitSeconds = totalSeconds; // Length in unit seconds
        float UnitMinutes = UnitSeconds / 60f; // Length in unit minutes
        int MinuteDuration = Mathf.FloorToInt(UnitMinutes); // Number of minutes
        int SecondDuration = Mathf.FloorToInt((UnitMinutes - MinuteDuration) * 60f); // Number of seconds

        if (SecondDuration < 10)
            return string.Format("{0}:0{1}", MinuteDuration, SecondDuration);

        return string.Format("{0}:{1}", MinuteDuration, SecondDuration);
    }

    private void AddFavouriteToMiniList(MusicTrackData trackData)
    {
        GameObject go = Instantiate(trackPrefab, m_MListPanel);
        if (!go.TryGetComponent(out Button button)) return;

        Image[] graphics = go.GetComponentsInChildren<Image>();
        TextMeshProUGUI[] texts = go.GetComponentsInChildren<TextMeshProUGUI>();
        if (graphics == null) return;
        if (texts == null) return;

        if (graphics.Length >= 2)
        {
            graphics[1].sprite = trackData.cover;
        }
        if (texts.Length >= 2)
        {
            texts[0].text = string.Format("{0} - {1}", trackData.title, trackData.artist);
            texts[1].text = string.Format("Genre: {0}", trackData.GetCategoryName());

            if (texts[0].text.Length > 20) texts[0].fontSize = 3f;
        }

        button.onClick.AddListener(() => SwitchTrack(trackData));
        m_MiniFavourites.Add(trackData.id, button);
    }
    private void RemoveFavouriteFromMiniList(MusicTrackData trackData)
    {
        if (!m_MiniFavourites.ContainsKey(trackData.id)) return;

        m_MiniFavourites.TryGetValue(trackData.id, out Button button);
        button.onClick.RemoveAllListeners();
        Destroy(button.gameObject);
        m_MiniFavourites.Remove(trackData.id);
    }

    // Animations
    public void DOTweenVolumePanel()
    {
        // Terminate existing animations, if any.
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        m_VolumePanel.DOComplete();
        m_VolumePanel.DOKill();

        if (m_VolumePanelToggle.isOn)
        {
            m_VolumePanel.DOLocalMoveY(18f, 0.3f, true);
            return;
        }
        m_VolumePanel.DOLocalMoveY(m_VolumePanelOrigin.y, 0.3f, true);
    }
    public void DOTweenFavouritePanel()
    {
        // Terminate existing animations, if any.
        AudioController.Instance.PlayUI(AudioController.SOUND_ID.CLICK);

        m_FavouritePanel.DOComplete();
        m_FavouritePanel.DOKill();

        if (m_FavouriteListToggle.isOn)
        {
            m_FavouritePanel.DOLocalMoveY(-40f, 0.5f, true);
            m_FavouriteScrollbar.gameObject.SetActive(true);
            return;
        }
        m_FavouritePanel.DOLocalMoveY(m_FavouritePanelOrigin.y, 0.5f, true);
        m_FavouriteScrollbar.gameObject.SetActive(false);
    }
}