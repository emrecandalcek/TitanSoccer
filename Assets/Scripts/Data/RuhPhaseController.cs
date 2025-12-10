using UnityEngine;
using TMPro;

public class RunPhaseController : MonoBehaviour
{
    [Header("Saha ve Oyuncu")]
    public RectTransform fieldRect;      // ChanceFieldBG
    public RectTransform playerIcon;     // RunPlayerIcon
    public TextMeshProUGUI infoText;     // ChanceInfoText

    [Header("Koşu Ayarları")]
    public float runSpeed = 800f;        // ekranda koşma hızı

    [Header("Şut Oku")]
    public RectTransform arrowRoot;      // ArrowRoot (ok görseli)
    public float maxArrowLength = 300f;  // maksimum çekme mesafesi

    private MatchSceneUI matchUI;

    private bool attackActive = false;
    private bool isMoving = false;
    private bool isAiming = false;

    private Vector2 moveTarget;
    private Vector2 aimDir;
    private float aimPower;

    public void BeginAttack(MatchSceneUI ui)
    {
        matchUI = ui;
        attackActive = true;
        isMoving = false;
        isAiming = false;

        if (arrowRoot != null)
            arrowRoot.gameObject.SetActive(false);

        if (infoText != null)
            infoText.text = "Sahaya dokun → hareket et. Oyuncunun üstüne basılı tut → okla şut çek!";
    }

    public void EndAttack()
    {
        attackActive = false;
        isMoving = false;
        isAiming = false;

        if (arrowRoot != null)
            arrowRoot.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!attackActive)
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
        bool up   = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
        Vector2 pointerPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Vector2.zero;
#endif

        if (down)
            OnPointerDown(pointerPos);

        if (held)
            OnPointerHeld(pointerPos);

        if (up)
            OnPointerUp(pointerPos);

        HandleMovement();
    }

    // ---------------- POINTER LOGIC ---------------- //

    private void OnPointerDown(Vector2 screenPos)
    {
        // Oyuncunun üstüne mi bastık?
        bool onPlayer = RectTransformUtility.RectangleContainsScreenPoint(
            playerIcon,
            screenPos,
            null
        );

        if (onPlayer)
        {
            // Şut aim moduna geç
            isAiming = true;
            isMoving = false;

            matchUI.OnRunStopped();   // zaman dursun

            if (arrowRoot != null)
            {
                arrowRoot.gameObject.SetActive(true);
                arrowRoot.anchoredPosition = playerIcon.anchoredPosition;
            }

            if (infoText != null)
                infoText.text = "Oku sürükle → yön ve güç belirle. Bırakınca şut!";
        }
        else
        {
            // Sahaya tıklandı → hareket et
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                fieldRect,
                screenPos,
                null,
                out localPoint))
            {
                Rect r = fieldRect.rect;
                localPoint.x = Mathf.Clamp(localPoint.x, r.xMin, r.xMax);
                localPoint.y = Mathf.Clamp(localPoint.y, r.yMin, r.yMax);

                moveTarget = localPoint;
                isMoving = true;
                isAiming = false;

                matchUI.OnRunStarted();   // zaman aksın

                if (arrowRoot != null)
                    arrowRoot.gameObject.SetActive(false);

                if (infoText != null)
                    infoText.text = "Oyuncun hedefe doğru koşuyor...";
            }
        }
    }

    private void OnPointerHeld(Vector2 screenPos)
    {
        if (!isAiming)
            return;

        // Ekran konumunu saha local koordinatına çevir
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            fieldRect,
            screenPos,
            null,
            out localPoint))
            return;

        // Oyuncunun local pozisyonu
        Vector2 playerPos = playerIcon.anchoredPosition;
        Vector2 delta = localPoint - playerPos;

        if (delta.sqrMagnitude < 1f)
            return;

        aimDir = delta.normalized;

        float len = Mathf.Clamp(delta.magnitude, 20f, maxArrowLength);
        aimPower = (len - 20f) / (maxArrowLength - 20f);  // 0–1 arası

        if (arrowRoot != null)
        {
            arrowRoot.anchoredPosition = playerPos;

            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
            arrowRoot.localEulerAngles = new Vector3(0, 0, angle);

            Vector2 size = arrowRoot.sizeDelta;
            size.y = len;
            arrowRoot.sizeDelta = size;
        }
    }

    private void OnPointerUp(Vector2 screenPos)
    {
        if (isAiming)
        {
            // Şut denemesi
            isAiming = false;

            if (arrowRoot != null)
                arrowRoot.gameObject.SetActive(false);

            if (infoText != null)
                infoText.text = "Şut çektin!";

            PerformShot();
        }
        else if (isMoving)
        {
            // Parmağı kaldırınca sadece koşmaya devam etsin
        }
    }

    // ---------------- HAREKET ---------------- //

    private void HandleMovement()
    {
        if (!isMoving)
            return;

        Vector2 current = playerIcon.anchoredPosition;
        Vector2 next = Vector2.MoveTowards(
            current,
            moveTarget,
            runSpeed * Time.unscaledDeltaTime
        );

        playerIcon.anchoredPosition = next;

        if (Vector2.Distance(next, moveTarget) < 2f)
        {
            isMoving = false;
            matchUI.OnRunStopped();   // zaman dursun

            if (infoText != null)
                infoText.text = "Tekrar sahaya dokun veya oyuncunun üstüne basılı tutup şut çek!";
        }
    }

    // ---------------- ŞUT HESAPLAMA ---------------- //

    private void PerformShot()
    {
        // Çok basit bir gol ihtimali hesabı (ileride geliştiririz)
        float baseChance = 0.3f;                 // minimum %30
        float finalChance = Mathf.Clamp01(baseChance + aimPower * 0.5f);

        bool isGoal = (Random.value < finalChance);

        if (matchUI != null)
        {
            if (isGoal)
                matchUI.commentaryText.text = "Müthiş bir şut gönderiyorsun... GOOOOL!";
            else
                matchUI.commentaryText.text = "Şutun kalecide kaldı veya auta gitti.";

            // Pozisyonu bitir → MatchSceneUI maça devam etsin
            matchUI.OnShotFinalized(isGoal);
        }

        // Attack modu kapanır
        EndAttack();
    }
}
