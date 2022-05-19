using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunTest : Ability
{
    public GameObject projectileInstance;
    public float speed;
    public EImmboalize immobalize;
    public EStun stun;
    public EKnock knock;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BaseUpdate();
    }

    public override void AbilityButtonUp()
    {
        base.AbilityButtonDown();
        GameObject prInst = Instantiate(projectileInstance);
        prInst.transform.position = characterController.transform.position;
        prInst.GetComponent<Projectile>().owner = characterController;
        prInst.GetComponent<Projectile>().target = GetTargetPosition();
        prInst.GetComponent<Projectile>().speed = speed;
        prInst.GetComponent<Projectile>().EnemyHit += (controller) =>
        {
            controller.stats.StartCoroutine(RootStunKnock(controller));
            Destroy(prInst);
        };
        StartCooldown();
    }
    IEnumerator RootStunKnock(CharacterController controller)
    {
        controller.GetComponent<StatBlock>().AddStatusEffect(0, Instantiate(immobalize));
        yield return new WaitForSeconds(immobalize.duration);
        controller.GetComponent<StatBlock>().AddStatusEffect(0, Instantiate(stun));
        yield return new WaitForSeconds(stun.duration);
        EKnock kn = Instantiate(knock);
        kn.targetPosition = controller.transform.position;
        controller.GetComponent<StatBlock>().AddStatusEffect(0, kn);
    }
}
