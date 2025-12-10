using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;

    [Header("UI Referanslarý")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailText;
    public Button continueButton;
    public Button deleteButton;
    public Button newGameButton;

    private SaveData loadedData;
    private SaveSlotsMenu menu;

    public void Initialize(SaveSlotsMenu parentMenu)
    {
        menu = parentMenu;
    }

    private void Start()
    {
        if (menu == null)
        {
            menu = GetComponentInParent<SaveSlotsMenu>();
        }

        // Hangi referanslar eksik, baþta bir kere loglayalým
        if (titleText == null) Debug.LogError($"[SaveSlotUI] titleText atanmadý! Slot: {slotIndex}");
        if (detailText == null) Debug.LogError($"[SaveSlotUI] detailText atanmadý! Slot: {slotIndex}");
        if (continueButton == null) Debug.LogError($"[SaveSlotUI] continueButton atanmadý! Slot: {slotIndex}");
        if (deleteButton == null) Debug.LogError($"[SaveSlotUI] deleteButton atanmadý! Slot: {slotIndex}");
        if (newGameButton == null) Debug.LogError($"[SaveSlotUI] newGameButton atanmadý! Slot: {slotIndex}");

        Refresh();
    }

    public void Refresh()
    {
        // Koruma: UI referanslarý hiç yoksa boþuna devam etme
        if (titleText == null || detailText == null ||
            continueButton == null || deleteButton == null || newGameButton == null)
        {
            Debug.LogError($"[SaveSlotUI] Refresh çaðrýldý ama UI referanslarýndan biri NULL! Slot: {slotIndex}");
            return;
        }

        if (SaveSystem.HasSave(slotIndex))
        {
            loadedData = SaveSystem.LoadGame(slotIndex);

            titleText.text = $"{loadedData.playerName} - {loadedData.clubName}";
            detailText.text = $"Sezon {loadedData.season} | OVR {loadedData.overall}";
            continueButton.gameObject.SetActive(true);
            deleteButton.gameObject.SetActive(true);
            newGameButton.gameObject.SetActive(false);
        }
        else
        {
            loadedData = null;
            titleText.text = "Boþ Slot";
            detailText.text = "Yeni kariyer oluþtur.";
            continueButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            newGameButton.gameObject.SetActive(true);
        }
    }

    public void OnContinue()
    {
        Debug.Log($"[SaveSlotUI] Continue týklandý. Slot: {slotIndex}, loadedData null mu? {loadedData == null}, menu null mu? {menu == null}");
        if (loadedData == null || menu == null) return;

        menu.OnSlotContinue(loadedData, slotIndex);
    }

    public void OnDelete()
    {
        Debug.Log($"[SaveSlotUI] Delete týklandý. Slot: {slotIndex}");
        SaveSystem.DeleteSave(slotIndex);
        Refresh();
    }

    public void OnNewGame()
    {
        Debug.Log($"[SaveSlotUI] NewGame týklandý. Slot: {slotIndex}");
        if (menu == null) return;

        menu.OnSlotNewGame(slotIndex);
    }
}
