using UnityEngine;
using DG.Tweening;

public class WaterWall : MonoBehaviour, IFreezable, IBurnable
{
    [SerializeField] private Collider2D _waterWallCollider;
    [SerializeField] private SpriteRenderer _waterWallRenderer;

    private void Start()
    {
        _waterWallRenderer.DOFade(0.5f, 0.3f);

        gameObject.layer = LayerMask.NameToLayer("Water");
    }

    public void Burn()
    {
        _waterWallRenderer.DOFade(0.5f, 0.3f);

        gameObject.layer = LayerMask.NameToLayer("Water");

        Debug.Log("Water Wall has been burned and is now water.");
    }

    public void Freeze()
    {

        _waterWallRenderer.DOFade(1.0f, 0.3f);

        gameObject.layer = LayerMask.NameToLayer("Ice");

        Debug.Log("Water Wall has been frozen and is now ice.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Object entered Water Wall: " + collision.gameObject.name);
    }
}
