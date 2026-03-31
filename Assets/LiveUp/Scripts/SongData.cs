using System;
using System.Collections.Generic;

[Serializable]
public class SongData
{
    public float offset = 0.05f;
    public List<NoteData> notes = new List<NoteData>();
}