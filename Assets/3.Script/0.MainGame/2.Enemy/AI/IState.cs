using UnityEngine;
using UnityEngine.AI;


public interface IState
{
    // 상태 진입 시 1회 호출

    void Enter(AIController ai);



    // 상태 유지 중 매 프레임 호출

    void Execute(AIController ai);



    // 상태 이탈 시 1회 호출

    void Exit(AIController ai);
}

