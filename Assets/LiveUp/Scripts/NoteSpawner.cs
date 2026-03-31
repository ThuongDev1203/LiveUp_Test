using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public RectTransform[] lanes;
    public RectTransform hitLine;

    public float spawnOffset = 300f;
    public float noteSpeed = 1000f;

    SongData song;
    int index = 0;

    float travelTime;
    float spawnY;

    void Start()
    {
        song = JSONLoader.Load();

        song.notes.Sort((a, b) => a.time.CompareTo(b.time));

        float laneHeight = lanes[0].rect.height;

        spawnY = laneHeight / 2f + spawnOffset;

        float distance = spawnY - hitLine.anchoredPosition.y;
        travelTime = distance / noteSpeed;
    }

    void Update()
    {
        float current = AudioSync.Instance.SongTime;

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

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        Note note = obj.GetComponent<Note>();
        note.Init(
            data.time,
            spawnY,
            hitLine.anchoredPosition.y,
            travelTime,
            data.lane
        );
    }
}