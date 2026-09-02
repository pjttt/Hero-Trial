using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActorSo", menuName = "Dialog/NPC")]
public class ActorSO : ScriptableObject
{
    public string actorName;
    public Sprite portrait;//头像
}
