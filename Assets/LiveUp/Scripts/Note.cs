using UnityEngine;

public class Note : MonoBehaviour
{
    float speed;
    float targetTime;

    bool isHit;

    RectTransform rt;
    RectTransform hitLine;

    int lane;

    public RectTransform Rect => rt;
    public RectTransform HitLine => hitLine;
    public float TargetTime => targetTime;

    const float PERFECT = 0.08f;
    const float GOOD = 0.15f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void Init(float s, float time, RectTransform line, int laneIndex)
    {
        speed = s;
        targetTime = time;
        hitLine = line;
        lane = laneIndex;
        isHit = false;

        NoteManager.Instance.Register(this);
    }

    void Update()
    {
        if (isHit) return;

        // MOVE
        rt.anchoredPosition += Vector2.down * speed * Time.deltaTime;

        // MISS theo TIME (chuẩn rhythm game)
        float current = AudioSync.Instance.SongTime;
        float delta = current - targetTime;

        if (delta > GOOD)
        {
            Miss();
        }
    }

    public void TryHit()
    {
        if (isHit) return;

        var next = NoteManager.Instance.GetNextNote();

        // 🔥 CHỈ CHO HIT NOTE SỚM NHẤT TOÀN GAME
        if (next != this) return;

        float current = AudioSync.Instance.SongTime;
        float delta = Mathf.Abs(current - targetTime);

        if (delta <= PERFECT)
        {
            Hit(150);
        }
        else if (delta <= GOOD)
        {
            Hit(100);
        }
        else
        {
            Miss();
        }
    }

    void Hit(int score)
    {
        isHit = true;

        NoteManager.Instance.Unregister(this);
        gameObject.SetActive(false);

        // TODO: add score system here
    }

    void Miss()
    {
        if (isHit) return;

        isHit = true;

        NoteManager.Instance.Unregister(this);
        gameObject.SetActive(false);

        // TODO: add miss effect here
    }
}