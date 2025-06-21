using UnityEngine.Events;
using UnityEngine;

public class EventReceiver : MonoBehaviour
{
    [SerializeField] UnityEvent enableEvent;

    private void OnEnable()
    {
        enableEvent.Invoke();
    }
}
