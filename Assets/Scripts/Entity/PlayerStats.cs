using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Just a prototype for testing.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private RectTransform _healthBarFill;
    [SerializeField]
    private RectTransform _healthBarContainer;
    [SerializeField]
    private int _maxHealth;
    [SerializeField]
    private int _curHealth;
    [SerializeField]
    private int _attack;
    [SerializeField]
    private int _defense;
    [SerializeField]
    private List<TRAIT_TARGET> traits;



    public string Name;
    public int MaxHealth 
    {
        get
        {
            return _maxHealth;
        }
    }
    public int CurHealth
    {
        get
        {
            return _curHealth;
        }
    }
    //This has count the value of equipments
    public int Attack
    {
        get
        {
            return _attack+inventory.GetEquippedItemStats()[0];
        }
    }
    //This has count the value of equipments
    public int Defense
    {
        get
        {
            return _defense + +inventory.GetEquippedItemStats()[1];
        }
    }

    InventoryManager inventory;

    private void Start()
    {
        inventory=InventoryManager.Instance;
        UpdateHealthBar();
    }
    public void TakeDamage(int damage)
    {
        _curHealth -= damage;
        _curHealth = Mathf.Clamp(_curHealth, 0, MaxHealth);
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        float fillAmount = (float)CurHealth / MaxHealth;

        float newWidth = fillAmount * _healthBarContainer.rect.width;
        _healthBarFill.sizeDelta = new Vector2(newWidth, _healthBarFill.sizeDelta.y);
    }

}
