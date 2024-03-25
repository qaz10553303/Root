public class NewFlagItemController : ItemControllerBase
{
    public float scrollSpdAdd = 1f;
    public float waterDecrease = 2f;
    public override void OnPickUp()
    {
        GameManager.Instance.AddNewRoot();
        GameManager.Instance.AddScrollSpeed(scrollSpdAdd);
        GameManager.Instance.AddWaterDecrease(waterDecrease);
        gameObject.SetActive(false);
    }
}