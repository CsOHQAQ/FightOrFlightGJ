using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyMoveable
{
    Rigidbody RB { get; set; }

    float RotationSpeed { get; set; }

    void MoveEnemy(Vector3 velocity);

}
