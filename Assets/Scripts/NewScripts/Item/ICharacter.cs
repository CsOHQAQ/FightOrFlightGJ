using System;
public interface ICharacter {
    // Basic attributes, which can be extended as needed
   

    float Health { get; set; }
    float MaxHealth { get; }

    Faction Faction{ get; }

    // Provides methods to increase or decrease the character's health for convenient item use (e.g., healing potions)
    void AddHealth(float amount);
    void TakeDamage(EventContext context);
    void Die();
    
    public AbilitySystemComponent GetAbilitySystemComponent();

    public event Action<ICharacter> OnCharacterDied;
}

public enum Faction{
    PLAYER,
    ENEMY,
    NONE,
}