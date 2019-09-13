using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData
{
	public int Hp;
	public int Coins;
}

public struct CardData : IComponentData
{
	
}

public struct PotionData : IComponentData
{
	
}

public struct SwordData : IComponentData
{
	
}

public struct MonsterData : IComponentData
{
	
}

public struct ShieldData : IComponentData
{
	
}

public struct CoinsData : IComponentData
{
	
}


public struct DealCardsFromDeckData : IComponentData
{
	public int Amount;
}

public struct AbilityData : IComponentData
{
	public AbilityType Type;
}


public enum AbilityType
{
	Fortify,
	BloodPact,
	Life,
	Trade,
	Faith
}


public struct StatData : IComponentData
{
	public int Value;
}

[Serializable]
public struct SlotData : IComponentData
{
	public SlotType Type;
	public byte Occupied;
	public Entity Entity;
}


public struct SlotEmptiedData : IComponentData
{
}



public enum SlotType
{
	Player,
	LeftHand,
	RightHand,
	Bag,
	Deck
}

public struct SellCardData : IComponentData
{
	public int Price;
}


public struct KillCardData : IComponentData
{
	
}


public struct ResolveAbilityFaithData : IComponentData
{
	
}