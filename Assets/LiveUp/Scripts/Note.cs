using UnityEngine;

public class Note : MonoBehaviour
{
    float speed;
    float targetTime;
    bool isHit;

    int laneIndex;

    RectTransform rt;

    float hitLineY;
    float spawnY;

    public float TargetTime => targetTime;
    public int Lane => laneIndex;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void Init(float s, float time, RectTransform hitLine, int lane, float offsetY)
    {
        speed = s;
        targetTime = time;
        laneIndex = lane;

        isHit = false;

        RectTransform parent = rt.parent as RectTransform;

        hitLineY = parent.InverseTransformPoint(hitLine.position).y;
        spawnY = parent.rect.height / 2f + offsetY;

        rt.anchoredPosition = new Vector2(0, spawnY);

        NoteManager.Instance.Register(this);
    }

    void Update()
    {
        if (isHit) return;

        float current = AudioSync.Instance.SongTime;
        float timeToHit = targetTime - current;

        float y = hitLineY + timeToHit * speed;
        rt.anchoredPosition = new Vector2(0, y);

        // 🔥 MISS tự động
        if (current - targetTime > 0.3f)
        {
            Miss();
        }
    }

    public void TryHit()
    {
        if (isHit) return;

        // 🔥 CHẶN nếu đang có miss chưa xử lý
        if (!NoteManager.Instance.CanHit())
        {
            Debug.Log("⛔ ĐANG CÓ MISS → KHÔNG CHO HIT");
            return;
        }

        var bottom = NoteManager.Instance.GetBottomNote(laneIndex);
        if (bottom != this) return;

        var globalFirst = NoteManager.Instance.GetFirstGlobal();
        if (globalFirst != this) return;

        float current = AudioSync.Instance.SongTime;
        float delta = Mathf.Abs(current - targetTime);

        if (delta <= 0.25f)
        {
            Debug.Log("✅ HIT");
            Hit();
        }
        else
        {
            Debug.Log("💀 MISS (tap sai)");
            Miss();
        }
    }

    void Hit()
    {
        if (isHit) return;

        isHit = true;

        NoteManager.Instance.Unregister(this);
        NotePool.Instance.Return(gameObject);
    }

    void Miss()
    {
        if (isHit) return;

        isHit = true;

        Debug.Log("💀 MISS");

        // 🔥 báo manager là đang miss
        NoteManager.Instance.OnMiss();

        NoteManager.Instance.Unregister(this);
        NotePool.Instance.Return(gameObject);

        // 🔥 clear miss ngay sau khi remove
        NoteManager.Instance.ClearMiss();
    }

    void OnDisable()
    {
        isHit = false;
    }
}