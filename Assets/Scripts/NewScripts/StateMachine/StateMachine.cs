using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region State Machine Interfaces / Base Classes

/// <summary>
/// 所有能被StateMachine管理的角色或对象的接口
/// </summary>
public interface IStateMachineEntity
{
    // 可定义角色共有的一些属性或方法，例如:
    public StateMachine BaseStateMachine { get; }
    // ...
}

/// <summary>
/// 通用状态机基类，可被Player或Enemy复用
/// </summary>
public class StateMachine
{
    public BaseState CurrentState { get; private set; }
    
    public void Initialize(BaseState startState)
    {
        CurrentState = startState;
        CurrentState.Enter();
    }
    
    public void ChangeState(BaseState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
    
    public void Update()
    {
        CurrentState?.UpdateLogic();
    }
    
    public void FixedUpdate()
    {
        CurrentState?.UpdatePhysics();
    }
}

/// <summary>
/// 基本状态类，提供通用生命周期
/// </summary>
public abstract class BaseState
{
    protected StateMachine stateMachine;
    protected IStateMachineEntity owner;  // 持有者，可是Player或Enemy
    
    protected BaseState(IStateMachineEntity owner, StateMachine stateMachine)
    {
        this.owner = owner;
        this.stateMachine = stateMachine;
    }
    
    public virtual void Enter() { }
    public virtual void UpdateLogic() { }
    public virtual void UpdatePhysics() { }
    public virtual void Exit() { }

    public virtual void OnInteractInput(float scrollY)
    {
        // default does nothing
    }

    public virtual void OnMovementInput (Vector2 movementInput)
    {

    }

    public virtual void OnShootInput (Vector2 lookInput)
    {
        
    }
}

#endregion
