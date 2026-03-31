using UnityEngine;

public class Note : MonoBehaviour
{
    public int lane;

    float targetTime;
    float spawnY;
    float hitY;
    float travelTime;

    bool isHit = false;

    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void Init(float time, float sY, float hY, float tTime, int laneIndex)
    {
        targetTime = time;
        spawnY = sY;
        hitY = hY;
        travelTime = tTime;
        lane = laneIndex;

        isHit = false;
    }

    void Update()
    {
        if (isHit) return;

        float current = AudioSync.Instance.SongTime;

        float t = 1f - (targetTime - current) / travelTime;
        t = Mathf.Clamp01(t);

        float y = Mathf.Lerp(spawnY, hitY, t);
        rt.anchoredPosition = new Vector2(0, y);

        // miss theo time
        if (current > targetTime + 0.2f)
        {
            Miss();
        }
    }

    public void Hit()
    {
        if (isHit) return;

        isHit = true;
        NotePool.Instance.Return(gameObject);
    }

    void Miss()
    {
        if (isHit) return;

        isHit = true;
        Debug.Log("Miss");

        NotePool.Instance.Return(gameObject);
    }
}