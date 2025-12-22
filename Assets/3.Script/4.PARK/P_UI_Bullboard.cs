using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_UI_Bullboard : MonoBehaviour
{
    private Transform mainCam;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCam = Camera.main.transform;
        }
    }

    // LateUpdate: 카메라가 다 움직인 뒤에 따라가야 떨림이 없음
    void LateUpdate()
    {
        if (mainCam != null)
        {
            // [핵심] 캔버스의 앞면(forward)을 카메라의 앞면과 일치시킵니다.
            transform.forward = mainCam.forward;
        }
    }
}
