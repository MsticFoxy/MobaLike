using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySlot : MonoBehaviour
{
    public Ability ability;
    public Image sprite;
    public Image backgroundSprite;
    public TextMeshProUGUI cooldownText;

    private bool prevInCast = false;

    // Start is called before the first frame update
    void Start()
    {
        sprite.material = Instantiate(sprite.material);
        SetAbility(ability);
    }

    protected bool ShowCooldownAsMinutes()
    {
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        if (ability != null)
        {
            sprite.fillAmount = 1.0f - (ability.currentCooldown / ability.GetCooldown());

            if(ability.currentCooldown > 0)
            {
                string time = "" +Mathf.CeilToInt(ability.currentCooldown);

                if(ShowCooldownAsMinutes())
                {
                    if(Mathf.Ceil(ability.currentCooldown) >= 60)
                    {
                        int mins = Mathf.FloorToInt(ability.currentCooldown / 60);
                        int secs = Mathf.CeilToInt(ability.currentCooldown - mins * 60);
                        if(secs == 60)
                        {
                            secs = 0;
                            mins++;
                        }
                        if(secs < 10)
                        {
                            time = "" + mins + ":0" + secs;
                        }
                        else
                        {
                            time = "" + mins + ":" + secs;
                        }
                        
                    }
                }

                if(ability.currentCooldown < 1)
                {
                    time = "0." + Mathf.RoundToInt(ability.currentCooldown * 100);
                }

                cooldownText.text = time;
            }
            else
            {
                cooldownText.text = "";
            }

            if (prevInCast != ability.IsCastable())
            {
                if (!ability.IsCastable())
                {
                    sprite.material.SetFloat("_Desaturate", 1);
                }
                else
                {
                    sprite.material.SetFloat("_Desaturate", 0);
                }
                prevInCast = ability.IsCastable();
            }
        }
        else
        {
            cooldownText.text = "";
        }
    }

    public void SetAbility(Ability ability)
    {
        if (ability != null)
        {
            this.ability = ability;
            sprite.sprite = ability.sprite;
            backgroundSprite.sprite = ability.sprite;
            sprite.material.SetFloat("_BlackOut", 0);
            sprite.material.SetFloat("_Desaturate", 0);
            prevInCast = true;
        }
        else
        {
            sprite.sprite = null;
            sprite.material.SetFloat("_Desaturate", 1);
            sprite.material.SetFloat("_BlackOut", 1);
        }
    }


    private void OnMouseEnter()
    {
        ability.ShowUI();
    }

    private void OnMouseExit()
    {
        ability.HideUI();
    }
}
