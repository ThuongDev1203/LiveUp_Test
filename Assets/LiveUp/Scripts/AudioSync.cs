using UnityEngine;

public class AudioSync : MonoBehaviour
{
    public static AudioSync Instance;

    public AudioSource audioSource;
    public float offset = 0.05f;

    double dspStartTime;

    public bool IsPlaying { get; private set; } = false; // ✅ thêm dòng này

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        dspStartTime = AudioSettings.dspTime + 0.5;
        audioSource.PlayScheduled(dspStartTime);

        Invoke(nameof(SetPlaying), 0.5f); // ✅ bật IsPlaying sau khi nhạc bắt đầu
    }

    void SetPlaying()
    {
        IsPlaying = true;
    }

    public float SongTime
    {
        get
        {
            double time = AudioSettings.dspTime - dspStartTime;

            if (time < 0) return -1f; // 🚫 chưa tới giờ thì return âm

            return (float)time + offset;
        }
    }
}