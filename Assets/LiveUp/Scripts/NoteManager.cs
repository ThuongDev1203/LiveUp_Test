using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    Dictionary<int, List<Note>> laneNotes = new Dictionary<int, List<Note>>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < 4; i++)
            laneNotes[i] = new List<Note>();
    }

    public void Register(Note note, int lane)
    {
        laneNotes[lane].Add(note);
    }

    public void Unregister(Note note, int lane)
    {
        laneNotes[lane].Remove(note);
    }

    // 🔥 LẤY NOTE GẦN HIT LINE NHẤT
    public Note GetBottomNote(int lane)
    {
        if (laneNotes[lane].Count == 0) return null;

        Note best = null;
        float lowestY = float.MaxValue;

        foreach (var n in laneNotes[lane])
        {
            float y = n.transform.position.y;

            if (y < lowestY)
            {
                lowestY = y;
                best = n;
            }
        }

        return best;
    }
}