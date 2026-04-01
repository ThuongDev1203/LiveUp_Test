using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RhythmFinalTool : EditorWindow
{
    AudioClip audioClip;

    float duration = 30f;

    [Header("Timing")]
    bool autoDetectBPM = true;
    float bpm = 120f;
    float offset = 0.08f;
    int subdivision = 2;

    [Header("Detect")]
    float sensitivity = 1.2f;

    [Header("Gameplay")]
    int laneCount = 4;
    float minNoteGap = 0.25f;
    float randomSkipChance = 0.2f;

    List<float> peaks = new List<float>();
    List<NoteData> notes = new List<NoteData>();

    [MenuItem("Tools/Rhythm FINAL")]
    public static void Open()
    {
        GetWindow<RhythmFinalTool>("Rhythm FINAL");
    }

    void OnGUI()
    {
        audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio", audioClip, typeof(AudioClip), false);
        duration = EditorGUILayout.FloatField("Duration", duration);

        GUILayout.Space(10);
        GUILayout.Label("Timing", EditorStyles.boldLabel);

        autoDetectBPM = EditorGUILayout.Toggle("Auto BPM", autoDetectBPM);

        if (!autoDetectBPM)
            bpm = EditorGUILayout.FloatField("Manual BPM", bpm);

        offset = EditorGUILayout.Slider("Offset", offset, -0.2f, 0.2f);

        subdivision = EditorGUILayout.IntPopup("Subdivision",
            subdivision,
            new string[] { "1/4", "1/8", "1/16" },
            new int[] { 1, 2, 4 });

        GUILayout.Space(10);

        sensitivity = EditorGUILayout.Slider("Sensitivity", sensitivity, 0.8f, 2f);

        GUILayout.Space(10);

        GUILayout.Label("Gameplay", EditorStyles.boldLabel);
        laneCount = EditorGUILayout.IntSlider("Lane Count", laneCount, 2, 6);
        minNoteGap = EditorGUILayout.Slider("Min Gap", minNoteGap, 0.1f, 0.5f);
        randomSkipChance = EditorGUILayout.Slider("Random Skip", randomSkipChance, 0f, 0.5f);

        GUILayout.Space(10);

        if (GUILayout.Button("1. Detect Beat"))
            DetectPro();

        if (GUILayout.Button("2. Generate Notes"))
            Generate();

        if (GUILayout.Button("Export JSON"))
            Export();

        GUILayout.Label($"Peaks: {peaks.Count}");
        GUILayout.Label($"Notes: {notes.Count}");
    }

    // ================= PRO DETECT =================
    void DetectPro()
    {
        peaks.Clear();

        if (audioClip == null) return;

        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        int sampleRate = audioClip.frequency;
        int window = 1024;

        List<float> lowFlux = new List<float>();
        List<float> midFlux = new List<float>();

        float prevLow = 0f;
        float prevMid = 0f;

        for (int i = 0; i < samples.Length - window; i += window)
        {
            float low = 0f;
            float mid = 0f;

            for (int j = 0; j < window; j++)
            {
                float s = samples[i + j];

                float abs = Mathf.Abs(s);

                // fake band split (approximation)
                low += abs * (j < window * 0.3f ? 1f : 0.2f);
                mid += abs * (j >= window * 0.3f && j < window * 0.8f ? 1f : 0.2f);
            }

            low /= window;
            mid /= window;

            float lowDiff = Mathf.Max(0, low - prevLow);
            float midDiff = Mathf.Max(0, mid - prevMid);

            lowFlux.Add(lowDiff);
            midFlux.Add(midDiff);

            prevLow = low;
            prevMid = mid;
        }

        // ===== chọn mode =====
        float avgLow = Average(lowFlux);
        float avgMid = Average(midFlux);

        bool useLow = avgLow > avgMid; // 🔥 auto chọn beat hay vocal

        List<float> chosen = useLow ? lowFlux : midFlux;

        for (int i = 1; i < chosen.Count; i++)
        {
            if (chosen[i] > sensitivity * Average(chosen))
            {
                float time = (float)(i * window) / sampleRate;

                // chống spam
                if (peaks.Count > 0 && time - peaks[peaks.Count - 1] < 0.12f)
                    continue;

                peaks.Add(time);
            }
        }

        if (autoDetectBPM && peaks.Count > 10)
        {
            bpm = EstimateBPM(peaks);
            bpm = Mathf.Clamp(bpm, 60f, 180f);
        }

        Debug.Log($"Detected PRO Peaks: {peaks.Count} | Mode: {(useLow ? "BEAT" : "VOCAL")} | BPM: {bpm}");
    }

    float Average(List<float> list)
    {
        float sum = 0;
        foreach (var v in list) sum += v;
        return sum / list.Count;
    }

    // ================= GENERATE =================
    void Generate()
    {
        notes.Clear();

        float beat = 60f / bpm;
        float step = beat / subdivision;

        int lastLane = -1;
        float lastTime = -999f;

        HashSet<float> used = new HashSet<float>();

        foreach (var peak in peaks)
        {
            if (Random.value < randomSkipChance) continue;

            float t = Mathf.Round(peak / step) * step + offset;

            if (t > duration) continue;
            if (used.Contains(t)) continue;
            if (t - lastTime < minNoteGap) continue;

            int lane = Random.Range(0, laneCount);

            int safe = 0;
            while (lane == lastLane && safe < 10)
            {
                lane = Random.Range(0, laneCount);
                safe++;
            }

            used.Add(t);
            lastLane = lane;
            lastTime = t;

            notes.Add(new NoteData
            {
                time = t,
                lane = lane,
                duration = 0
            });
        }

        notes.Sort((a, b) => a.time.CompareTo(b.time));

        Debug.Log("Generated Notes: " + notes.Count);
    }

    // ================= BPM =================
    float EstimateBPM(List<float> beats)
    {
        Dictionary<int, int> hist = new Dictionary<int, int>();

        for (int i = 1; i < beats.Count; i++)
        {
            float diff = beats[i] - beats[i - 1];
            if (diff < 0.1f || diff > 1f) continue;

            int key = Mathf.RoundToInt(diff * 100);

            if (!hist.ContainsKey(key)) hist[key] = 0;
            hist[key]++;
        }

        int bestKey = 0;
        int bestCount = 0;

        foreach (var kv in hist)
        {
            if (kv.Value > bestCount)
            {
                bestCount = kv.Value;
                bestKey = kv.Key;
            }
        }

        float interval = bestKey / 100f;
        return 60f / interval;
    }

    // ================= EXPORT =================
    void Export()
    {
        if (notes.Count == 0)
        {
            Debug.LogError("No notes!");
            return;
        }

        SongData song = new SongData();
        song.offset = offset;
        song.notes = notes;

        string json = JsonUtility.ToJson(song, true);

        string path = EditorUtility.SaveFilePanel(
            "Save JSON",
            Application.dataPath,
            "notes.json",
            "json"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log("Saved JSON");
        }
    }
}
