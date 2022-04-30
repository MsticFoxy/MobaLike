using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MyBox;

public class DashObserver
{
    public virtual void OnDashStart(Dash dash)
    {

    }

    public virtual void OnDashUpdate(Dash dash)
    {

    }

    public virtual void OnDashEnd(Dash dash)
    {

    }

    public virtual void OnDashOverlap(Dash dash, RaycastHit hit)
    {

    }

    public virtual void OnDashBreak(Dash dash)
    {

    }
}

public enum DashInformationType
{
    Speed,
    Duration
}

[RequireComponent(typeof(NavMeshAgent))]
public class Dash : MonoBehaviour
{
    

    protected List<DashObserver> observer;
    private bool inDash;
    public Vector3 target;
    public DashInformationType moveType;

    [ConditionalField("moveType", false, DashInformationType.Speed)]
    public float speed = 500;
    [ConditionalField("moveType", false, DashInformationType.Duration)]
    public float duration = 1;

    private Vector3 dashOrigin;

    private NavMeshAgent agent;
    private Coroutine dashCorutine;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddDashObserver(DashObserver obs)
    {
        if (!observer.Contains(obs))
        {
            observer.Add(obs);
        }
    }

    public void RemoveObserver(DashObserver obs)
    {
        observer.Remove(obs);
    }

    public void StartDash()
    {
        if (dashCorutine == null)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(target, out hit, 100, agent.areaMask))
            {
                target = hit.position;
            }
            else
            {
                target = transform.position;
            }

            dashOrigin = transform.position;
            speed = (target - dashOrigin).magnitude / duration;


            OnDashStart();
            dashCorutine = StartCoroutine(DashExecution());
        }
    }

    private float GetDeltaSpeed()
    {
        switch(moveType)
        {
            case DashInformationType.Speed: return speed;
            case DashInformationType.Duration: return speed;
        }
        return 0;
    }

    IEnumerator DashExecution()
    {
        inDash = true;
        while(inDash)
        {
            yield return new WaitForFixedUpdate();
            Vector3 start = transform.position;
            agent.Warp(Vector3.MoveTowards(transform.position, target, GetDeltaSpeed()));
            OnDashUpdate();
            Vector3 end = transform.position;

            float rad = agent.radius;
            if(Physics.CapsuleCast(transform.position + new Vector3(0, rad,0) + start,
                transform.position + new Vector3(0, agent.height, 0) + start, rad, end - start, out RaycastHit hit))
            {
                OnDashOverlap(hit);
            }

            if((transform.position - target).magnitude < 0.01f)
            {
                if (inDash)
                {
                    inDash = false;
                    OnDashEnd();
                }
            }
        }
    }

    public void Break()
    {
        inDash = false;
        StopCoroutine(dashCorutine);
        dashCorutine = null;
        OnDashBreak();
        OnDashEnd();
    }

    private void OnDashStart()
    {
        foreach(DashObserver ob in observer)
        {
            ob.OnDashStart(this);
        }
    }

    private void OnDashUpdate()
    {
        foreach (DashObserver ob in observer)
        {
            ob.OnDashUpdate(this);
        }
    }

    private void OnDashEnd()
    {
        foreach (DashObserver ob in observer)
        {
            ob.OnDashEnd(this);
        }
    }

    private void OnDashOverlap(RaycastHit hit)
    {
        foreach (DashObserver ob in observer)
        {
            ob.OnDashOverlap(this, hit);
        }
    }

    private void OnDashBreak()
    {
        foreach (DashObserver ob in observer)
        {
            ob.OnDashBreak(this);
        }
    }
}
