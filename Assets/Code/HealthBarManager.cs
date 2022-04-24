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
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
