using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DnDMonsters
{
    public enum Stats { STR=0, DEX, CON, INT, WIS, CHA }

    public class Monster
    {
        public string Name;
        public int AC;
        public string HP;
        public string Speed;
        public Dictionary<string, int> Stat = new Dictionary<string, int>();
        //public int STR;
        //public int DEX;
        //public int CON;
        //public int INT;
        //public int WIS;
        //public int CHA;
        public string Skills;
        public string Vuln;
        public string Resist;
        public string Immune;
        public int Darkvision;
        public int Truesight;
        public int Blindsight;
        public int PassivePerception;
        public string Saves;
        public int XP;
        public string nAttacks;
        public List<Attack> Attacks = new List<Attack>();
        string FeatsLong;
        public int SpellDC;
        public int SpellRngAttack;
        public string SpellsCantrips;
        public Dictionary<int, Tuple<int, string>> Spells = new Dictionary<int, Tuple<int, string>>();
        public string Descr;
        public string Size;
        public string Type;
        public string Alignment;

        public Dictionary<string, string> Feats = new Dictionary<string, string>();
        public static Monster FromDataRow(DataRow r)
        {
            Monster m = new Monster();
            m.Name = r["Name"].ToString();
            m.AC = GetInt(r, "AC");
            m.HP = r["HP"].ToString();
            m.Speed = r["Speed"].ToString();
            if (m.Speed == "") m.Speed = "30ft";
            foreach (string s in Enum.GetNames(typeof(Stats)))
                m.Stat[s] = GetInt(r, s);
            m.Skills = r["Skills"].ToString();
            m.Vuln = r["Vulnerable"].ToString();
            m.Resist = r["Resist"].ToString();
            m.Immune = r["Immune"].ToString();
            m.Darkvision = GetInt(r, "Darkvision");
            m.Truesight = GetInt(r, "Truesight");
            m.Blindsight = GetInt(r, "Blindsight");
            m.PassivePerception = GetInt(r, "Passive Perception");
            m.Saves = r["Saves"].ToString();
            m.XP = GetInt(r, "XP");
            m.nAttacks = r["#Attack"].ToString();
            Attack a = Attack.Melee(r["Melee"].ToString(), GetInt(r, "MeleeAtk"), r["MeleeDmg"].ToString(), r["MeleeSpecial"].ToString());
            if (a != null) m.Attacks.Add(a);
            a = Attack.Ranged(r["Rng"].ToString(), GetInt(r, "RngAtk"), r["RngDmg"].ToString(), GetInt(r, "RngShort"),
                GetInt(r, "RngFull"), r["RngSpecial"].ToString());
            if (a != null) m.Attacks.Add(a);
            List<Attack> aa = Attack.FromOther(r["Other"].ToString());
            if (aa != null) m.Attacks.AddRange(aa);
            m.FeatsLong = r["Feats"].ToString();
            m.SpellDC = GetInt(r, "Spell DC");
            m.SpellRngAttack = GetInt(r, "Spell Rng Atk");
            m.SpellsCantrips = r["Cantrips"].ToString();
            m.Spells.Add(1, new Tuple<int, string>(GetInt(r, "#1st Level"), r["1st Level"].ToString()));
            m.Spells.Add(2, new Tuple<int, string>(GetInt(r, "#2nd Level"), r["2nd Level"].ToString()));
            m.Spells.Add(3, new Tuple<int, string>(GetInt(r, "#3rd Level"), r["3rd Level"].ToString()));
            m.Descr = r["Descr"].ToString();
            m.Size = r["Size"].ToString();
            m.Type = r["Type"].ToString();
            m.Alignment = r["Alignment"].ToString();

            m.ParseFeats();

            return m;
        }

        public override string ToString()
        {
            return Name;
        }
        Regex hpReg = new Regex(@"(\d+)d(\d+)(\+(\d+))?");
        public int GetHP()
        {
            Match m = hpReg.Match(HP);
            if (!m.Success)
            {
                int i;
                if (Int32.TryParse(HP, out i)) return i;
                return -1;
            }
            //1,2,4
            try
            {
                int ret = 0;
                if(m.Groups[4].Value!="") Int32.Parse(m.Groups[4].Value);
                int nDice = Int32.Parse(m.Groups[1].Value);
                int szDice = Int32.Parse(m.Groups[2].Value);
                for (int i = 0; i < nDice;i++)
                {
                    ret += EncounterPlanner.rand.Next(szDice) + 1;
                }
                return ret;
            }
            catch (Exception)
            {
                return -1;
            }
        }


        static int GetInt(DataRow r, string ColumnName)
        {
            string s = r[ColumnName].ToString();
            if (s == "") return 0;
            decimal i = 0;
            if (decimal.TryParse(s, out i)) return (int)i;
            return 0;
        }
        void ParseFeats()
        {
            Feats = new Dictionary<string, string>();
            string[] s = FeatsLong.Split(new string[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
            int idx = 0;
            string feat = "";
            while(idx<s.Length){
                int colon = s[idx].IndexOf(':');
                if((colon!=-1 || idx==s.Length-1) && feat!=""){
                    int fColon = feat.IndexOf(':');
                    if (fColon!=-1)
                        Feats.Add(feat.Substring(0, fColon), feat.Substring(fColon + 1));
                    else Feats.Add(feat, "");
                    feat = "";
                }
                feat += s[idx]+". ";
                idx++;
            }
        }
    }
}
