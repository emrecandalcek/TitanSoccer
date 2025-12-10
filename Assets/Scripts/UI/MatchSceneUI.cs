using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MatchSceneUI : MonoBehaviour
{
    [Header("Skorboard UI")]
    public TextMeshProUGUI homeTeamNameText;
    public TextMeshProUGUI awayTeamNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    [Header("Saha / İstatistik UI")]
    public TextMeshProUGUI possessionText;

    [Header("Alt Panel / Maç Kontrolleri")]
    public GameObject startButton;
    public GameObject skipButton;
    public GameObject speedButton;
    public TextMeshProUGUI speedButtonText;
    public TextMeshProUGUI commentaryText;

    [Header("Pozisyon UI (1. Aşama: Güç)")]
    public GameObject chancePanel;
    public MatchChanceController chanceController;

    [Header("Şut Ekranı UI (2. Aşama: Yön)")]
    public GameObject shotViewPanel;
    public ShotViewController shotViewController;
    
    [Header("Koşu Fazı (Run Phase)")]
    public GameObject runPhaseRoot;
    public RunPhaseController runPhaseController;

    // Attack UI'de kullanacağımız Şut butonu
    public GameObject attackShootButton;

    private int homeScore = 0;
    private int awayScore = 0;

    private float matchTime = 0f;   // saniye cinsinden
    private float timeScale = 1f;
    private bool isPlaying = false;

    private float nextEventTime = 0f;
    private System.Random rng;

    private void Start()
    {
        rng = new System.Random();

        // Takım isimleri
        string homeName = "Ev Sahibi";
        string awayName = "Rakip FC";

        if (GameManager.Instance != null && GameManager.Instance.CurrentSave != null)
        {
            homeName = GameManager.Instance.CurrentSave.clubName;
        }

        if (homeTeamNameText != null) homeTeamNameText.text = homeName;
        if (awayTeamNameText != null) awayTeamNameText.text = awayName;

        if (scoreText != null) scoreText.text = "0 - 0";
        if (timeText != null) timeText.text = "00:00";
        if (possessionText != null) possessionText.text = "Topla oynama: %50 - %50";
        if (commentaryText != null) commentaryText.text = "Maç başlamak üzere...";

        if (speedButtonText != null) speedButtonText.text = "Hız: 1x";

        if (chancePanel != null)
            chancePanel.SetActive(false);

        if (shotViewPanel != null)
            shotViewPanel.SetActive(false);

        ScheduleNextEvent();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        // Zamanı ilerlet
        matchTime += Time.deltaTime * timeScale;

        int totalMinutes = Mathf.FloorToInt(matchTime / 60f);
        int seconds = Mathf.FloorToInt(matchTime % 60f);

        if (timeText != null)
            timeText.text = $"{totalMinutes:00}:{seconds:00}";

        // 90. dakika sonrası maçı bitir
        if (totalMinutes >= 90)
        {
            isPlaying = false;
            if (commentaryText != null)
                commentaryText.text = "Maç bitti! Skor: " + homeScore + " - " + awayScore;
            return;
        }

        // Rastgele olay zamanı
        if (matchTime >= nextEventTime)
        {
            TriggerRandomEvent(totalMinutes);
            ScheduleNextEvent();
        }
    }

    private void ScheduleNextEvent()
    {
        // 5–15 saniye arası rastgele olay aralığı
        float delay = Random.Range(5f, 15f);
        nextEventTime = matchTime + delay;
    }

    private void TriggerRandomEvent(int currentMinute)
    {
        int roll = rng.Next(0, 100);

        // %20 ihtimalle SENİN pozisyonun gelsin
        if (roll < 20)
        {
            StartPlayerChance();
            return;
        }

        // Basit gol / spiker olayları
        if (roll < 30)
        {
            // Ev sahibi gol atsın
            homeScore++;
            if (scoreText != null)
                scoreText.text = $"{homeScore} - {awayScore}";

            if (commentaryText != null)
                commentaryText.text = currentMinute + ". dakikada gol! Ev sahibi öne geçiyor!";
        }
        else if (roll < 40)
        {
            // Deplasman golü
            awayScore++;
            if (scoreText != null)
                scoreText.text = $"{homeScore} - {awayScore}";

            if (commentaryText != null)
                commentaryText.text = currentMinute + ". dakikada gol! Deplasman ekibi golü buldu!";
        }
        else if (roll < 70)
        {
            if (commentaryText != null)
                commentaryText.text = currentMinute + ". dakika: Orta sahada dengeli bir oyun var.";
        }
        else
        {
            if (commentaryText != null)
                commentaryText.text = currentMinute + ". dakika: Kanattan tehlikeli bir atak gelişiyor.";
        }

        // Topla oynama yüzdesi
        if (possessionText != null)
        {
            int homePoss = rng.Next(40, 61); // 40–60 arası
            int awayPoss = 100 - homePoss;
            possessionText.text = $"Topla oynama: %{homePoss} - %{awayPoss}";
        }
    }

    // --- 1. AŞAMA: ÇEK-BIRAK GÜÇ EKRANI ---

    public void StartPlayerChance()
    {
        // Pozisyon başladı: attack modu aktif
        if (commentaryText != null)
            commentaryText.text = "Tehlikeli bir atak! Sahaya dokun ya da Şut Çek!";

        // Maç zamanı başlangıçta duruyor, sadece hareket ederken akacak
        isPlaying = false;
        Time.timeScale = 1f;

        if (runPhaseRoot != null)
            runPhaseRoot.SetActive(true);

        if (runPhaseController != null)
            runPhaseController.BeginAttack(this);

        if (attackShootButton != null)
            attackShootButton.SetActive(true);
    }
    public void OnRunStarted()
    {
        // Oyuncu hareket ediyor → maç zamanı yavaş şekilde aksın
        isPlaying = true;
        Time.timeScale = 0.4f;
    }

    public void OnRunStopped()
    {
        // Oyuncu durdu → zaman dursun
        isPlaying = false;
        Time.timeScale = 1f;
    }

    public void OnAttackShootButton()
    {
        // Attack modunda şut başlat
        // Zamanı durdur
        isPlaying = false;
        Time.timeScale = 1f;

        // Koşu UI'lerini kapat
        if (runPhaseRoot != null)
            runPhaseRoot.SetActive(false);

        if (runPhaseController != null)
            runPhaseController.EndAttack();

        if (attackShootButton != null)
            attackShootButton.SetActive(false);

        if (commentaryText != null)
            commentaryText.text = "Şut gücünü ayarla! (Çek-bırak)";

        if (chancePanel != null && chanceController != null)
        {
            chancePanel.SetActive(true);
            chanceController.Begin(this);
        }
    }

    // MatchChanceController bittiğinde buraya powerScore gönderir
    public void OpenShotView(float powerScore)
    {
        // 1. paneli kapat
        if (chancePanel != null)
            chancePanel.SetActive(false);

        // Şut ekranını aç
        if (shotViewPanel != null)
            shotViewPanel.SetActive(true);

        if (commentaryText != null)
            commentaryText.text = "Şut ekranı: Topun neresine vurmak istiyorsan oraya dokun!";

        if (shotViewController != null)
            shotViewController.BeginShot(this, powerScore);

        // Arka planı hafif yavaşlatmak istersen (istersen koyarsın):
        Time.timeScale = 0.2f;
    }

    // --- 2. AŞAMA: ŞUT EKRANI SONUCU ---

    public void OnShotFinalized(bool isGoal)
    {
        if (shotViewPanel != null)
            shotViewPanel.SetActive(false);

        // Zamanı normale al
        Time.timeScale = 1f;

        isPlaying = true; // Maça devam

        if (isGoal)
        {
            homeScore++;
            if (scoreText != null)
                scoreText.text = $"{homeScore} - {awayScore}";

            if (commentaryText != null)
                commentaryText.text += " GOOOOOL!";
        }
        else
        {
            if (commentaryText != null)
                commentaryText.text += " ancak sonuç alamadın...";
        }
        // Attack tamamen kapansın
        if (runPhaseRoot != null)
            runPhaseRoot.SetActive(false);

        if (attackShootButton != null)
            attackShootButton.SetActive(false);

    }

    // --- BUTONLAR ---

    public void OnStartMatchButton()
    {
        Debug.Log("BUTON TIKLANDI");
        isPlaying = true;
        Time.timeScale = 1f;

        if (commentaryText != null)
            commentaryText.text = "Maç başladı!";
    }

    public void OnToggleSpeedButton()
    {
        if (timeScale == 1f)
            timeScale = 2f;
        else
            timeScale = 1f;

        if (speedButtonText != null)
            speedButtonText.text = $"Hız: {timeScale}x";
    }

    public void OnSkipMatchButton()
    {
        isPlaying = false;

        if (commentaryText != null)
            commentaryText.text = "Maç simüle edildi. Skor: " + homeScore + " - " + awayScore;

        // Şimdilik direkt CareerHub sahnesine dönüyoruz
        UnityEngine.SceneManagement.SceneManager.LoadScene("CareerHub");
    }

}
