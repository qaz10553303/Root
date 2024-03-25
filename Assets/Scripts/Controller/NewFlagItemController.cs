public class NewFlagItemController : ItemControllerBase
{
    public float scrollSpdAdd = 1f;
    public override void OnPickUp()
    {
        GameManager.Instance.AddNewRoot();
        GameManager.Instance.AddScrollSpeed(scrollSpdAdd);
        gameObject.SetActive(false);
    }
}