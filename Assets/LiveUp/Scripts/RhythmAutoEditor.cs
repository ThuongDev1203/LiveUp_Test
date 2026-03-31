// IMPROVED RHYTHM AUTO GENERATOR (BETTER SYNC)
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RhythmAutoEditor : EditorWindow
{
    AudioClip audioClip;
    AudioSource previewSource;

    float currentTime;
    bool isPlaying;

    float duration = 30f;
    int laneCount = 4;

    float sensitivity = 1.3f;

    List<NoteData> notes = new List<NoteData>();

    [MenuItem("Tools/Rhythm Auto Tool PRO")]
    public static void ShowWindow()
    {
        GetWindow<RhythmAutoEditor>("Rhythm Auto Tool PRO");
    }

    void OnEnable()
    {
        var go = new GameObject("AudioPreview");
        go.hideFlags = HideFlags.HideAndDontSave;
        previewSource = go.AddComponent<AudioSource>();
    }

    void OnDisable()
    {
        if (previewSource != null)
            DestroyImmediate(previewSource.gameObject);
    }

    void OnGUI()
    {
        GUILayout.Label("Audio", EditorStyles.boldLabel);

        audioClip = (AudioClip)EditorGUILayout.ObjectField(audioClip, typeof(AudioClip), false);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play")) Play();
        if (GUILayout.Button("Stop")) Stop();
        GUILayout.EndHorizontal();

        duration = EditorGUILayout.FloatField("Duration", duration);
        sensitivity = EditorGUILayout.Slider("Sensitivity", sensitivity, 0.5f, 2f);

        GUILayout.Label($"Time: {currentTime:F2}");

        if (GUILayout.Button("🔥 Auto Generate PRO"))
        {
            AutoGenerate();
        }

        if (GUILayout.Button("Export JSON"))
        {
            Export();
        }

        DrawTimeline();

        if (isPlaying)
            Repaint();
    }

    void Update()
    {
        if (isPlaying)
            currentTime = previewSource.time;
    }

    void Play()
    {
        if (audioClip == null) return;

        previewSource.clip = audioClip;
        previewSource.time = currentTime;
        previewSource.Play();

        isPlaying = true;
    }

    void Stop()
    {
        previewSource.Stop();
        isPlaying = false;
    }

    // ================= AUTO GENERATE =================

    void AutoGenerate()
    {
        notes.Clear();

        if (audioClip == null) return;

        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        int sampleRate = audioClip.frequency;

        int frameSize = 1024;
        int hopSize = 512;

        List<float> energy = new List<float>();

        // ===== 1. RMS =====
        for (int i = 0; i < samples.Length - frameSize; i += hopSize)
        {
            float sum = 0f;
            for (int j = 0; j < frameSize; j++)
            {
                float s = samples[i + j];
                sum += s * s;
            }
            energy.Add(Mathf.Sqrt(sum / frameSize));
        }

        // ===== 2. SPECTRAL FLUX (DIFFERENCE) =====
        List<float> flux = new List<float>();
        flux.Add(0);

        for (int i = 1; i < energy.Count; i++)
        {
            float value = Mathf.Max(0, energy[i] - energy[i - 1]);
            flux.Add(value);
        }

        // ===== 3. PEAK DETECT =====
        List<float> rawBeats = new List<float>();
        int window = 8;

        for (int i = window; i < flux.Count - 1; i++)
        {
            float avg = 0f;
            for (int j = i - window; j < i; j++) avg += flux[j];
            avg /= window;

            if (flux[i] > flux[i - 1] && flux[i] > flux[i + 1] && flux[i] > avg * sensitivity)
            {
                float time = (float)(i * hopSize) / sampleRate;
                rawBeats.Add(time);
            }
        }

        if (rawBeats.Count < 5)
        {
            Debug.LogWarning("Not enough beats!");
            return;
        }

        // ===== 4. BPM =====
        float bpm = EstimateBPM(rawBeats);
        float interval = 60f / bpm;

        Debug.Log("BPM: " + bpm);

        // ===== 5. QUANTIZE SOFT =====
        List<float> beats = new List<float>();
        float last = -999;

        foreach (var t in rawBeats)
        {
            float snap = Mathf.Round(t / interval) * interval;
            float final = Mathf.Lerp(t, snap, 0.6f);

            if (final - last > 0.15f)
            {
                beats.Add(final);
                last = final;
            }
        }

        // ===== 6. GENERATE NOTES =====
        int lane = 0;

        foreach (var t in beats)
        {
            lane = (lane + Random.Range(1, laneCount)) % laneCount;

            notes.Add(new NoteData
            {
                time = t,
                lane = lane,
                duration = 0
            });
        }

        notes.Sort((a, b) => a.time.CompareTo(b.time));

        Debug.Log("Generated: " + notes.Count);
    }

    float EstimateBPM(List<float> beats)
    {
        Dictionary<int, int> histogram = new Dictionary<int, int>();

        for (int i = 1; i < beats.Count; i++)
        {
            float diff = beats[i] - beats[i - 1];
            if (diff < 0.1f || diff > 1f) continue;

            int key = Mathf.RoundToInt(diff * 100);
            if (!histogram.ContainsKey(key)) histogram[key] = 0;
            histogram[key]++;
        }

        int bestKey = 0;
        int bestCount = 0;

        foreach (var kv in histogram)
        {
            if (kv.Value > bestCount)
            {
                bestCount = kv.Value;
                bestKey = kv.Key;
            }
        }

        float interval = bestKey / 100f;
        float bpm = 60f / interval;

        while (bpm < 80) bpm *= 2;
        while (bpm > 180) bpm /= 2;

        return bpm;
    }

    void DrawTimeline()
    {
        Rect rect = GUILayoutUtility.GetRect(800, 200);
        GUI.Box(rect, "");

        float laneWidth = rect.width / laneCount;

        foreach (var note in notes)
        {
            float x = rect.x + note.lane * laneWidth;
            float y = rect.y + (note.time / duration) * rect.height;

            EditorGUI.DrawRect(new Rect(x + 5, y, laneWidth - 10, 8), Color.green);
        }

        float playY = rect.y + (currentTime / duration) * rect.height;
        EditorGUI.DrawRect(new Rect(rect.x, playY, rect.width, 2), Color.red);
    }

    void Export()
    {
        SongData song = new SongData();
        song.offset = 0f;
        song.notes = notes;

        string json = JsonUtility.ToJson(song, true);

        string path = EditorUtility.SaveFilePanel("Save JSON", "", "notes.json", "json");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log("Saved JSON");
        }
    }
}
