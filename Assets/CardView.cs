using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

public class CardView : MonoBehaviour
{

    public TextMeshPro StatText;
    public TextMeshPro NameText;

    [HideInInspector] public GameObject slot;

    [HideInInspector] public bool isDisabled;

    private void Awake()
    {
        Assert.IsNotNull(StatText);
        Assert.IsNotNull(NameText);
    }


    public void SetStat(int stat)
    {
        StatText.SetText(stat.ToString());
        
    }


    public void SetName(String name)
    {
        NameText.SetText(name);
    }



    public void Kill()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(this.GetComponent<SpriteRenderer>().DOFade(0, GameplayConstants.KilledCardFadeOutDuration));
        seq.AppendCallback(() => GameObject.Destroy(this.gameObject));
        foreach (var VARIABLE in this.GetComponentsInChildren<SpriteRenderer>())
        {
            VARIABLE.DOFade(0, GameplayConstants.KilledCardFadeOutDuration);
        }

        foreach (TextMeshPro textMeshPro in this.GetComponentsInChildren<TextMeshPro>())
        {
            textMeshPro.DOFade(0, GameplayConstants.KilledCardFadeOutDuration);

        }

        ExitSlot();

    }

    private void ExitSlot()
    {
        var entity = this.GetComponent<GameObjectEntity>().Entity;
        var entityManager = this.GetComponent<GameObjectEntity>().EntityManager;
        var slotData = entityManager.GetComponentData<SlotData>(entity);
        
        entityManager.SetComponentData(entity,new SlotData
        {
            Type = slotData.Type,
            Entity = entity,
            Occupied = 0
        });


        if (slotData.Type == SlotType.Deck)
        {
            var evt = entityManager.CreateEntity();
            entityManager.AddComponentData(evt, new SlotEmptiedData());
        }
    }


    public void Disable()
    {
        isDisabled = true;
        foreach (var VARIABLE in this.GetComponentsInChildren<SpriteRenderer>())
        {
            VARIABLE.DOFade(0, GameplayConstants.KilledCardFadeOutDuration);
        }

        foreach (TextMeshPro textMeshPro in this.GetComponentsInChildren<TextMeshPro>())
        {
            textMeshPro.DOFade(0, GameplayConstants.KilledCardFadeOutDuration);

        }
        
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
