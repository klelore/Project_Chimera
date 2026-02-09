using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private Animator animator;

    // 1. 提前把参数名转成 Hash，性能更高，也不容易写错字符串
    // 这里的名字要和你 Animator 面板里的 Parameters 一模一样
    private int isWalkingHash = Animator.StringToHash("IsWalk");
    //private int attackTriggerHash = Animator.StringToHash("Attack");
    //private int jumpTriggerHash = Animator.StringToHash("Jump");
    //private int speedFloatHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // 2. 通用的动作接口（处理触发类动画，比如攻击、跳跃、受伤）
    public void PlayAction(AnimAction action)
    {
        switch (action)
        {

        }
    }

    // 3. 状态类接口（处理持续性状态，比如跑、走、待机）
    // 你可以在这里封装逻辑，比如传入 true 就是跑，false 就是停
    public void SetWalking(bool isWalking)
    {
        animator.SetBool(isWalkingHash, isWalking);
    }

    //数值类接口
    public void UpdateMovementState(Vector3 velocity)
    {
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        bool isMoving = horizontalVelocity.magnitude > 0.1f;
        SetWalking(isMoving);
    }
}

// 定义动作枚举（只包含触发类的动作）
public enum AnimAction
{
    Attack,
    Jump,
    Walk,
    Idle,
}