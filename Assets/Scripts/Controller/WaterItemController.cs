public class WaterItemController : ItemControllerBase
{
    private float waterAmount = 50f;
    protected override void OnPickUp()
    {
        GameManager.Instance.AddWater(waterAmount);
        gameObject.SetActive(false);
    }
}