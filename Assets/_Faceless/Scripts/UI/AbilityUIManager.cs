using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;

public class AbilityUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AbilityManager _abilityManager;
    [SerializeField] private AbilityButton _abilityButtonPrefab;
    [SerializeField] private Transform _buttonsContainer;
    [SerializeField] private Button _unequipAllButton;
    [SerializeField] private Transform _abilityPanel;
    [SerializeField] private InputActionReference _toggleAbilityPanelAction;

    private List<AbilityButton> _spawnedButtons = new List<AbilityButton>();

    private void Start()
    {

        if (_abilityManager == null)
        {
            Debug.LogError("AbilityUIManager: AbilityManager not found!");
            return;
        }

        // Subscribe to events
        _abilityManager.OnAbilityUnlocked += HandleAbilityUnlocked;
        _abilityManager.OnAbilityEquipped += HandleAbilityEquipped;

        // Setup Unequip All button
        if (_unequipAllButton != null)
        {
            _unequipAllButton.onClick.AddListener(OnUnequipAllClicked);
        }

        // Initialize existing abilities
        foreach (var ability in _abilityManager.UnlockedAbilities)
        {
            CreateButton(ability);
        }
        
        // Initial visual state
        // We assume nothing is equipped initially or we check manager (but manager doesn't expose logic for current ability type easily without event, 
        // actually we can't easily check 'current' type from public API yet, but typically it starts empty or we wait for an event)
        // Ideally we should sync with manager's current state if possible. 
        // For now, let's just default to all unselected until an event fires.
    }

    private void OnEnable()
    {
        _toggleAbilityPanelAction.action.performed += OnToggleAbilityPanel;
        _toggleAbilityPanelAction.action.canceled += OnToggleAbilityPanel;

        _toggleAbilityPanelAction.action.Enable();
    }

    private void OnDisable()
    {
        _toggleAbilityPanelAction.action.performed -= OnToggleAbilityPanel;
        _toggleAbilityPanelAction.action.canceled -= OnToggleAbilityPanel;

        _toggleAbilityPanelAction.action.Disable();
    }

    private void OnDestroy()
    {
        if (_abilityManager != null)
        {
            _abilityManager.OnAbilityUnlocked -= HandleAbilityUnlocked;
            _abilityManager.OnAbilityEquipped -= HandleAbilityEquipped;
        }
    }

    private void HandleAbilityUnlocked(AbilitySO ability)
    {
        CreateButton(ability);
    }

    private void CreateButton(AbilitySO ability)
    {
        // Don't create buttons for passive abilities if they shouldn't be selectable (Masks seem to be active abilities)
        // Assuming all abilities in the list should have buttons.
        
        AbilityButton btn = Instantiate(_abilityButtonPrefab, _buttonsContainer);
        btn.Initialize(ability, _abilityManager);
        _spawnedButtons.Add(btn);
    }

    private void HandleAbilityEquipped(AbilityType type)
    {
        foreach (var btn in _spawnedButtons)
        {
            bool isEquipped = (btn.Ability.Type == type);
            btn.UpdateVisuals(isEquipped);
        }
    }

    private void OnUnequipAllClicked()
    {
        _abilityManager.UnequipCurrentAbility();
    }

    private void OnToggleAbilityPanel(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _abilityPanel.DOScale(1.0f, 0.2f);
        }
        else if (context.canceled)
        {
            _abilityPanel.DOScale(0.0f, 0.2f);
        }
    }
}
