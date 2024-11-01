using UnityEngine;
using UnityEngine.UI;

public class PowerMeter : MonoBehaviour
{
    [SerializeField] Image imageFill;
    public void SetImageFill(float amount)
    {
        imageFill.fillAmount = amount;
    }
}
