using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class AbilityButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Button _button;
    [SerializeField] private Image _selectionHighlight; // Optional visual for selection

    private AbilitySO _ability;
    private AbilityManager _manager;

    public AbilitySO Ability => _ability;

    public void Initialize(AbilitySO ability, AbilityManager manager)
    {
        _ability = ability;
        _manager = manager;

        if (_ability.Icon != null)
        {
            _iconImage.sprite = _ability.Icon;
            _iconImage.color = _ability.AbilityColor;
        }

        _button.onClick.AddListener(OnButtonClicked);
        
        UpdateVisuals(false);
    }

    private void OnButtonClicked()
    {
        if (_manager != null)
        {
            _manager.EquipAbility(_ability);
        }
    }

    public void UpdateVisuals(bool isEquipped)
    {
        _button.interactable = !isEquipped;
        
        if (_selectionHighlight != null)
        {
            _selectionHighlight.enabled = isEquipped;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
    }
}
