public class WaterItemController : ItemControllerBase
{
    private float waterAmount = 15f;
    public override void OnPickUp()
    {
        GameManager.Instance.AddWater(waterAmount);
        gameObject.SetActive(false);
    }
}
