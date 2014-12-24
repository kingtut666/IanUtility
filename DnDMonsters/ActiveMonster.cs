using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DnDMonsters
{
    public class ActiveMonster : IComparer<ActiveMonster>
    {
        public Monster Monster;
        public int HP;
        public bool Summary;

        int IComparer<ActiveMonster>.Compare(ActiveMonster a, ActiveMonster b)
        {
            if (a == null && b == null) return 0;
            if (a == null)
            {
                return -1;
            }
            if (b == null) return 1;
            int i;
            //compare by name
            i = String.Compare(a.Monster.Name, b.Monster.Name, StringComparison.CurrentCultureIgnoreCase);
            if (i < 0)
            {
                return i;
            }
            else if (i > 0) return i;
            //compare by summary
            if (a.Summary != b.Summary)
            {
                if (a.Summary) return 1;
                else
                {
                    return -1; //b.Summary
                }
            }
            //compare by HP
            if (a.HP == b.HP) return 0;
            if (a.HP > b.HP)
            {
                return -1;
            }
            return 1;
        }
   
    }
}
