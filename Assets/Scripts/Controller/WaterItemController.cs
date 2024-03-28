public class WaterItemController : ItemControllerBase
{
    private float waterAmount = 15f;
    protected override void OnPickUp()
    {
        GameManager.Instance.AddWater(waterAmount);
    }
}
