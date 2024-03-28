using UnityEngine;

public abstract class ItemControllerBase: MonoBehaviour
{
    protected int _pickupCount = 0;
    protected abstract void OnPickUp();

    protected void HideItem()
    {
        gameObject.SetActive(false);
    }

    public void PickUp()
    {
        if (CanPickUp())
        {
            _pickupCount++;
            OnPickUp();
            HideItem();
        }
    }

    protected virtual bool CanPickUp()
    {
        return _pickupCount == 0;
    }
}
