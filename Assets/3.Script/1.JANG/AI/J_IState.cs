using UnityEngine;
using UnityEngine.AI;


public interface J_IState
{
    // 상태 진입 시 1회 호출

    void Enter(J_AIController ai);



    // 상태 유지 중 매 프레임 호출

    void Execute(J_AIController ai);



    // 상태 이탈 시 1회 호출

    void Exit(J_AIController ai);
}

