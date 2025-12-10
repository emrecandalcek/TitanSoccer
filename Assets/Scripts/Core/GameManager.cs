using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public SaveData CurrentSave;
    public int CurrentSaveSlotIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCurrentSave(SaveData data, int slotIndex)
    {
        CurrentSave = data;
        CurrentSaveSlotIndex = slotIndex;
    }
}
