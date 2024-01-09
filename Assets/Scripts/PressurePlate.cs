using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Image overlayImage;
    private bool isActive = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something hit plate");
        if(other.TryGetComponent<Player>(out Player player))
        {
            Debug.Log("Player hit plate, toggling active state to " + !isActive);
            isActive = !isActive;
            if (overlayImage != null)
            {
                Debug.Log("Changing overlay image color");
                overlayImage.color = isActive ? Color.white : new Color(0.2f, 0.2f, 0.2f);
            }
        }
    }
}
