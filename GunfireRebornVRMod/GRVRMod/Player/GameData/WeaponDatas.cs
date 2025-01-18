using HeroMoveState;
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
        public static WeaponData defaultWeaponData = new WeaponData(0, "", new Vector3(0, 0, 0), new Vector3(-0.29f, 0.13f, -0.57f));

        public enum WeaponType
        {
            Default = 0,
            SniperRifle = 1,
            Minigun = 2,
            Bow = 3,
            Gauntlet = 4,
            Melee = 5,
            Talisman = 6,
            Split = 7,
            Helmet = 8
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
            public Vector3 parentOffset;
            public Vector3 modelOffset;
            public HoldingStyle HoldingStyle;
            public WeaponType weaponType;
            public Vector3 parentRotationEuler;
            public Vector3 modelRotationEuler;
            public bool hideMuzzle;
            public bool useParentTrans;

            public WeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset,
                Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default, HoldingStyle HoldingStyle = HoldingStyle.TwoHanded, WeaponType weaponType = WeaponType.Default, bool hideMuzzle = false, bool useParentTrans = false)
            {
                this.id = id;
                this.name = name;
                this.parentOffset = parentOffset;
                this.modelOffset = modelOffset;
                this.parentRotationEuler = parentRotationEuler;
                this.modelRotationEuler = modelRotationEuler;
                this.HoldingStyle = HoldingStyle;
                this.weaponType = weaponType;
                this.hideMuzzle = hideMuzzle;
                this.useParentTrans = useParentTrans;
            }
        }

        public class ParentTransWeaponData : WeaponData
        {
            public ParentTransWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset,
                Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default, HoldingStyle HoldingStyle = HoldingStyle.TwoHanded, WeaponType weaponType = WeaponType.Default, bool hideMuzzle = false)
                : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle, weaponType, hideMuzzle, true)
            {
            }
        }

        public class RifleWeaponData : WeaponData
        {
            public string scopeParent;
            public Vector3 scopePos;
            public Vector3 scopeEuler;
            public Vector3 scopeScale;

            public RifleWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default, string scopeParent = "", Vector3 scopePos = default, Vector3 scopeEuler = default, Vector3 scopeScale = default)
            : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle.TwoHanded, WeaponType.SniperRifle)
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

            public MinigunWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 leftHandOffset, Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default)
            : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle.TwoHanded, WeaponType.Minigun)
            {
                this.leftHandOffset = leftHandOffset;
            }
        }

        public class BowWeaponData : ParentTransWeaponData
        {
            public float leftHandForwardDistance;
            public int zAngle;

            public BowWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, float leftHandForwardDistance, int zAngle)
            : base(id, name, parentOffset, modelOffset, default, default, HoldingStyle.TwoHanded, WeaponType.Bow)
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

            public MeleeWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 attackOffset, Vector3 idleEuler, int idleHash)
            : base(id, name, parentOffset, modelOffset, default, default, HoldingStyle.OneHanded, WeaponType.Melee)
            {
                this.idleEuler = idleEuler;
                this.attackOffset = attackOffset;
                this.idleHash = idleHash;
            }
        }

        public class GauntletWeaponData : ParentTransWeaponData
        {
            public GauntletWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default)
            : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle.OneHanded, WeaponType.Gauntlet)
            {
            }
        }

        public class SplitWeaponData : WeaponData
        {
            public string splitPartName;

            public SplitWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default, string splitPartName = "")
            : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle.OneHanded, WeaponType.Split)
            {
                this.splitPartName = splitPartName;
            }
        }

        public class TalismanWeaponData : ParentTransWeaponData
        {
            public string leftWeaponName;
            public string leftAmuletName;

            public TalismanWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default, string leftWeaponName = null, string leftAmuletName = null)
            : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle.OneHanded, WeaponType.Talisman)
            {
                this.leftWeaponName = leftWeaponName;
                this.leftAmuletName = leftAmuletName;
            }
        }

        public class HelmetWeaponData : WeaponData
        {
            public int aimingHash;

            public HelmetWeaponData(int id, string name, Vector3 parentOffset, Vector3 modelOffset, Vector3 parentRotationEuler = default, Vector3 modelRotationEuler = default, int aimingHash = 0)
            : base(id, name, parentOffset, modelOffset, parentRotationEuler, modelRotationEuler, HoldingStyle.OneHanded, WeaponType.Helmet)
            {
                this.aimingHash = aimingHash;
            }
        }

        //public static Dictionary<int, WeaponData> weaponDatas_old = new Dictionary<int, WeaponData>()
        //{
        //    { 1002, new WeaponData(1002, "铁骑", new Vector3(0, -0.09f, -0.20f))},
        //    { 1003, new WeaponData(1003, "穹虹", new Vector3(0, -0.07f, -0.17f)) },
        //    { 1004, new WeaponData(1004, "赤目火麟", new Vector3(0f, -0.09f, -0.22f)) },
        //    { 1006, new WeaponData(1006, "玉追龙", new Vector3(0f, -0.09f, -0.19f)) },
        //    { 1007, new WeaponData(1007, "六方", new Vector3(-0.06f, -0.11f, -0.32f), HoldingStyle.TwoHanded, WeaponType.Default, new Vector3(0, 3, 0))},
        //    { 1008, new WeaponData(1008, "电鸣丸", new Vector3(0, -0.09f, -0.2f))},
        //    { 1010, new MinigunWeaponData(1010, "大河马", new Vector3(0, -0.08f, -0.20f), new Vector3(0.2f, 0.10f, 0f))},
        //    { 1016, new WeaponData(1016, "示缺", new Vector3(-0.02f, 0.20f, -0.10f)) },
        //    { 1017, new WeaponData(1017, "追猎", new Vector3(0.03f, -0.007f, -0.23f), HoldingStyle.TwoHanded, WeaponType.Default, new Vector3(2.5f, 3, 0)) },
        //    { 1101, new WeaponData(1101, "噬星者", new Vector3(0, -0.08f, -0.2f), HoldingStyle.OneHanded)},
        //    { 1102, new WeaponData(1102, "紫翎之光", new Vector3(-0.03f, 0f, -0.23f)) },
        //    { 1103, new WeaponData(1103, "青晞", new Vector3(0.02f, -0.14f, -0.45f), HoldingStyle.OneHanded, WeaponType.Default, new Vector3(0, 20, 0)) },
        //    { 1104, new WeaponData(1104, "炎魔传说", new Vector3(-0.01f, -0.07f, -0.18f)) },
        //    { 1105, new WeaponData(1105, "手术刀", new Vector3(0f, -0.10f, -0.2f), HoldingStyle.OneHanded) },
        //    { 1107, new WeaponData(1107, "隐弹魔王", new Vector3(0f, -0.12f, -0.17f)) },
        //    { 1108, new WeaponData(1108, "狼顾", new Vector3(0.08f, 0.04f, -0.24f)) },
        //    { 1201, new WeaponData(1201, "双菱裂", new Vector3(0, -0.05f, -0.20f), HoldingStyle.OneHanded)},
        //    { 1202, new WeaponData(1202, "熔炉", new Vector3(0, -0.08f, -0.20f))},
        //    { 1205, new WeaponData(1205, "烈焰弹丸", new Vector3(0, -0.09f, -0.20f), HoldingStyle.OneHanded)},
        //    { 1209, new WeaponData(1209, "极光之蛊", new Vector3(0.02f, -0.12f, -0.26f), HoldingStyle.OneHanded)},
        //    { 1211, new WeaponData(1211, "雷嗔", new Vector3(0, -0.09f, -0.15f), HoldingStyle.OneHanded, WeaponType.Default, Vector3.zero, true)},
        //    { 1212, new TalismanWeaponData(1212, "如律令", new Vector3(0.29f, -0.09f, -0.3f), "Home/weapon_1212_2", "Home/1212_AllRoot/1212_Amulet_01_01")},
        //    { 1213, new WeaponData(1213, "寒霜", new Vector3(-0.19f, -0.13f, -0.53f), HoldingStyle.OneHanded) },
        //    { 1214, new WeaponData(1214, "织云", new Vector3(-0.28f, 0.09f, -0.55f), HoldingStyle.OneHanded) },
        //    { 1215, new WeaponData(1215, "藏拙", new Vector3(-0.36f, 0.13f, -0.76f), HoldingStyle.OneHanded) },
        //    { 1302, new WeaponData(1302, "地狱", new Vector3(0f, -0.03f, -0.09f)) },
        //    { 1303, new WeaponData(1303, "幻道", new Vector3(0f, -0.07f, -0.2f)) },
        //    { 1304, new WeaponData(1304, "青鸾", new Vector3(0f, -0.04f, -0.18f)) },
        //    { 1305, new WeaponData(1305, "瞳", new Vector3(0f, -0.05f, -0.15f)) },
        //    { 1306, new WeaponData(1306, "狂猎", new Vector3(0f, -0.05f, -0.20f)) },
        //    { 1309, new WeaponData(1309, "刺猬", new Vector3(0f, -0.05f, -0.16f)) },
        //    { 1310, new WeaponData(1310, "锯轮", new Vector3(-0.06f, -0.10f, -0.20f), HoldingStyle.OneHanded) },
        //    { 1312, new WeaponData(1312, "望潮", new Vector3(-0.03f, 0.02f, -0.35f), HoldingStyle.OneHanded) },
        //    { 1401, new WeaponData(1401, "青铜虎炮", new Vector3(0f, -0.08f, -0.32f)) },
        //    { 1402, new GauntletWeaponData(1402, "镭射手套", new Vector3(0.1f, -0.1f, -0.4f), "LaserBullet", Vector3.zero, new Vector3(0f, -3f, 0f)) },
        //    { 1404, new WeaponData(1404, "狂鲨", new Vector3(0f, -0.09f, -0.20f)) },
        //    { 1406, new WeaponData(1406, "锐鸣炮", new Vector3(-0.01f, -0.08f, -0.20f)) },
        //    { 1407, new MinigunWeaponData(1407, "狱裂骨龙", new Vector3(-0.03f, -0.14f, -0.11f), new Vector3(0.2f, 0.10f, 0f))},
        //    { 1408, new WeaponData(1408, "蜥燚", new Vector3(0.12f, -0.28f, -0.38f), HoldingStyle.OneHanded, WeaponType.Default, new Vector3(0, 20, 0)) },
        //    { 1409, new WeaponData(1409, "震山镈", new Vector3(-0.26f, 0.11f, -0.46f)) },
        //    { 1410, new GauntletWeaponData(1410, "雷霆手套", new Vector3(0.1f, -0.1f, -0.4f), "LaserBullet", Vector3.zero, new Vector3(0f, -3f, 0f)) },
        //    { 1411, new WeaponData(1411, "彩虹", new Vector3(-0.01f, -0.09f, -0.19f), HoldingStyle.OneHanded) },
        //    { 1412, new WeaponData(1412, "火焰狂龙", new Vector3(0.07f, -0.13f, -0.37f), HoldingStyle.OneHanded, WeaponType.Default, new Vector3(0, 10, 0)) },
        //    { 1414, new GauntletWeaponData(1414, "辐射手套", new Vector3(0.1f, -0.1f, -0.4f), "LaserBullet", Vector3.zero, new Vector3(0f, -3f, 0f)) },
        //    { 1415, new WeaponData(1415, "星环", new Vector3(0f, 0.05f, -0.15f), HoldingStyle.OneHanded) },
        //    { 1416, new WeaponData(1416, "龙息", Vector3.zero, HoldingStyle.OneHanded, WeaponType.Default, new Vector3(5, 9, 0)) }, // Need to update vector
        //    { 1417, new WeaponData(1417, "骤雨", Vector3.zero) }, // Need to update vector
        //    { 1501, new RifleWeaponData(1501, "贯日者", new Vector3(0f, -0.06f, -0.15f), new Vector3(0, 0, 0), "1501_Bone012", new Vector3(-0.1f, 0.18f, 0f), new Vector3(90f, 0f, 0f))},
        //    { 1502, new RifleWeaponData(1502, "爆裂双星", new Vector3(0, -0.08f, -0.15f), new Vector3(0, 1, 0), "1502_bone01", new Vector3(-0.36f, 0.15f, 0f), new Vector3(0f, 270f, 0f))},
        //    { 1503, new RifleWeaponData(1503, "苍鹰", new Vector3(0.02f, -0.05f, -0.32f), new Vector3(0, 0, 0), "1503_Bone001", new Vector3(0.05f, 0.25f, 0f), new Vector3(0f, 270f, 0f))},
        //    { 1504, new RifleWeaponData(1504, "啄木鸟", new Vector3(0f, -0.09f, -0.2f), new Vector3(0, 0, 0), "1504_bone01", new Vector3(0.1655f, -0.14f, 0f), new Vector3(0f, 270f, 0f))},
        //    { 1505, new BowWeaponData(1505, "金陵长弓", new Vector3(-0.09f, 0.07f, -0.45f), 0.16f, 20) },
        //    { 1507, new RifleWeaponData(1507, "棱刺", new Vector3(0, -0.05f, -0.18f), new Vector3(0, 0, 0), "1507_Bone010", new Vector3(-0.15f, 0f, -0.15f))},
        //    { 1508, new WeaponData(1508, "惊蛰", new Vector3(0f, -0.09f, -0.03f)) },
        //    { 1509, new WeaponData(1509, "棱镜", new Vector3(0.16f, -0.03f, -0.36f), HoldingStyle.OneHanded) },
        //    { 1510, new BowWeaponData(1510, "纷飞", new Vector3(-0.09f, 0.07f, -0.45f), 0.16f, 20) },
        //    { 1512, new WeaponData(1512, "萤火", new Vector3(0.16f, -0.03f, -0.36f), HoldingStyle.OneHanded) },
        //    { 1513, new RifleWeaponData(1513, "雷刹", new Vector3(-0.035f, 0.01f, -0.38f), new Vector3(0, 3, 0), "1513_Bone014", new Vector3(0f, 0f, 0f), new Vector3(0f, 270f, 0f))},
        //    { 1514, new WeaponData(1514, "弧光", new Vector3(0.16f, -0.03f, -0.36f), HoldingStyle.OneHanded) },
        //    { 1601, new MeleeWeaponData(1601, "催妖", new Vector3(0.51f, -0.17f, 0.06f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-47.666f, 28.921f, 77.662f), -504015052) },
        //    { 1602, new MeleeWeaponData(1602, "流光", new Vector3(0.51f, -0.17f, 0.06f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-47.666f, 28.921f, 77.662f), -504015052) },
        //    { 1603, new MeleeWeaponData(1603, "鸠鬼", new Vector3(0.15f, -0.24f, -0.11f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-52f, -41.5f, -142.5f), -504015052) },
        //    { 1606, new WeaponData(1606, "逐风", Vector3.zero) }, // Need to update vector
        //    { 1701, new WeaponData(1701, "凤鸣", Vector3.zero) }, // Need to update vector
        //    { 1703, new SplitWeaponData(1703, "璇玑", new Vector3(0.11f, -0.19f, -0.54f)) },
        //};

        public static Dictionary<int, WeaponData> weaponDatas = new Dictionary<int, WeaponData>()
        {
            { 1002, new ParentTransWeaponData(1002, "铁骑", new Vector3(-0.17f, 0.13f, -0.52f), new Vector3(0, -0.09f, -0.20f))}, 
            { 1003, new WeaponData(1003, "穹虹", new Vector3(-0.19f, 0.17f, -0.32f), new Vector3(0, -0.07f, -0.17f)) },
            { 1004, new ParentTransWeaponData(1004, "赤目火麟", new Vector3(-0.17f, 0.16f, -0.39f), new Vector3(0f, -0.09f, -0.22f)) }, 
            { 1006, new ParentTransWeaponData(1006, "玉追龙", new Vector3(-0.20f, 0.13f, -0.40f), new Vector3(0f, -0.09f, -0.19f)) }, 
            { 1007, new WeaponData(1007, "六方", new Vector3(-0.26f, 0.14f, -0.53f), new Vector3(-0.06f, -0.11f, -0.32f), new Vector3(0.00f, 3.00f, 0.00f), new Vector3(0.00f, 3.00f, 0.00f))}, 
            { 1008, new ParentTransWeaponData(1008, "电鸣丸", new Vector3(-0.17f, 0.16f, -0.31f), new Vector3(0, -0.09f, -0.2f))}, 
            { 1010, new MinigunWeaponData(1010, "大河马", new Vector3(-0.28f, 0.05f, -0.30f), new Vector3(0.00f, -0.08f, -0.20f), new Vector3(0.2f, 0.10f, 0f), new Vector3(0.00f, 4.00f, 0.00f))}, 
            { 1016, new WeaponData(1016, "示缺", new Vector3(-0.13f, 0.16f, -0.44f), new Vector3(-0.04f, -0.02f, -0.16f), default, new Vector3(-1.50f, -2.50f, -5.50f)) }, 
            { 1017, new ParentTransWeaponData(1017, "追猎", new Vector3(-0.16f, 0.19f, -0.39f), new Vector3(0.03f, -0.06f, -0.23f), new Vector3(3.00f, 2.50f, 0.00f), new Vector3(3.50f, 2.50f, 0.00f)) },  
            { 1101, new ParentTransWeaponData(1101, "噬星者", new Vector3(-0.35f, 0.17f, -0.76f), new Vector3(0.00f, -0.08f, -0.20f), default, default, HoldingStyle.OneHanded)}, 
            { 1102, new ParentTransWeaponData(1102, "紫翎之光", new Vector3(-0.22f, 0.14f, -0.62f), new Vector3(-0.03f, 0f, -0.23f), new Vector3(0, 4, 0), default) },  
            { 1103, new WeaponData(1103, "青晞", new Vector3(-0.23f, 0.07f, -0.66f), new Vector3(0.02f, -0.14f, -0.45f), default, default, HoldingStyle.OneHanded) },  
            { 1104, new WeaponData(1104, "炎魔传说", new Vector3(-0.26f, 0.14f, -0.39f), new Vector3(-0.01f, -0.07f, -0.18f), new Vector3(0.00f, 4.00f, 0.00f), new Vector3(0.00f, 4.00f, 0.00f)) }, 
            { 1105, new ParentTransWeaponData(1105, "手术刀", new Vector3(-0.34f, 0.14f, -0.77f), new Vector3(0.00f, -0.14f, -0.22f), default, default, HoldingStyle.OneHanded) }, 
            { 1107, new ParentTransWeaponData(1107, "隐弹魔王", new Vector3(-0.23f, 0.13f, -0.51f), new Vector3(0f, -0.12f, -0.17f), new Vector3(0.00f, 4.00f, 0.00f), default) }, 
            { 1108, new ParentTransWeaponData(1108, "狼顾", new Vector3(-0.30f, 0.13f, -0.60f), new Vector3(-0.05f, -0.08f, -0.39f), new Vector3(0.00f, 8.50f, 0.00f), new Vector3(0.00f, 8.00f, 0.00f)) }, 
            { 1201, new WeaponData(1201, "双菱裂", new Vector3(-0.21f, 0.16f, -0.61f), new Vector3(0.00f, -0.07f, -0.20f), default, default, HoldingStyle.OneHanded)}, 
            { 1202, new WeaponData(1202, "熔炉", new Vector3(-0.29f, 0.12f, -0.59f), new Vector3(0.00f, -0.08f, -0.20f))}, 
            { 1205, new WeaponData(1205, "烈焰弹丸", new Vector3(-0.24f, 0.19f, -0.79f), new Vector3(0, -0.09f, -0.20f), default, default, HoldingStyle.OneHanded)}, 
            { 1209, new WeaponData(1209, "极光之蛊", new Vector3(-0.28f, 0.16f, -0.81f), new Vector3(0.02f, -0.12f, -0.26f), default, default, HoldingStyle.OneHanded)}, 
            { 1211, new WeaponData(1211, "雷嗔", new Vector3(-0.35f, 0.13f, -0.81f), new Vector3(0.02f, -0.11f, -0.15f), default, default, HoldingStyle.OneHanded)},  
            { 1212, new TalismanWeaponData(1212, "如律令", new Vector3(-0.04f, 0.11f, -0.65f), new Vector3(0.29f, -0.09f, -0.30f), default, default, "Home/weapon_1212_2", "Home/1212_AllRoot/1212_Amulet_01_01")},  
            { 1213, new WeaponData(1213, "寒霜", new Vector3(-0.19f, -0.13f, -0.53f), new Vector3(-0.19f, -0.13f, -0.53f), default, default, HoldingStyle.OneHanded) },
            { 1214, new WeaponData(1214, "织云", new Vector3(-0.28f, 0.09f, -0.55f), new Vector3(-0.28f, 0.09f, -0.55f), default, default, HoldingStyle.OneHanded) }, 
            { 1215, new WeaponData(1215, "藏拙", new Vector3(-0.36f, 0.14f, -0.85f), new Vector3(-0.36f, 0.14f, -0.85f), default, default, HoldingStyle.OneHanded) }, 
            { 1302, new WeaponData(1302, "地狱", new Vector3(-0.17f, 0.08f, -0.30f), new Vector3(0.00f, -0.03f, -0.09f)) }, 
            { 1303, new ParentTransWeaponData(1303, "幻道", new Vector3(-0.19f, 0.13f, -0.41f), new Vector3(0.00f, -0.07f, -0.20f)) }, 
            { 1304, new WeaponData(1304, "青鸾", new Vector3(-0.21f, 0.16f, -0.36f), new Vector3(0.00f, -0.04f, -0.18f)) }, 
            { 1305, new WeaponData(1305, "瞳", new Vector3(-0.23f, 0.12f, -0.30f), new Vector3(0f, -0.05f, -0.15f)) }, 
            { 1306, new WeaponData(1306, "狂猎", new Vector3(-0.19f, 0.13f, -0.38f), new Vector3(0.00f, -0.05f, -0.18f)) }, 
            { 1309, new WeaponData(1309, "刺猬", new Vector3(-0.22f, 0.18f, -0.43f), new Vector3(0f, -0.05f, -0.16f)) }, 
            { 1310, new WeaponData(1310, "锯轮", new Vector3(-0.32f, 0.17f, -0.78f), new Vector3(-0.08f, -0.10f, -0.20f), default, default, HoldingStyle.OneHanded) }, 
            { 1312, new WeaponData(1312, "望潮", new Vector3(-0.28f, 0.16f, -0.69f), new Vector3(-0.03f, 0.02f, -0.35f), default, default, HoldingStyle.OneHanded) }, 
            { 1401, new WeaponData(1401, "青铜虎炮", new Vector3(-0.35f, 0.23f, -0.51f), new Vector3(0.00f, -0.08f, -0.32f)) }, 
            { 1402, new GauntletWeaponData(1402, "镭射手套", new Vector3(-0.22f, 0.11f, -0.77f), new Vector3(0.1f, -0.1f, -0.4f)) }, 
            { 1404, new WeaponData(1404, "狂鲨", new Vector3(-0.32f, 0.18f, -0.73f), new Vector3(0f, -0.09f, -0.20f)) }, 
            { 1406, new WeaponData(1406, "锐鸣炮", new Vector3(-0.26f, 0.19f, -0.36f), new Vector3(-0.01f, -0.08f, -0.20f)) }, 
            { 1407, new MinigunWeaponData(1407, "狱裂骨龙", new Vector3(-0.41f, 0.10f, -0.22f), new Vector3(-0.07f, -0.14f, -0.11f), new Vector3(0.2f, 0.10f, 0f), new Vector3(0.00f, 6.50f, 0.00f), new Vector3(0.00f, 6.50f, 0.00f))},  
            { 1408, new WeaponData(1408, "蜥燚", new Vector3(-0.23f, 0.06f, -0.62f), new Vector3(0.12f, -0.28f, -0.38f), default, default, HoldingStyle.OneHanded) }, 
            { 1409, new WeaponData(1409, "震山镈", new Vector3(-0.26f, 0.11f, -0.46f), new Vector3(-0.26f, 0.11f, -0.46f)) }, 
            { 1410, new GauntletWeaponData(1410, "雷霆手套", new Vector3(-0.22f, 0.11f, -0.77f), new Vector3(0.1f, -0.1f, -0.4f)) }, 
            { 1411, new WeaponData(1411, "彩虹", new Vector3(-0.25f, 0.07f, -0.54f), new Vector3(-0.01f, -0.09f, -0.19f), default, default, HoldingStyle.OneHanded) },  
            { 1412, new WeaponData(1412, "火焰狂龙", new Vector3(-0.29f, 0.14f, -0.84f), new Vector3(0.09f, -0.14f, -0.39f), new Vector3(0.00f, 2.50f, 0.00f), new Vector3(-0.50f, 2.50f, 0.00f), HoldingStyle.OneHanded) }, 
            { 1414, new GauntletWeaponData(1414, "辐射手套", new Vector3(-0.22f, 0.11f, -0.77f), new Vector3(0.1f, -0.1f, -0.4f)) }, 
            { 1415, new WeaponData(1415, "星环", new Vector3(-0.24f, 0.09f, -0.81f), new Vector3(0.00f, -0.05f, -0.47f), default, default, HoldingStyle.OneHanded) },  
            { 1416, new WeaponData(1416, "龙息", new Vector3(-0.35f, 0.17f, -0.54f), new Vector3(-0.20f, -0.01f, -0.42f), new Vector3(0.00f, 8.50f, 0.00f), new Vector3(0.00f, 8.50f, 0.00f), HoldingStyle.OneHanded) },  
            { 1417, new HelmetWeaponData(1417, "骤雨", new Vector3(-0.22f, 0.14f, -0.79f), new Vector3(-0.22f, 0.14f, -0.79f), default, default, -1879668672) },
            { 1418, new WeaponData(1418, "素湍", new Vector3(-0.26f, 0.15f, -0.85f), new Vector3(0.09f, -0.19f, -0.61f), new Vector3(0.00f, 11.50f, 0.00f), new Vector3(0.00f, 11.50f, 0.00f), HoldingStyle.OneHanded)},
            { 1501, new RifleWeaponData(1501, "贯日者", new Vector3(-0.17f, 0.14f, -0.29f), new Vector3(-0.01f, -0.06f, -0.17f), new Vector3(0.00f, 1.50f, 0.00f), new Vector3(0.00f, 1.50f, 0.00f), "1501_Bone012", new Vector3(-0.1f, 0.18f, 0f), new Vector3(90f, 0f, 0f))}, 
            { 1502, new RifleWeaponData(1502, "爆裂双星", new Vector3(-0.17f, 0.09f, -0.29f), new Vector3(0, -0.08f, -0.15f), new Vector3(0, 1, 0), new Vector3(0, 1, 0), "1502_bone01", new Vector3(-0.36f, 0.15f, 0f), new Vector3(0f, 270f, 0f))}, 
            { 1503, new RifleWeaponData(1503, "苍鹰", new Vector3(-0.22f, 0.17f, -0.48f), new Vector3(0.01f, -0.05f, -0.32f), default, default, "1503_Bone001", new Vector3(0.05f, 0.25f, 0f), new Vector3(0f, 270f, 0f))}, 
            { 1504, new RifleWeaponData(1504, "啄木鸟", new Vector3(0f, -0.09f, -0.2f), new Vector3(0f, -0.09f, -0.2f), new Vector3(0, 0, 0), default, "1504_bone01", new Vector3(0.1655f, -0.14f, 0f), new Vector3(0f, 270f, 0f))}, 
            { 1505, new BowWeaponData(1505, "金陵长弓", new Vector3(-0.17f, 0.08f, -0.62f), new Vector3(-0.09f, 0.07f, -0.45f), 0.16f, 20) }, 
            { 1507, new RifleWeaponData(1507, "棱刺", new Vector3(-0.16f, 0.12f, -0.29f), new Vector3(-0.01f, -0.06f, -0.17f), new Vector3(0.00f, 1.50f, 0.00f), new Vector3(0.00f, 1.50f, 0.00f), "1507_Bone010", new Vector3(-0.15f, 0f, -0.15f))}, 
            { 1508, new WeaponData(1508, "惊蛰", new Vector3(-0.19f, 0.14f, -0.36f), new Vector3(0f, -0.09f, -0.03f)) },  
            { 1509, new WeaponData(1509, "棱镜", new Vector3(-0.17f, 0.15f, -0.71f), new Vector3(0.16f, -0.03f, -0.36f), default, default, HoldingStyle.OneHanded) },  
            { 1510, new BowWeaponData(1510, "纷飞", new Vector3(-0.17f, 0.08f, -0.62f), new Vector3(-0.09f, 0.07f, -0.45f), 0.16f, 20) }, 
            { 1512, new WeaponData(1512, "萤火", new Vector3(-0.18f, 0.15f, -0.71f), new Vector3(0.16f, -0.03f, -0.36f), default, default, HoldingStyle.OneHanded) }, 
            { 1513, new RifleWeaponData(1513, "雷刹", new Vector3(-0.20f, 0.16f, -0.53f), new Vector3(-0.04f, -0.01f, -0.38f), new Vector3(0, 3, 0), new Vector3(0, 3, 0), "1513_Bone014", new Vector3(0f, 0f, 0f), new Vector3(0f, 270f, 0f))}, 
            { 1514, new WeaponData(1514, "弧光", new Vector3(-0.18f, 0.15f, -0.70f), new Vector3(0.16f, -0.03f, -0.36f), default, default, HoldingStyle.OneHanded) }, 
            { 1601, new MeleeWeaponData(1601, "催妖", new Vector3(0.12f, 0.12f, -0.40f), new Vector3(0.52f, -0.14f, 0.05f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-20f, 0f, 65f), -504015052) }, 
            { 1602, new MeleeWeaponData(1602, "流光", new Vector3(0.14f, 0.12f, -0.39f), new Vector3(0.51f, -0.17f, 0.06f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-20f, 0f, 65f), -504015052) }, 
            { 1603, new MeleeWeaponData(1603, "鸠鬼", new Vector3(-0.22f, 0.05f, -0.56f), new Vector3(0.15f, -0.24f, -0.11f), new Vector3(0.05f, -0.22f, 0.05f), new Vector3(-52f, -41.5f, -142.5f), -504015052) },
            { 1606, new WeaponData(1606, "逐风", new Vector3(-0.11f, 0.25f, -0.34f), new Vector3(0.26f, 0.11f, -0.14f)) }, 
            { 1701, new WeaponData(1701, "凤鸣", new Vector3(-0.42f, 0.26f, -0.85f), new Vector3(-0.61f, 0.16f, -0.84f)) },
            { 1703, new SplitWeaponData(1703, "璇玑", new Vector3(-0.27f, 0.15f, -0.77f), new Vector3(0.11f, -0.19f, -0.54f), default, default, "weapon_1703_") },
            { 1704, new WeaponData(1704, "鹤吟", new Vector3(-0.29f, 0.12f, -0.59f), new Vector3(0.00f, -0.08f, -0.20f))}
        };

        public static WeaponData GetWeaponData(int weaponID)
        {
            return weaponDatas.GetValueOrDefault(weaponID, defaultWeaponData);
        }

        public static void SetWeaponData(int weaponID, WeaponData data)
        {
            weaponDatas[weaponID] = data;
        }
    }
}
