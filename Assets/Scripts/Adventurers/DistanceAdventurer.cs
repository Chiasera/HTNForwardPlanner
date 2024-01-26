using System.Threading.Tasks;
using UnityEngine;

public class DistanceAdventurer : Adventurer
{
    [SerializeField]
    private Canon canon;
    private GameObject target;

    protected override void Awake()
    {
        base.Awake();
        adventurerType = AdventurerType.Ranged;
    }
    public override void AttackTarget(GameObject target)
    {
        if (canAttack)
        {
            canAttack = false;
            this.target = target;
            canon.ShootFrom(this.gameObject);
            OnAttack(target);
        }
    }

    private async void OnAttack(GameObject target)
    {
        //Cooldown of 1s
        await Task.Delay(1000);
        canAttack = true;
    }

    protected override void Update()
    {
        base.Update();
        if(target != null)
        {
            canon.AimAtTarget(target.transform.position);
        }
    }
}
