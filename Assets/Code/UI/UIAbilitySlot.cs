using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySlot : MonoBehaviour
{
    public Ability ability;
    public Image sprite;
    public Image backgroundSprite;

    // Start is called before the first frame update
    void Start()
    {
        SetAbility(ability);
    }

    // Update is called once per frame
    void Update()
    {
        if (ability != null)
        {
            sprite.fillAmount = 1.0f - (ability.currentCooldown / ability.GetCooldown());
        }
    }

    public void SetAbility(Ability ability)
    {
        if (ability != null)
        {
            this.ability = ability;
            sprite.sprite = ability.sprite;
            backgroundSprite.sprite = ability.sprite;
        }
        else
        {
            sprite.sprite = null;
        }
    }
}
