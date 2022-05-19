using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttack : MonoBehaviour, AttackInstance
{
    private Vector3 _target;
    private DamageInfo _damageInfo;
    private ChampionStats _instigator;

    public float damageDelay;
    List<Collider> targets = new List<Collider>();

    public Vector3 target => _target;
    public DamageInfo damageInfo => _damageInfo;
    public ChampionStats instigator => _instigator;

    public void Initialize(ChampionStats instigator, Vector3 target, DamageInfo damageInfo)
    {
        _instigator = instigator;
        _target = target;
        _damageInfo = damageInfo;

        transform.rotation = Quaternion.LookRotation(target - instigator.transform.position, Vector3.up);
        transform.position = instigator.transform.position;
    }

    IEnumerator DelayDamage()
    {
        yield return new WaitForSeconds(damageDelay);
        foreach(Collider c in targets)
        {
            if(c.GetComponent<ChampionStats>() != null)
            {
                c.GetComponent<ChampionStats>().Damage(_instigator, _damageInfo);
            }
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        targets.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        targets.Remove(other);
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayDamage());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
