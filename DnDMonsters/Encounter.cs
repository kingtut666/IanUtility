using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DnDMonsters
{
    public partial class Encounter : Form
    {
        public Encounter()
        {
            InitializeComponent();
        }

        EncounterPlanner parent;
        Dictionary<int, TreeNode> nodesByXP = new Dictionary<int, TreeNode>();

        #region Public methods
        public void SetParent(EncounterPlanner frm)
        {
            parent = frm;

            parent.MonstersChanged += parent_MonstersChanged;
            parent.ActiveMonstersChanged += parent_ActiveMonstersChanged;
            
            numNPlayers.Value = Properties.Settings.Default.NumPlayers;
            numLevel.Value = Properties.Settings.Default.PlayersLevel;
            comboDifficulty.SelectedIndex = Properties.Settings.Default.Difficulty;
            CalcXP();
            PopulateMonsterTree();
            DisableMonsterTreeByXP();
        }
        #endregion

        #region Event Handlers
        void parent_ActiveMonstersChanged(object sender, ListChangedEventArgs e)
        {
            CalcXP();
        }

        void parent_MonstersChanged(object sender, ListChangedEventArgs e)
        {
            PopulateMonsterTree();
            DisableMonsterTreeByXP();
        }
        #endregion

        
        #region UI
        private void butRollMonsters_Click(object sender, EventArgs e)
        {
            checkListMonsters.Items.Clear();
            availMonsters = new Dictionary<int,List<Monster>>();
            minMonsterXP = 0;
            foreach (TreeNode n in treeMonstersByXP.Nodes)
            {
                foreach (TreeNode n2 in n.Nodes)
                {
                    if (!n2.Checked) continue;
                    Monster m = n2.Tag as Monster;
                    if (minMonsterXP == 0 || minMonsterXP >= m.XP) minMonsterXP = m.XP;
                    if (!availMonsters.ContainsKey(m.XP)) availMonsters.Add(m.XP, new List<Monster>());
                    availMonsters[m.XP].Add(m);
                }
            }

            decimal XP = numTargetXP.Value / EncounterMultiplier((int)numMonsters.Value);

            List<Monster> mons = TryRoll((int)XP, (int)numMonsters.Value, false);
            if (mons != null)
            {
                foreach (Monster m in mons)
                {
                    checkListMonsters.Items.Add(m, CheckState.Checked);
                }
            }
        }

        private void butAddSelected_Click(object sender, EventArgs e)
        {
            parent.ToggleSuspendLayout();
            foreach (object o in checkListMonsters.CheckedItems)
            {
                Monster m = o as Monster;
                bool Summary = false;
                foreach (ActiveMonster a in parent.activeMonsters)
                {
                    if (a.Monster == m && !a.Summary)
                    {
                        Summary = true;
                        break;
                    }
                }
                parent.AddMonster(m, Summary);
            }
            parent.ToggleSuspendLayout();
        }

        private void butClear_Click(object sender, EventArgs e)
        {
            parent.ClearMonsters();
        }


        private void numNPlayers_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.NumPlayers = (int)numNPlayers.Value;
            Properties.Settings.Default.Save();
            CalcXP();
        }

        private void numLevel_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.PlayersLevel = (int)numLevel.Value;
            Properties.Settings.Default.Save();
            CalcXP();
        }

        private void comboDifficulty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Difficulty = comboDifficulty.SelectedIndex;
            Properties.Settings.Default.Save();
            CalcXP();
        }

        private void treeMonstersByXP_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown) return;
            if (e.Node.Tag == null)
            {
                foreach (TreeNode t in e.Node.Nodes)
                {
                    t.Checked = e.Node.Checked;
                }
            }
            else
            {

            }
        }

        private void numTargetXP_ValueChanged(object sender, EventArgs e)
        {
            DisableMonsterTreeByXP();
        }
        #endregion



        void CalcXP()
        {
            if (comboDifficulty.SelectedIndex < 0)
            {
                lblThresholdXP.Text = " - ";
                lblCurrentXP.Text = " - ";
                lblRemainingXP.Text = " - ";
                return;
            }

            int partyXP = (int)(numNPlayers.Value);
            partyXP *= diff[(int)numLevel.Value, comboDifficulty.SelectedIndex];

            lblThresholdXP.Text = partyXP.ToString();

            int monsterXP = 0;
            foreach (ActiveMonster m in parent.activeMonsters)
            {
                monsterXP += m.Monster.XP;
            }

            monsterXP = (int)((decimal)monsterXP * EncounterMultiplier(parent.activeMonsters.Count));

            lblCurrentXP.Text = monsterXP.ToString();
            lblRemainingXP.Text = (partyXP - monsterXP).ToString();
            if (partyXP < monsterXP) numTargetXP.Value = 0;
            else numTargetXP.Value = partyXP - monsterXP;

        }
        
        void PopulateMonsterTree()
        {
            treeMonstersByXP.Nodes.Clear();
            nodesByXP.Clear();

            foreach (Monster m in parent.Monsters.Values)
            {
                TreeNode xp = null;
                if (nodesByXP.ContainsKey(m.XP))
                {
                    xp = nodesByXP[m.XP];
                }
                else
                {
                    xp = new TreeNode(m.XP.ToString());
                    treeMonstersByXP.Nodes.Add(xp);
                    nodesByXP.Add(m.XP, xp);
                }

                TreeNode t = new TreeNode(m.Name);
                t.Tag = m;
                xp.Nodes.Add(t);
            }

            //order nodes
            treeMonstersByXP.TreeViewNodeSorter = new TreeSorter();
            treeMonstersByXP.Sort();

            //check all
            foreach (TreeNode t1 in treeMonstersByXP.Nodes)
            {
                t1.Checked = true;

                foreach (TreeNode t2 in t1.Nodes)
                    t2.Checked = true;
            }

        }
        public class TreeSorter : System.Collections.IComparer
        {
            public int Compare(object a, object b)
            {
                TreeNode x = a as TreeNode;
                TreeNode y = b as TreeNode;

                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                if (x.Tag == null)
                {
                    int i, j;
                    if (!Int32.TryParse(x.Text, out i)) return -1;
                    if (!Int32.TryParse(y.Text, out j)) return 1;
                    if (i == j) return 0;
                    if (i < j) return -1;
                    return 1;
                }
                else
                {
                    return String.Compare(x.Text, y.Text);
                }
            }
        }
        void DisableMonsterTreeByXP()
        {
            foreach (TreeNode n in treeMonstersByXP.Nodes)
            {
                int XP = Int32.Parse(n.Text);
                if (XP > numTargetXP.Value)
                {
                    n.ForeColor = Color.Gray;
                    n.Checked = false;
                    foreach (TreeNode n2 in n.Nodes)
                    {
                        n2.ForeColor = Color.Gray;
                        n2.Checked = false;
                    }
                }
                else
                {
                    if (n.ForeColor == Color.Gray && checkResetTree.Checked)
                    {
                        n.Checked = true;
                        foreach (TreeNode n2 in n.Nodes) n2.Checked = true;
                    }
                    n.ForeColor = Color.Black;
                    foreach (TreeNode n2 in n.Nodes) n2.ForeColor = Color.Black;
                }
            }
        }



        int[,] diff = new[,] { {1,1,1,1}, 
            { 25, 50, 75, 100}, //Level1
            {50,100,150,200},
            {75,150,225,400},
            {125,250,375,500},
            {250,500,750,1100}, //Level5
            {300,600,900,1400},
            {350,750,1100,1700},
            {450,900,1400,2100},
            {550,1100,1600,2400},
            {600,1200,1900,2800}, //Level10
            {800,1600,2400,3600},
            {1000,2000,3000,4000},
            {1100,2200,3400,5100},
            {1250,2500,3800,5700},
            {1400,2800,4300,6400}, //Level15
            {1600,3200,4800,7200},
            {2000,4200,6300,9500},
            {2100,4200,6300,9500},
            {2400,4900,7300,10900},
            {2800,5700,8500,12700} // Level20        
        };
        int minMonsterXP = 0;
        Dictionary<int, List<Monster>> availMonsters = null;
        
        List<Monster> ListAvailMonsters(int minXP, int maxXP)
        {
            List<Monster> ret = new List<Monster>();
            foreach (int xp in availMonsters.Keys)
            {
                if (minXP != -1 && minXP > xp) continue;
                if (maxXP != -1 && maxXP < xp) continue;
                ret.AddRange(availMonsters[xp]);
            }
            return ret;
        }
        List<Monster> GetMonsterGroup(int XP, int nMonsters)
        {
            List<Monster> ret = new List<Monster>();
            if (XP / nMonsters < minMonsterXP) return null;
            int targXP = XP / nMonsters;
            int highestSelected = 0;
            foreach (int xp in availMonsters.Keys)
                if (highestSelected < xp && xp < targXP && availMonsters[xp].Count > 0) highestSelected = xp;
            if (highestSelected == 0) return null;
            int i = EncounterPlanner.rand.Next(availMonsters[highestSelected].Count);
            Monster m = availMonsters[highestSelected][i];
            for (int j = 0; j < nMonsters; j++) ret.Add(m);
            return ret;
        }
        Monster FindBoss(int maxXP)
        {
            int xp = 0;
            foreach (int k in availMonsters.Keys)
                if (xp < k && k < maxXP) xp = k;
            if (xp == 0) return null;
            int i = EncounterPlanner.rand.Next(availMonsters[xp].Count);
            return availMonsters[xp][i];
        }
        List<Monster> TryRoll(int XP, int nMonsters, bool specialPicked)
        {
            List<Monster> ret = new List<Monster>();
            if (XP < minMonsterXP || nMonsters == 0) return null;
            if (radioRandom.Checked)
            {
                //pick a monster, with XP
                List<Monster> possible = ListAvailMonsters(-1, XP);
                if (possible == null || possible.Count == 0) return null;
                int i = 0;
                Monster m = null;
                i = EncounterPlanner.rand.Next(possible.Count);
                m = possible[i];
                ret = TryRoll(XP - m.XP, nMonsters - 1, true);
                if (ret == null) ret = new List<Monster>();
                ret.Add(m);
                return ret;
            }
            else if (radioMaxGroup.Checked)
            {
                return GetMonsterGroup(XP, nMonsters);
            }
            else if (radioMaxSingle.Checked)
            {
                if (!specialPicked)
                {
                    //Pick largest
                    List<int> xps = availMonsters.Keys.ToList();
                    xps.Sort((a, b) => b.CompareTo(a));
                    Monster m = null;
                    foreach (int i in xps)
                    {
                        if (i >= XP) continue;
                        //Pick a monster at this level
                        int r = EncounterPlanner.rand.Next(availMonsters[i].Count);
                        m = availMonsters[i][r];
                        ret = GetMonsterGroup(XP - m.XP, nMonsters - 1);
                        if (ret != null)
                        {
                            ret.Add(m);
                            return ret;
                        }
                    }
                    return null;
                }
                else
                {
                    //Pick remainder
                    return GetMonsterGroup(XP, nMonsters);
                }
            }
            else if (radioBoss25.Checked)
            {
                Monster m = FindBoss((XP * 3) / 4);
                if (m == null) return null;
                ret = GetMonsterGroup(XP - m.XP, nMonsters - 1);
                ret.Add(m);
                return ret;
            }
            else if (radioBoss50.Checked)
            {
                Monster m = FindBoss(XP / 2);
                if (m == null) return null;
                ret = GetMonsterGroup(XP - m.XP, nMonsters - 1);
                ret.Add(m);
                return ret;
            }
            else if (radioBoss75.Checked)
            {
                Monster m = FindBoss(XP / 4);
                if (m == null) return null;
                ret = GetMonsterGroup(XP - m.XP, nMonsters - 1);
                ret.Add(m);
                return ret;
            }

            return null;
        }
        decimal EncounterMultiplier(int nMonsters)
        {
            if (nMonsters == 1) return 1;
            else if (nMonsters == 2) return 1.5m;
            else if (nMonsters >= 3 && nMonsters <= 6) return 2;
            else if (nMonsters >= 7 && nMonsters <= 10) return 2.5m;
            else if (nMonsters >= 11 && nMonsters <= 14) return 3;
            else return 4;
        }
        
    }
}
