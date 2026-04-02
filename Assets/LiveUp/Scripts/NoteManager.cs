using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    // 🔥 mỗi lane 1 list
    private Dictionary<int, List<Note>> laneNotes = new Dictionary<int, List<Note>>();

    // 🔥 list global
    private List<Note> allNotes = new List<Note>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(Note note)
    {
        int lane = note.Lane;

        // 🔹 lane list
        if (!laneNotes.ContainsKey(lane))
        {
            laneNotes[lane] = new List<Note>();
        }

        laneNotes[lane].Add(note);
        laneNotes[lane].Sort((a, b) => a.TargetTime.CompareTo(b.TargetTime));

        // 🔹 global list
        allNotes.Add(note);
        allNotes.Sort((a, b) => a.TargetTime.CompareTo(b.TargetTime));
    }

    public void Unregister(Note note)
    {
        int lane = note.Lane;

        if (laneNotes.ContainsKey(lane))
        {
            laneNotes[lane].Remove(note);
        }

        allNotes.Remove(note);
    }

    // 🔥 lấy note dưới cùng trong lane
    public Note GetBottomNote(int lane)
    {
        if (!laneNotes.ContainsKey(lane)) return null;

        var list = laneNotes[lane];

        if (list.Count == 0) return null;

        return list[0];
    }

    // 🔥 lấy note sớm nhất toàn game
    public Note GetFirstGlobal()
    {
        if (allNotes.Count == 0) return null;

        return allNotes[0];
    }
}