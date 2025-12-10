using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NewGameFlowUI : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public TMP_InputField playerNameInput;
    public TMP_Dropdown positionDropdown;
    public TMP_Dropdown clubDropdown;

    private void Start()
    {
        // Hangi referanslar boþ, baþta bir kere loglayalým
        if (playerNameInput == null) Debug.LogError("playerNameInput Inspector'da atanmadý!");
        if (positionDropdown == null) Debug.LogError("positionDropdown Inspector'da atanmadý!");
        if (clubDropdown == null) Debug.LogError("clubDropdown Inspector'da atanmadý!");

        // Pozisyon listesi
        if (positionDropdown != null)
        {
            positionDropdown.ClearOptions();
            positionDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "ST", "LW", "RW", "CAM", "CM", "CDM", "CB", "LB", "RB", "GK"
            });
        }

        // Baþlangýç kulüpleri
        if (clubDropdown != null)
        {
            clubDropdown.ClearOptions();
            clubDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Stalburg FC",
                "Draymond City",
                "Cloudbury United"
            });
        }
    }

    public void OnStartCareerButton()
    {
        Debug.Log("OnStartCareerButton çaðrýldý.");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager yok, akýþ bozuk!");
            return;
        }

        // --- EN ÖNEMLÝ KISIM: NULL KONTROL ---
        if (playerNameInput == null)
        {
            Debug.LogError("playerNameInput NULL! Inspector'da baðlaman lazým.");
            return;
        }
        if (positionDropdown == null)
        {
            Debug.LogError("positionDropdown NULL! Inspector'da baðlaman lazým.");
            return;
        }
        if (clubDropdown == null)
        {
            Debug.LogError("clubDropdown NULL! Inspector'da baðlaman lazým.");
            return;
        }
        // ---------------------------------------

        string playerName = playerNameInput.text;
        if (string.IsNullOrWhiteSpace(playerName))
        {
            Debug.Log("Ýsim boþ olamaz.");
            return;
        }

        string pos = positionDropdown.options[positionDropdown.value].text;
        string club = clubDropdown.options[clubDropdown.value].text;

        SaveData data = new SaveData();
        data.playerName = playerName;
        data.position = pos;
        data.clubName = club;
        data.season = 1;
        data.leaguePosition = 12;
        data.overall = 60;

        int slotIndex = GameManager.Instance.CurrentSaveSlotIndex;
        if (slotIndex < 0)
        {
            Debug.LogError("Geçerli slot index yok!");
            return;
        }

        SaveSystem.SaveGame(data, slotIndex);
        GameManager.Instance.SetCurrentSave(data, slotIndex);

        SceneManager.LoadScene("CareerHub");
    }
}
