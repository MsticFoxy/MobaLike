using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarManager : MonoBehaviour
{
    public StatBlock statBlock;
    private StatValue<PoolValueFloat> health;
    public Image healthBar;

    // Start is called before the first frame update
    void Start()
    {
        health = statBlock.GetStat<StatValue<PoolValueFloat>>("health");
        health.OnStatChanged += () =>
        {
            healthBar.fillAmount = health.value.current / health.value.max;
            if(health.value.current <= 0)
            {
                gameObject.SetActive(false);
            }
        };
        healthBar.fillAmount = health.value.current / health.value.max;
        transform.SetParent(null);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        transform.position = statBlock.transform.position + Vector3.up * 2.25f;
    }
}
