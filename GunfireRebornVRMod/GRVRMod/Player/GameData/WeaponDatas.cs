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
        public static WeaponData defaultWeaponData = new WeaponData(0, "", Vector3.zero);

        public enum WeaponType
        {
            Default = 0,
            SniperRifle = 1,
            Minigun = 2,
            Bow = 3,
            Gauntlet = 4,
            Melee = 5,
            Talisman = 6
        }

        public enum HoldingStyle
        {
            OneHanded = 0,
            TwoHanded = 1
        }


        public class WeaponData
        {
            public int id;
            public string name;
            public Vector3 offset;
            public HoldingStyle HoldingStyle;
            public WeaponType weaponType;
            public Vector3 rotationEuler;
            public bool hideMuzzle;

            public WeaponData(int id, string name, Vector3 offset, HoldingStyle HoldingStyle = HoldingStyle.TwoHanded, WeaponType weaponType = WeaponType.Default, 
                Vector3 rotationEuler = default, bool hideMuzzle = false)
            {
                this.id = id;
                this.name = name;
                this.offset = offset;
                this.HoldingStyle = HoldingStyle;
                this.weaponType = weaponType;
                this.rotationEuler = rotationEuler;
                this.hideMuzzle = hideMuzzle;
            }
        }

        public class RifleWeaponData : WeaponData
        {
            public string scopeParent;
            public Vector3 scopePos;
            public Vector3 scopeEuler;
            public Vector3 scopeScale;

            public RifleWeaponData(int id, string name, Vector3 offset, Vector3 rotationEuler, string scopeParent = "", Vector3 scopePos = default, Vector3 scopeEuler = default, Vector3 scopeScale = default)
            : base(id, name, offset, HoldingStyle.TwoHanded, WeaponType.SniperRifle, rotationEuler)
            {
                this.scopeParent = scopeParent;
                this.scopePos = scopePos;
                this.scopeEuler = scopeEuler;
                this.scopeScale = (scopeScale == Vector3.zero) ? new Vector3(3f, 3f, 3f) : scopeScale;
            }
        }

        public class MinigunWeaponData : WeaponData
        {
            public Vector3 leftHandOffset;

            public MinigunWeaponData(int id, string name, Vector3 offset, Vector3 leftHandOffset)
            : base(id, name, offset, HoldingStyle.TwoHanded, WeaponType.Minigun)
            {
                this.leftHandOffset = leftHandOffset;
            }
        }

        public class BowWeaponData : WeaponData
        {
            public float leftHandForwardDistance;
            public int zAngle;

            public BowWeaponData(int id, string name, Vector3 offset, float leftHandForwardDistance, int zAngle)
            : base(id, name, offset, HoldingStyle.TwoHanded, WeaponType.Bow)
            {
                this.leftHandForwardDistance = leftHandForwardDistance;
                this.zAngle = zAngle;
            }
        }

        public class MeleeWeaponData : WeaponData
        {
            public Vector3 attackOffset;
            public Vector3 idleEuler;
            public int idleHash;

            public MeleeWeaponData(int id, string name, Vector3 offset, Vector3 attackOffset, Vector3 idleEuler, int idleHash)
            : base(id, name, offset, HoldingStyle.OneHanded, WeaponType.Melee)
            {
                this.idleEuler = idleEuler;
                this.attackOffset = attackOffset;
                this.idleHash = idleHash;
            }
        }

        //public class GauntletWeaponData : WeaponData
        //{
        //    public string muzzle;
        //    public Vector3 scopePos;
        //    public Vector3 scopeEuler;
        //    public Vector3 scopeScale;

        //    public GauntletWeaponData(int id, Vector3 offset, string scopeParent = "", Vector3 scopePos = default, Vector3 scopeEuler = default, Vector3 scopeScale = default)
        //    : base(id, offset, WeaponType.Gauntlet)
        //    {
        //        this.id = id;
        //        this.offset = offset;
        //        this.scopeParent = scopeParent;
        //        this.scopePos = scopePos;
        //        this.scopeEuler = scopeEuler;
        //        this.scopeScale = (scopeScale == Vector3.zero) ? new Vector3(3f, 3f, 3f) : scopeScale;
        //    }
        //}

        public static Dictionary<int, WeaponData> weaponDatas = new Dictionary<int, WeaponData>()
        {
            { 1002, new WeaponData(1002, "铁骑", new Vector3(0, -0.09f, -0.20f))},
            { 1003, new WeaponData(1003, "穹虹", new Vector3(0, -0.07f, -0.17f)) },
            { 1004, new WeaponData(1004, "赤目火麟", new Vector3(0f, -0.09f, -0.22f)) },
            { 1006, new WeaponData(1006, "玉追龙", new Vector3(0f, -0.09f, -0.19f)) },
            { 1007, new WeaponData(1007, "六方", new Vector3(-0.06f, -0.11f, -0.32f))},
            { 1008, new WeaponData(1008, "电鸣丸", new Vector3(0, -0.09f, -0.2f))},
            { 1010, new MinigunWeaponData(1010, "大河马", new Vector3(0, -0.08f, -0.20f), new Vector3(0.2f, 0.15f, 0f))},
            { 1101, new WeaponData(1101, "噬星者", new Vector3(0, -0.08f, -0.2f), HoldingStyle.OneHanded)},
            { 1102, new WeaponData(1102, "紫翎之光", new Vector3(-0.03f, 0f, -0.23f)) },
            { 1103, new WeaponData(1103, "青晞", new Vector3(0.02f, -0.14f, -0.45f), HoldingStyle.OneHanded, WeaponType.Default, new Vector3(0, 20, 0)) },
            { 1104, new WeaponData(1104, "炎魔传说", new Vector3(-0.01f, -0.07f, -0.18f)) },
            { 1105, new WeaponData(1105, "手术刀", new Vector3(0f, -0.10f, -0.2f), HoldingStyle.OneHanded) },
            { 1107, new WeaponData(1107, "隐弹魔王", new Vector3(0f, -0.12f, -0.17f)) },
            { 1201, new WeaponData(1201, "双菱裂", new Vector3(0, -0.05f, -0.20f), HoldingStyle.OneHanded)},
            { 1202, new WeaponData(1202, "熔炉", new Vector3(0, -0.08f, -0.20f))},
            { 1205, new WeaponData(1205, "烈焰弹丸", new Vector3(0, -0.09f, -0.20f), HoldingStyle.OneHanded)},
            { 1209, new WeaponData(1209, "极光之蛊", new Vector3(0.02f, -0.12f, -0.26f), HoldingStyle.OneHanded)},
            { 1211, new WeaponData(1211, "雷嗔", new Vector3(0, -0.09f, -0.15f), HoldingStyle.OneHanded, WeaponType.Default, Vector3.zero, true)},
            { 1212, new WeaponData(1212, "如律令", new Vector3(0.29f, -0.09f, -0.3f), HoldingStyle.OneHanded, WeaponType.Talisman)},
            { 1213, new WeaponData(1213, "寒霜", new Vector3(-0.19f, -0.13f, -0.53f), HoldingStyle.OneHanded) },
            { 1214, new WeaponData(1214, "织云", new Vector3(-0.28f, 0.09f, -0.55f), HoldingStyle.OneHanded) },
            { 1302, new WeaponData(1302, "地狱", new Vector3(0f, -0.03f, -0.09f)) },
            { 1303, new WeaponData(1303, "幻道", new Vector3(0f, -0.07f, -0.2f)) },
            { 1304, new WeaponData(1304, "青鸾", new Vector3(0f, -0.04f, -0.18f)) },
            { 1305, new WeaponData(1305, "瞳", new Vector3(0f, -0.05f, -0.15f)) },
            { 1306, new WeaponData(1306, "狂猎", new Vector3(0f, -0.05f, -0.20f)) },
            { 1309, new WeaponData(1309, "刺猬", new Vector3(0f, -0.05f, -0.16f)) },
            { 1401, new WeaponData(1401, "青铜虎炮", new Vector3(0f, -0.08f, -0.32f)) },
            { 1402, new WeaponData(1402, "镭射手套", new Vector3(0.1f, -0.1f, -0.4f), HoldingStyle.OneHanded) },
            { 1404, new WeaponData(1404, "狂鲨", new Vector3(0f, -0.09f, -0.20f)) },
            { 1406, new WeaponData(1406, "锐鸣炮", new Vector3(-0.01f, -0.08f, -0.20f)) },
            { 1407, new MinigunWeaponData(1407, "狱裂骨龙", new Vector3(-0.03f, -0.14f, -0.11f), new Vector3(0.2f, 0.15f, 0f))},
            { 1408, new WeaponData(1408, "蜥燚", new Vector3(0.12f, -0.28f, -0.38f), HoldingStyle.OneHanded, WeaponType.Default, new Vector3(0, 20, 0)) },
            { 1409, new WeaponData(1409, "镇山镈", new Vector3(-0.26f, 0.11f, -0.46f)) },
            { 1410, new WeaponData(1410, "雷霆手套", new Vector3(0.1f, -0.1f, -0.4f), HoldingStyle.OneHanded) },
            { 1411, new WeaponData(1411, "彩虹", new Vector3(-0.01f, -0.09f, -0.19f), HoldingStyle.OneHanded) },
            { 1412, new WeaponData(1412, "火焰狂龙", new Vector3(0.07f, -0.13f, -0.37f), HoldingStyle.OneHanded, WeaponType.Default, new Vector3(0, 20, 0)) },
            { 1501, new RifleWeaponData(1501, "贯日者", new Vector3(0f, -0.06f, -0.15f), new Vector3(0, 0, 0), "1501_Bone012", new Vector3(-0.1f, 0.18f, 0f), new Vector3(90f, 0f, 0f))},
            { 1502, new RifleWeaponData(1502, "爆裂双星", new Vector3(0, -0.08f, -0.15f), new Vector3(0, 1, 0), "1502_bone01", new Vector3(-0.36f, 0.15f, 0f), new Vector3(0f, 270f, 0f))},
            { 1503, new RifleWeaponData(1503, "苍鹰", new Vector3(0.02f, -0.05f, -0.32f), new Vector3(0, 0, 0), "1503_Bone001", new Vector3(0.05f, 0.25f, 0f), new Vector3(0f, 270f, 0f))},
            { 1504, new RifleWeaponData(1504, "啄木鸟", new Vector3(0f, -0.09f, -0.2f), new Vector3(0, 0, 0), "1504_bone01", new Vector3(0.1655f, -0.14f, 0f), new Vector3(0f, 270f, 0f))},
            { 1505, new BowWeaponData(1505, "金陵长弓", new Vector3(-0.09f, 0.07f, -0.45f), 0.16f, 30) },
            { 1507, new RifleWeaponData(1507, "棱刺", new Vector3(0, -0.05f, -0.18f), new Vector3(0, 0, 0), "1507_Bone010", new Vector3(-0.15f, 0f, -0.15f))},
            { 1508, new WeaponData(1508, "惊蛰", new Vector3(0f, -0.9f, -0.03f)) },
            { 1509, new WeaponData(1509, "棱镜", new Vector3(0.16f, -0.03f, -0.36f), HoldingStyle.OneHanded) },
            { 1510, new BowWeaponData(1510, "纷飞", new Vector3(-0.09f, 0.07f, -0.45f), 0.16f, 30) },
            { 1512, new WeaponData(1512, "萤火", new Vector3(0.16f, -0.03f, -0.36f), HoldingStyle.OneHanded) },
            { 1513, new RifleWeaponData(1513, "雷刹", new Vector3(-0.035f, 0.01f, -0.38f), new Vector3(0, 3, 0), "1513_Bone014", new Vector3(0f, 0f, 0f), new Vector3(0f, 270f, 0f))},
            { 1514, new WeaponData(1514, "弧光", new Vector3(0.16f, -0.03f, -0.36f), HoldingStyle.OneHanded) },
            //1601 催妖
            { 1601, new MeleeWeaponData(1601, "催妖", Vector3.zero, Vector3.zero, Vector3.zero, -504015052) },
            { 1602, new MeleeWeaponData(1602, "流光", new Vector3(0.51f, -0.17f, 0.06f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-47.666f, 28.921f, 77.662f), -504015052) },
            { 1603, new MeleeWeaponData(1603, "鸠鬼", new Vector3(0.15f, -0.24f, -0.11f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-52f, -41.5f, -142.5f), -504015052) },
        };

        public static WeaponData GetWeaponData(int weaponID)
        {
            return weaponDatas.GetValueOrDefault(weaponID, defaultWeaponData);
        }

    }
}
