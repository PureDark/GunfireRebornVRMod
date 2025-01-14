using System.Collections.Generic;

namespace VRMod.Player.GameData
{
    public class HeroDatas
    {
        public static HeroData defaultHeroData = new HeroData();

        public class HeroData
        {
            public int sid;
            public int idleHash;
            public int secondarySkillHash;
            public int primarySkillEnterHash;
            public int primarySkillHash;
            public int primarySkillExitHash;

            public HeroData(int sid = 0, int idleHash = 0, int secondarySkillHash = 0, int primarySkillEnterHash = 0, int primarySkillHash = 0, int primarySkillExitHash = 0)
            {
                this.sid = sid;
                this.idleHash = idleHash;
                this.secondarySkillHash = secondarySkillHash;
                this.primarySkillEnterHash = primarySkillEnterHash;
                this.primarySkillHash = primarySkillHash;
                this.primarySkillExitHash = primarySkillExitHash;
            }
        }

        public static Dictionary<int, HeroData> heroDatas = new Dictionary<int, HeroData>()
        {
            { 201, new HeroData(201, 213165597, 186524081, 0, 0, 0) },  // 狗 Dog 102
            { 205, new HeroData(205, 213165597, 186524081, -1165874225, 0, 0) },  // 猫 Cat 101
            { 206, new HeroData(206, 213165597, 284930434, 1981930497, 0, -282554949) },  // 鸟 Bird 103
            { 207, new HeroData(207, 213165597, 1251315677, -1165874225, 0, 0) },  // 老虎 Tiger
            { 212, new HeroData(212, 213165597, 1684128848, 0, 0, 0) },  // 兔子 Rabbit
            { 213, new HeroData(213, -308670424, -308670424, 449243048, 1046461363, 1187555745) },  // 龟龟 Turtle
            { 214, new HeroData(214, 213165597, 186524081, -1697787571, 0, 0) },  // 猴子 Monkey
            { 215, new HeroData(215, 213165597, -1728220380, 95846934, 0, -1778958746) },  // 狐狸 Fox
            { 216, new HeroData(216, 210248539, -1725670550, -1165874225, 709821075, -1369725717) },  // 猫头鹰 Owl
            { 217, new HeroData(217, 213165597, -2036359730, -799370238, 0, 0) },  // 小熊猫 Red Panda 114
            { 218, new HeroData(218, 213165597, 186524081, -2062435787, 0, 0) },  // 山羊 Goat 116
            { 219, new HeroData(219, 213165597, -803373760, 0, 0, 0) }  // 松鼠 Squirrel 117
        };

        public static HeroData GetHeroData(int heroID)
        {
            return heroDatas.GetValueOrDefault(heroID, defaultHeroData);
        }
    }
}
