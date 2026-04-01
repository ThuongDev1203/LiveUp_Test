using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public RectTransform[] lanes;
    public RectTransform hitLine;

    public float baseSpeed = 800f;       // 👉 dùng để tính độ dài HOLD (CỐ ĐỊNH)
    public float noteSpeed = 800f;       // 👉 speed thật đang chạy
    public float noteHeight = 350f;

    public float speedIncreaseRate = 20f;
    public float maxSpeed = 1400f;

    SongData song;
    int index = 0;

    float travelTime;

    Dictionary<int, float> lastSpawnTime = new Dictionary<int, float>();
    float lastSpawnGlobalTime = -999f;

    const float MIN_LANE_GAP = 0.18f;
    const float MIN_GLOBAL_GAP = 0.12f;

    void Start()
    {
        song = JSONLoader.Load();

        if (song == null)
        {
            Debug.LogError("❌ Song NULL");
            return;
        }

        song.notes.Sort((a, b) => a.time.CompareTo(b.time));

        index = 0;

        UpdateTravelTime();

        Debug.Log("TravelTime: " + travelTime);
    }

    void Update()
    {
        if (song == null || NotePool.Instance == null) return;

        float current = AudioSync.Instance.SongTime;

        if (current < 0f) return;

        // 🔥 tăng tốc theo thời gian (chỉ ảnh hưởng movement)
        noteSpeed += speedIncreaseRate * Time.deltaTime;
        noteSpeed = Mathf.Min(noteSpeed, maxSpeed);

        // 🔥 update lại travelTime theo speed mới
        UpdateTravelTime();

        while (index < song.notes.Count)
        {
            float spawnTime = song.notes[index].time - travelTime;

            if (current >= spawnTime)
            {
                Spawn(song.notes[index]);
                index++;
            }
            else break;
        }
    }

    void Spawn(NoteData data)
    {
        int laneIndex = Mathf.Clamp(data.lane, 0, lanes.Length - 1);

        // chống dính global
        if (Mathf.Abs(data.time - lastSpawnGlobalTime) < MIN_GLOBAL_GAP)
            return;

        // chống dính lane
        if (!CanSpawn(laneIndex, data.time))
            return;

        RectTransform lane = lanes[laneIndex];

        GameObject obj = NotePool.Instance.Get();
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.SetParent(lane, false);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        // ===== UI chuẩn =====
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1f);

        // 🔥 TÍNH CHIỀU CAO NOTE (TAP / HOLD)
        float height = noteHeight;

        if (data.duration > 0f)
        {
            // 👉 dùng baseSpeed để giữ độ dài ổn định
            float holdDistance = data.duration * baseSpeed;

            // 👉 tránh quá ngắn hoặc quá dài
            height = Mathf.Clamp(holdDistance, noteHeight, 1500f);
        }

        rt.sizeDelta = new Vector2(-20f, height);

        // spawn từ trên
        rt.anchoredPosition = new Vector2(0, 0);

        Note note = obj.GetComponent<Note>();
        note.Init(noteSpeed, data.time, hitLine, laneIndex);

        lastSpawnTime[laneIndex] = data.time;
        lastSpawnGlobalTime = data.time;
    }

    bool CanSpawn(int lane, float time)
    {
        if (!lastSpawnTime.ContainsKey(lane))
            return true;

        return Mathf.Abs(time - lastSpawnTime[lane]) >= MIN_LANE_GAP;
    }

    void UpdateTravelTime()
    {
        float spawnY = lanes[0].rect.height;
        float hitY = GetHitLineLocalY(lanes[0]);

        float distance = Mathf.Abs(spawnY - hitY);
        travelTime = distance / noteSpeed;
    }

    float GetHitLineLocalY(RectTransform lane)
    {
        Vector3 worldPos = hitLine.position;
        Vector3 localPos = lane.InverseTransformPoint(worldPos);
        return localPos.y;
    }
}