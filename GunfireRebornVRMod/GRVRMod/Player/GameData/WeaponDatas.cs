using InControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static VRMod.Player.MotionControlls.HandController;

namespace VRMod.Player.GameData
{
    /// <summary>
    /// 这游戏武器众多，每一把的握持动画都不一样，需要单独记录每一把所需的Offset来对齐手柄
    /// </summary>
    public class WeaponDatas
    {
        
        public static WeaponData defaultWeaponData = new WeaponData(0, Vector3.zero, true);
        public struct WeaponData
        {
            public int id;
            public Vector3 offset;
            public bool isTwoHanded;
            public HandType mainHand;

            public WeaponData(int id, Vector3 offset, bool isTwoHanded, HandType mainHand = HandType.Right)
            {
                this.id = id;
                this.offset = offset;
                this.isTwoHanded = isTwoHanded;
                this.mainHand = mainHand;
            }
        }

        public static Dictionary<int, WeaponData> WeaponOffsets = new Dictionary<int, WeaponData>()
        {
            { 1202, new WeaponData(1202, new Vector3(0, -0.08f, -0.15f), true)},
            { 1402, new WeaponData(1402, new Vector3(0.1f, -0.1f, -0.4f), true) },
            { 1404, new WeaponData(1404, new Vector3(0f, -0.08f, -0.15f), true) },
            { 1410, new WeaponData(1410, new Vector3(0.1f, -0.04f, -0.4f), false) },
            { 90, new WeaponData(90, new Vector3(-0.28f, 0.09f, -0.55f),false) },
            { 1505, new WeaponData(1505, new Vector3(-0.09f, 0.07f, -0.42f), true, HandType.Left) },
            { 1508, new WeaponData(1508, new Vector3(-0.02f, -0.1f, -0.03f), true) },
            { 1510, new WeaponData(1510, new Vector3(-0.09f, 0.07f, -0.42f), true, HandType.Left) }
        };

        public static WeaponData GetWeaponData(int weaponID)
        {
            return WeaponOffsets.GetValueOrDefault(weaponID, defaultWeaponData);
        }

    }
}
