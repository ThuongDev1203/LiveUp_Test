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

        // MISS
        if (current - targetTime > 0.3f)
        {
            Miss();
        }
    }

    public void TryHit()
    {
        if (isHit) return;

        // 🔥 1. check đúng lane (note dưới cùng)
        var bottom = NoteManager.Instance.GetBottomNote(laneIndex);
        if (bottom != this)
        {
            Debug.Log("❌ KHÔNG PHẢI NOTE DƯỚI CÙNG LANE");
            return;
        }

        // 🔥 2. check thứ tự global
        var globalFirst = NoteManager.Instance.GetFirstGlobal();
        if (globalFirst != this)
        {
            Debug.Log("❌ CHƯA ĐẾN LƯỢT NOTE NÀY");
            return;
        }

        float current = AudioSync.Instance.SongTime;
        float delta = Mathf.Abs(current - targetTime);

        if (delta <= 0.25f)
        {
            Debug.Log("✅ HIT ĐÚNG THỨ TỰ");
            Hit();
        }
        else
        {
            Debug.Log("💀 MISS");
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

        NoteManager.Instance.Unregister(this);
        NotePool.Instance.Return(gameObject);
    }

    void OnDisable()
    {
        isHit = false;
    }
}