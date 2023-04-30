using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animation : MonoBehaviour {
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent agent;

    // Start is called before the first frame update
    private void Start() {
    }

    public void SetStun(bool isStunned) {
        animator.SetBool("isStunned", isStunned);
    }

    public void PlayAttack() {
        animator.SetTrigger("Attack");
    }

    // Update is called once per frame
    private void Update() {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
