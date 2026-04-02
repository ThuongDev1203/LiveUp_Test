using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    public RectTransform[] lanes;
    public RectTransform hitLine;

    public float noteSpeed = 800f;
    public float spawnOffsetY = 200f;

    SongData song;
    int index = 0;

    float travelTime;

    void Start()
    {
        song = JSONLoader.Load();

        if (song == null)
        {
            Debug.LogError("❌ Song NULL");
            return;
        }

        song.notes.Sort((a, b) => a.time.CompareTo(b.time));

        RectTransform lane = lanes[0];

        float hitY = lane.InverseTransformPoint(hitLine.position).y;
        float spawnY = lane.rect.height / 2f + spawnOffsetY;

        travelTime = (spawnY - hitY) / noteSpeed;
    }

    void Update()
    {
        if (song == null || NotePool.Instance == null) return;

        float current = AudioSync.Instance.SongTime;

        while (index < song.notes.Count)
        {
            var data = song.notes[index];

            // 🔥 bỏ note đã quá thời gian (tránh spawn lại)
            if (data.time < current - 0.1f)
            {
                index++;
                continue;
            }

            if (data.time - current <= travelTime)
            {
                Spawn(data);
                index++;
            }
            else break;
        }
    }

    void Spawn(NoteData data)
    {
        int laneIndex = Mathf.Clamp(data.lane, 0, lanes.Length - 1);
        RectTransform lane = lanes[laneIndex];

        GameObject obj = NotePool.Instance.Get();
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.SetParent(lane, false);
        rt.localScale = Vector3.one;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.sizeDelta = new Vector2(lane.rect.width - 20f, 350f);

        Note note = obj.GetComponent<Note>();
        note.Init(noteSpeed, data.time, hitLine, data.lane, spawnOffsetY);
    }
}