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
            { 201, new HeroData(201, 213165597, 186524081, 0, 0, 0) },  // 狗
            { 205, new HeroData(205, 213165597, 186524081, -1165874225, 0, 0) },  // 猫
            { 206, new HeroData(206, 213165597, 284930434, 1981930497, 0, -282554949) },  // 鸟
            { 207, new HeroData(207, 213165597, 1251315677, -1165874225, 0, 0) },  // 老虎
            { 212, new HeroData(212, 213165597, 1684128848, 0, 0, 0) },  // 兔子
            { 213, new HeroData(213, -308670424, -308670424, 449243048, 1046461363, 1187555745) },  // 龟龟
            { 214, new HeroData(214, 213165597, 186524081, -1697787571, 0, 0) },  // 猴子
            { 215, new HeroData(215, 213165597, -1728220380, 95846934, 0, -1778958746) }  // 狐狸
        };

        public static HeroData GetHeroData(int heroID)
        {
            return heroDatas.GetValueOrDefault(heroID, defaultHeroData);
        }
    }
}
