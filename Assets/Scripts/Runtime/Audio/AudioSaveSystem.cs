using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class AudioSaveSystem
{
    public static void SaveAudioSettings(AudioSettingsConfig current)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/AudioSettings.dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        AudioSettingsConfig config = new AudioSettingsConfig(current);
        formatter.Serialize(stream, config);    
        stream.Close();
        Debug.Log("Saved at: " + Application.persistentDataPath);
    }

    public static AudioSettingsConfig LoadAudioSettings()
    {
        string path = Application.persistentDataPath + "/AudioSettings.dat";

        Debug.Log("path: " + path);
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            AudioSettingsConfig data = formatter.Deserialize(stream) as AudioSettingsConfig;
            stream.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }
}
