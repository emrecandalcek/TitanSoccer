using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShotViewController : MonoBehaviour
{
    public Image ballImage;
    public TextMeshProUGUI infoText;

    private MatchSceneUI matchUI;
    private float powerScore;
    private bool waitingForShot = false;

    public void BeginShot(MatchSceneUI ui, float power)
    {
        matchUI = ui;
        powerScore = power;
        waitingForShot = true;

        if (infoText != null)
            infoText.text = "Topun neresine vurmak istiyorsan oraya dokun!";
    }

    private void Update()
    {
        if (!waitingForShot)
            return;

#if UNITY_EDITOR || UNITY_STANDALONE
        bool down = Input.GetMouseButtonDown(0);
        Vector2 pointerPos = Input.mousePosition;
#else
        bool down = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        Vector2 pointerPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Vector2.zero;
#endif

        if (down)
        {
            OnTap(pointerPos);
        }
    }

    private void OnTap(Vector2 screenPos)
    {
        if (ballImage == null)
        {
            FinishShot(false, "Top bulunamadý");
            return;
        }

        RectTransform rect = ballImage.rectTransform;

        Vector2 localPoint;
        // Ekran noktasýný topun local alanýna çeviriyoruz
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            screenPos,
            null,
            out localPoint))
        {
            FinishShot(false, "Þut dengesiz gitti");
            return;
        }

        // Topun merkezine göre normalize et (-1 ile 1 arasý)
        Rect r = rect.rect;
        float halfW = r.width * 0.5f;
        float halfH = r.height * 0.5f;

        if (halfW <= 0f || halfH <= 0f)
        {
            FinishShot(false, "Geçersiz top boyutu");
            return;
        }

        float nx = Mathf.Clamp(localPoint.x / halfW, -1f, 1f);
        float ny = Mathf.Clamp(localPoint.y / halfH, -1f, 1f);

        Vector2 dir = new Vector2(nx, ny);

        // Þimdi nereye vurduðuna göre yönü yorumlayalým:
        string directionDesc = "";

        bool high = ny > 0.3f;
        bool low = ny < -0.3f;
        bool left = nx < -0.3f;
        bool right = nx > 0.3f;

        if (high && left)
            directionDesc = "Sol üst köþeye bir þut gönderdin!";
        else if (high && right)
            directionDesc = "Sað üst köþeye bir þut gönderdin!";
        else if (high)
            directionDesc = "Kaleye doðru yükselen bir þut gönderdin!";
        else if (low && left)
            directionDesc = "Sol köþeye yerden bir þut gönderdin!";
        else if (low && right)
            directionDesc = "Sað köþeye yerden bir þut gönderdin!";
        else if (low)
            directionDesc = "Kaleye doðru yerden sert bir þut gönderdin!";
        else if (left)
            directionDesc = "Sol direk tarafýna bir þut gönderdin!";
        else if (right)
            directionDesc = "Sað direk tarafýna bir þut gönderdin!";
        else
            directionDesc = "Kalecinin tam üstüne bir þut gönderdin!";

        // Gol ihtimali hesabý:
        // Güç + yön kalitesine göre basit bir skor üretelim
        float directionQuality = 0.5f;

        // Üst köþeler en iyisi
        if ((high && left) || (high && right))
            directionQuality = 1.0f;
        else if (high || low)
            directionQuality = 0.8f;
        else if (left || right)
            directionQuality = 0.7f;
        else
            directionQuality = 0.4f; // kalecinin üstü

        // finalScore = güç %60 + yön %40
        float finalScore = powerScore * 0.6f + directionQuality * 0.4f;

        // Biraz da þans
        finalScore += Random.Range(-0.1f, 0.1f);
        finalScore = Mathf.Clamp01(finalScore);

        bool isGoal = finalScore > 0.55f;

        if (matchUI != null && matchUI.commentaryText != null)
        {
            matchUI.commentaryText.text = directionDesc;
        }

        FinishShot(isGoal, directionDesc);
    }

    private void FinishShot(bool isGoal, string description)
    {
        waitingForShot = false;

        if (infoText != null)
        {
            if (isGoal)
                infoText.text = description + "\nTop aðlarla buluþtu!";
            else
                infoText.text = description + "\nKaleci veya savunma bu þutu engelledi!";
        }

        if (matchUI != null)
        {
            matchUI.OnShotFinalized(isGoal);
        }
    }
}
