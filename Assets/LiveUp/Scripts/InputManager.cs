using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Check(Input.mousePosition);
        }
    }

    void Check(Vector2 pos)
    {
        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = pos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            Note note = r.gameObject.GetComponent<Note>();
            if (note != null)
            {
                //note.TryHit();
                note.Hit();
                return;
            }
        }
    }
}