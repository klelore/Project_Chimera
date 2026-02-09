using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public AnimationManager animMgr;

    void Update()
    {
        // 移动逻辑
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Debug.Log(h + v);

        bool isMoving = (h != 0 || v != 0);

        // 调用 Manager：只需要传个 bool，不需要管 Animator 参数名
        animMgr.SetWalking(isMoving);

        // 如果你有混合树，也可以传速度
        // animMgr.UpdateSpeed(new Vector2(h, v).magnitude);
    }
}
