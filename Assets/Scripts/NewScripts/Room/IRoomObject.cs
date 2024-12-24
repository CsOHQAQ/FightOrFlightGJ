using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRoomObject
{
    public void OnCombatStartedInRoom(Room room);
    public void OnCombatEndedInRoom(Room room);
}
