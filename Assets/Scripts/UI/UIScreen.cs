
using UnityEngine;

public class UIScreen : MonoBehaviour
{
    [SerializeField] public string screenId;
    
    public virtual void show()
    {
        gameObject.SetActive(true);
    }

    public virtual void hide()
    {
        gameObject.SetActive(false);
    }
}
