using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string GetPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetPath(slot));
    }

    public static void SaveGame(SaveData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }

    public static SaveData LoadGame(int slot)
    {
        string path = GetPath(slot);

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave(int slot)
    {
        string path = GetPath(slot);
        if (File.Exists(path))
            File.Delete(path);
    }
}
