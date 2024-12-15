public interface ICharacter {
    // Basic attributes, which can be extended as needed
    float Health { get; set; }
    float MaxHealth { get; }

    // Provides methods to increase or decrease the character's health for convenient item use (e.g., healing potions)
    void AddHealth(float amount);
    void TakeDamage(float amount);
    
    
}
