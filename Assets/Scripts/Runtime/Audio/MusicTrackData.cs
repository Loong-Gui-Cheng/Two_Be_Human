using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MUSIC_TRACK_DATA", menuName = "Custom/Audio/MusicTrack")]
public class MusicTrackData : ScriptableObject
{
    public enum ID
    {
        SIGMA_PIANO = 0,
        CLAVIER_PIANO = 1,
        TRIA_AMBIENT = 2,
        SHADES_OF_ORANGE_AMBIENT = 3,
        GOOD_TIMES = 4
    }

    public enum CATEGORY_ID
    {
        METAL = 0,
        HIP_HOP = 1,
        LO_FI = 2,
        JAZZ = 3,
        EDM = 4,
        CLASSICAL = 5,
        PUNK = 6,
        VIDEO_GAME = 7,
        ARCADE = 8,
        AMBIENT = 9,
        CHILL = 10
    }

    [Header("Album Properties")]
    public ID id;
    public CATEGORY_ID category;
    public string title;
    public string artist;
    public Sprite cover;
    public AudioClip clip;

    public string GetCategoryName()
    {
        switch (category)
        {
            case CATEGORY_ID.METAL:
                return "Metal";

            case CATEGORY_ID.HIP_HOP:
                return "Hip-Hop";

            case CATEGORY_ID.LO_FI:
                return "Lo-Fi";

            case CATEGORY_ID.JAZZ:
                return "Jazz";

            case CATEGORY_ID.CLASSICAL:
                return "Classical";

            case CATEGORY_ID.PUNK:
                return "Punk";

            case CATEGORY_ID.VIDEO_GAME:
                return "Video Game";

            case CATEGORY_ID.ARCADE:
                return "Arcade";

            case CATEGORY_ID.AMBIENT:
                return "Ambient";

            case CATEGORY_ID.CHILL:
                return "Chill";
        }
        return string.Empty;
    }
}