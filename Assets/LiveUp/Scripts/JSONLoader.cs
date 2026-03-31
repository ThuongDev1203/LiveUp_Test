using UnityEngine;

public static class JSONLoader
{
    public static SongData Load(string fileName = "notes")
    {
        TextAsset json = Resources.Load<TextAsset>(fileName);

        if (json == null)
        {
            Debug.LogError("❌ JSON NOT FOUND in Resources/");
            return null;
        }

        return JsonUtility.FromJson<SongData>(json.text);
    }
}