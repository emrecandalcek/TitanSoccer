using UnityEngine;
using TMPro;

public class MatchChanceController : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI infoText;

    private MatchSceneUI matchUI;

    private bool isActive = false;
    private Vector2 startPos;
    private Vector2 currentPos;

    public void Begin(MatchSceneUI match)
    {
        matchUI = match;
        isActive = true;

        if (infoText != null)
            infoText.text = "Ekrana bas, geriye doğru sürükle ve bırak! (Güç ayarı)";
    }

    public void End()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        bool down = Input.GetMouseButtonDown(0);
        bool held = Input.GetMouseButton(0);
        bool up = Input.GetMouseButtonUp(0);
        Vector2 pointerPos = Input.mousePosition;
#else
        bool down = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool held = Input.touchCount > 0 &&
                    (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary);
        bool up = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
        Vector2 pointerPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Vector2.zero;
#endif

        if (down)
        {
            startPos = pointerPos;
            currentPos = pointerPos;
        }
        else if (held)
        {
            currentPos = pointerPos;
        }
        else if (up)
        {
            currentPos = pointerPos;
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector2 drag = startPos - currentPos;
        float power = drag.magnitude;

        if (power < 30f)
        {
            if (infoText != null)
                infoText.text = "Çok zayıf bir vuruş denedin. Bu pozisyon kaçtı.";
            End();
            // Çok zayıfsa direkt kötü şut: şut ekranına bile geçme istersen
            matchUI.OnShotFinalized(false);
            return;
        }

        // 0–1 arası güç skoru
        float clampedPower = Mathf.Clamp(power, 30f, 300f);
        float powerScore = (clampedPower - 30f) / (300f - 30f); // 0–1

        if (infoText != null)
            infoText.text = "Güç ayarlandı! Şut ekranı açılıyor...";

        End();

        // 2. aşama: Şut ekranını aç
        matchUI.OpenShotView(powerScore);
    }
}
