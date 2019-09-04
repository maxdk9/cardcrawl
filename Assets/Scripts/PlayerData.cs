using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PlayerData : IComponentData
{
	public int Hp;
	public int Coins;
}
