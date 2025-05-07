using UnityEngine;

public class InteractableDoor : MonoBehaviour
{
    [SerializeField]
    private float m_rotateAngle = 90f;
    [SerializeField]
    private float m_coolTime = 0.5f;
    [SerializeField]
    private Transform m_Door;

    private bool m_isOpen = false;

    private float m_currentTime = 0f;

    public void InteractDoor()
    {
        if (m_currentTime < m_coolTime)
            return;

        m_currentTime = 0f;
        m_isOpen = !m_isOpen;
        m_Door.rotation = Quaternion.Euler(0, m_isOpen ? m_rotateAngle : 0, 0);
    }

    private void Update()
    {
        m_currentTime += Time.deltaTime;
    }
}
