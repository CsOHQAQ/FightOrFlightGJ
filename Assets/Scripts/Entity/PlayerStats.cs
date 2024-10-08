using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using UnityEngine;

/// <summary>
/// Just a prototype for testing.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private int _maxHealth;
    [SerializeField]
    private int _curHealth;
    [SerializeField]
    private int _attack;
    [SerializeField]
    private int _defense;


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
            int equipValue=0;
            foreach (var equip in inventory.GetEquipment()) 
            {
                if(equip!=null)
                if (equip.itemDurability > 0)
                {
                    equipValue += equip.attackValue;
                }
            }

            return _attack+equipValue;
        }
    }
    //This has count the value of equipments
    public int Defense
    {
        get
        {
            int equipValue = 0;
            foreach (var equip in inventory.GetEquipment())
            {
                if (equip != null)
                    if (equip.itemDurability > 0)
                {
                    equipValue += equip.defenceValue;
                }
            }

            return _defense + equipValue;
        }
    }

    InventoryManager inventory;

    private void Start()
    {
        inventory=InventoryManager.Instance;
    }
    public void TakeDamage(int damage)
    {
        _curHealth -= damage;
        _curHealth = Mathf.Max(_curHealth, 0);
    }

}
