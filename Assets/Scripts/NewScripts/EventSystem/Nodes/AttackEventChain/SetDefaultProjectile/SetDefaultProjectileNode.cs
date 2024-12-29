using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDefaultProjectileNode : IEventNode<EventContext>
{
    public int Priority => 0;
    GameObject ProjectilePrefab;
    
    public SetDefaultProjectileNode(GameObject projectilePrefab)
    {
       if (projectilePrefab == null)
       {
        Debug.LogError("No Default Projectile Set");
       }
       ProjectilePrefab = projectilePrefab;
    }

    public void Process(EventContext context) {

        //
        if (context == null || context.AttackInfo == null) 
        {
            
            return;
        }
        
        context.AttackInfo.ProjectilePrefab=ProjectilePrefab;
        
    }
}
