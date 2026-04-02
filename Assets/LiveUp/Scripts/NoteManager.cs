using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    private Dictionary<int, List<Note>> laneNotes = new Dictionary<int, List<Note>>();
    private List<Note> allNotes = new List<Note>();

    // 🔥 chặn input nếu có miss
    private bool hasPendingMiss = false;

    void Awake()
    {
        Instance = this;
    }

    public void Register(Note note)
    {
        int lane = note.Lane;

        if (!laneNotes.ContainsKey(lane))
            laneNotes[lane] = new List<Note>();

        laneNotes[lane].Add(note);
        laneNotes[lane].Sort((a, b) => a.TargetTime.CompareTo(b.TargetTime));

        allNotes.Add(note);
        allNotes.Sort((a, b) => a.TargetTime.CompareTo(b.TargetTime));
    }

    public void Unregister(Note note)
    {
        int lane = note.Lane;

        if (laneNotes.ContainsKey(lane))
            laneNotes[lane].Remove(note);

        allNotes.Remove(note);
    }

    public Note GetBottomNote(int lane)
    {
        if (!laneNotes.ContainsKey(lane)) return null;

        var list = laneNotes[lane];
        if (list.Count == 0) return null;

        return list[0];
    }

    public Note GetFirstGlobal()
    {
        if (allNotes.Count == 0) return null;
        return allNotes[0];
    }

    // 🔥 gọi khi MISS xảy ra
    public void OnMiss()
    {
        hasPendingMiss = true;
    }

    // 🔥 reset khi note đã được remove
    public void ClearMiss()
    {
        hasPendingMiss = false;
    }

    public bool CanHit()
    {
        return !hasPendingMiss;
    }
}