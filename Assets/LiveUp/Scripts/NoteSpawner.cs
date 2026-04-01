using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [Header("Setup")]
    public RectTransform[] lanes;
    public RectTransform hitLine;

    [Header("Note Settings")]
    public float noteSpeed = 1200f; // tăng tốc cho giống Magic Tiles
    public float noteHeight = 220f; // NOTE DÀI HƠN

    SongData song;
    int index = 0;

    float travelTime;

    void Start()
    {
        song = JSONLoader.Load();

        if (song == null)
        {
            Debug.LogError("❌ SongData NULL");
            return;
        }

        song.notes.Sort((a, b) => a.time.CompareTo(b.time));

        // 👇 TÍNH travelTime CHUẨN theo vị trí hitLine
        float spawnY = 0f;
        float hitY = hitLine.anchoredPosition.y;

        float distance = Mathf.Abs(spawnY - hitY);
        travelTime = distance / noteSpeed;

        Debug.Log("TravelTime: " + travelTime);
    }

    void Update()
    {
        if (song == null || NotePool.Instance == null) return;

        float current = AudioSync.Instance.SongTime;

        if (current < 0.05f) return;

        while (index < song.notes.Count &&
               current >= song.notes[index].time - travelTime)
        {
            Spawn(song.notes[index]);
            index++;
        }
    }

    void Spawn(NoteData data)
    {
        RectTransform lane = lanes[data.lane];

        GameObject obj = NotePool.Instance.Get();
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.SetParent(lane, false);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        // ===== UI =====
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1f);

        // 👇 NOTE DÀI HƠN (GIỐNG MAGIC TILES)
        rt.sizeDelta = new Vector2(-20f, noteHeight);

        // spawn top
        rt.anchoredPosition = new Vector2(0, 0);

        Note note = obj.GetComponent<Note>();
        note.Init(noteSpeed, data.time, hitLine, data.lane);
    }
}