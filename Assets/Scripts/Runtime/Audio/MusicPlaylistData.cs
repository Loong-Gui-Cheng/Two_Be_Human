using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PLAYLIST_DATA", menuName = "Custom/Audio/Playlist")]
public class MusicPlaylistData : ScriptableObject
{
    [Header("Playlist Properties")]
    public string playlistTitle;
    public string creator;
    public Sprite cover;
    public List<MusicTrackData> tracks;

    private Dictionary<MusicTrackData.ID, MusicTrackData> library;

    private void OnEnable()
    {
        library = new();

        for (int i = tracks.Count - 1; i >= 0; i--)
        {
            MusicTrackData track = tracks[i];
            if (track == null) continue;
            if (library.ContainsKey(track.id))
            {
                tracks.RemoveAt(i);
                continue;
            }

            library.Add(track.id, track);
        }
    }

    public void AddTrack(MusicTrackData track)
    {
        if (library.ContainsKey(track.id)) return;

        tracks.Add(track);
        library.Add(track.id, track);
    }
    public void RemoveTrack(MusicTrackData track)
    {
        if (!library.ContainsKey(track.id)) return;

        for (int i = tracks.Count - 1; i >= 0; i--)
        {
            if (tracks[i].id == track.id)
            {
                tracks.RemoveAt(i);
                break;
            }
        }
        library.Remove(track.id);
    }

    public bool DoesTrackIDExist(MusicTrackData.ID trackID)
    {
        if (library == null)
        {
            Debug.Log("Dictionary uninitialised.");
            return false;
        }

        if (library.ContainsKey(trackID))
            return true;

        return false;
    }
}