using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public System.Action<bool> HeldChanged;
    readonly HashSet<int> pointers = new HashSet<int>();

    public void OnPointerDown(PointerEventData data)
    {
        if (!GameSession.IsPlaying || !pointers.Add(data.pointerId)) return;
        if (pointers.Count == 1) HeldChanged?.Invoke(true);
    }

    public void OnPointerUp(PointerEventData data) { Release(data.pointerId); }
    public void OnPointerExit(PointerEventData data) { Release(data.pointerId); }

    void Release(int id)
    {
        if (pointers.Remove(id) && pointers.Count == 0) HeldChanged?.Invoke(false);
    }

    void OnDisable()
    {
        if (pointers.Count == 0) return;
        pointers.Clear();
        HeldChanged?.Invoke(false);
    }
}
