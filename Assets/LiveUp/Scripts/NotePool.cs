using System.Collections.Generic;
using UnityEngine;

public class NotePool : MonoBehaviour
{
    public static NotePool Instance;

    public GameObject notePrefab;
    public int initialSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        Instance = this;

        if (notePrefab == null)
        {
            Debug.LogError("❌ NotePool: notePrefab NOT assigned!");
            return;
        }

        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(notePrefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(notePrefab, transform);
        }

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform); // reset parent
        pool.Enqueue(obj);
    }
}