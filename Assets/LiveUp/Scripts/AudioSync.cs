using UnityEngine;

public class AudioSync : MonoBehaviour
{
    public static AudioSync Instance;

    public AudioSource audioSource;
    public float offset = 0.05f;

    double dspStartTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // delay để đảm bảo sync đúng (TRÁNH spawn 1 cục)
        dspStartTime = AudioSettings.dspTime + 0.5;
        audioSource.PlayScheduled(dspStartTime);
    }

    public float SongTime
    {
        get
        {
            return (float)(AudioSettings.dspTime - dspStartTime) + offset;
        }
    }
}