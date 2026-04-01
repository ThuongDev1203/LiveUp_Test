using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    private List<Note> allNotes = new List<Note>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(Note note)
    {
        allNotes.Add(note);
    }

    public void Unregister(Note note)
    {
        allNotes.Remove(note);
    }

    // 🔥 NOTE SỚM NHẤT (GLOBAL)
    public Note GetNextNote()
    {
        if (allNotes.Count == 0) return null;

        Note best = null;
        float earliest = float.MaxValue;

        foreach (var n in allNotes)
        {
            if (n.TargetTime < earliest)
            {
                earliest = n.TargetTime;
                best = n;
            }
        }

        return best;
    }
}