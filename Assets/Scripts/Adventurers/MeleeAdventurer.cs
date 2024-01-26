using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MeleeAdventurer : Adventurer
{
    private Animator animator;
    [SerializeField]
    private Paddle paddle;
    private GameObject target;
    public SphereCollider attackCollider;
   
    public override void AttackTarget(GameObject target)
    {
        if (canAttack)
        {
            canAttack = false;
            paddle.SetYielder(this.gameObject);
            animator.SetTrigger("attack");
            attackCollider.enabled = true;
            this.target = target;
            OnAttack();
        }
    }

    private async void OnAttack()
    {
        //Cooldown of 1s
        await Task.Delay(1000);
        canAttack = true;
        attackCollider.enabled = false;
    }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        attackCollider.enabled = false;
        animator = GetComponent<Animator>();
        adventurerType = AdventurerType.Melee;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            AttackTarget(target);
        }   
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponentInChildren<Minotaur>().GetComponent<Rigidbody>()
                .AddForce((other.transform.position - transform.position).normalized * 10);
        }
    }
}
