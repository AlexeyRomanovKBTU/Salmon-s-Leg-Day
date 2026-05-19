using UnityEngine;
using System.IO;

public static class SaveSystem
{
    static string Path => Application.persistentDataPath + "/save.json";

    public static void Save(SaveData data)
    {
        File.WriteAllText(Path, JsonUtility.ToJson(data, true));
    }

    public static SaveData Load()
    {
        if (!File.Exists(Path)) return null;
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));
    }

    public static void Delete()
    {
        if (File.Exists(Path)) File.Delete(Path);
    }
}
