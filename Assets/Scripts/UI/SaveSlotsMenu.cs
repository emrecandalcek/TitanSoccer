using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotsMenu : MonoBehaviour
{
    [Header("Slot referanslarý")]
    public SaveSlotUI[] slotUIs;

    private void Awake()
    {
        // Her slota bu menüyü tanýt
        foreach (var slot in slotUIs)
        {
            if (slot != null)
                slot.Initialize(this);
        }
    }

    private void OnEnable()
    {
        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in slotUIs)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    // Slot doluysa "Devam Et" buradan gelir
    public void OnSlotContinue(SaveData data, int slotIndex)
    {
        GameManager.Instance.SetCurrentSave(data, slotIndex);
        SceneManager.LoadScene("CareerHub");
    }

    // Slot boþsa "Yeni Oyun" buradan gelir
    public void OnSlotNewGame(int slotIndex)
    {
        GameManager.Instance.CurrentSaveSlotIndex = slotIndex;
        GameManager.Instance.CurrentSave = null; // yeni kariyer
        SceneManager.LoadScene("NewGameFlow");
    }

    // Geri butonu
    public void OnBackButton()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
