using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CareerHubUI : MonoBehaviour
{
    [Header("Profil UI")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI clubNameText;
    public TextMeshProUGUI seasonText;
    public TextMeshProUGUI overallText;

    [Header("Maç Bilgisi UI")]
    public TextMeshProUGUI nextMatchTitleText;
    public TextMeshProUGUI nextMatchTeamsText;
    public TextMeshProUGUI nextMatchTypeText;

    private void Start()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentSave == null)
        {
            Debug.LogError("CareerHub: CurrentSave bulunamadý!");
            if (playerNameText != null)
                playerNameText.text = "Kayýt bulunamadý";
            return;
        }

        var data = GameManager.Instance.CurrentSave;

        // Profil bilgileri
        if (playerNameText != null)
            playerNameText.text = $"{data.playerName} ({data.position})";

        if (clubNameText != null)
            clubNameText.text = data.clubName;

        if (seasonText != null)
            seasonText.text = $"Sezon {data.season}";

        if (overallText != null)
            overallText.text = $"OVR {data.overall}";

        // Maç bilgisi (þimdilik basit dummy veri)
        if (nextMatchTitleText != null)
            nextMatchTitleText.text = "Sýradaki Maç";

        if (nextMatchTeamsText != null)
            nextMatchTeamsText.text = $"{data.clubName} vs Rakip FC";

        if (nextMatchTypeText != null)
            nextMatchTypeText.text = "Lig Maçý";
    }

    public void OnGoToMatchButton()
    {
        Debug.Log("Maça Git butonuna basýldý! (Buradan Match sahnesine geçeceðiz)");
        SceneManager.LoadScene("MatchScene");
    }
}
