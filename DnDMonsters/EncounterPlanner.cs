using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DnDMonsters
{
    public partial class EncounterPlanner : Form
    {

        #region Public
        public Dictionary<string, Monster> Monsters = new Dictionary<string, Monster>();
        public IanUtility.SortedBindingList<ActiveMonster> activeMonsters = new IanUtility.SortedBindingList<ActiveMonster>();
        public static Random rand = new Random();
        
        public EncounterPlanner()
        {
            InitializeComponent();

            if (Properties.Settings.Default.MonsterFile != "") 
                LoadMonsters(Properties.Settings.Default.MonsterFile);
            if (Properties.Settings.Default.Encounters != "")
            {
                LoadAllEncounters();

                SaveEncounterList.RaiseListChangedEvents = false;
                SaveEncounterList.Clear();
                foreach (string ss in SavedEncounters.Keys) SaveEncounterList.Add(ss);
                SaveEncounterList.RaiseListChangedEvents = true;
                
                ((IBindingList)SaveEncounterList).ApplySort(null, ListSortDirection.Ascending);
            }

            ((IBindingList)activeMonsters).ApplySort(null, ListSortDirection.Descending);
            activeMonsters.ListChanged += activeMonsters_ListChanged;

            BindingSource bind = new BindingSource(SaveEncounterList, null);
            comboEncounters.DisplayMember = "Key";
            comboEncounters.DataSource = bind;
            comboEncounters.SelectedIndex = -1;
        }

        void activeMonsters_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset)
            {
                flowMonsters.Controls.Clear();
                flowMonsters.Refresh();
                foreach (ActiveMonster m in activeMonsters)
                {
                    TableLayoutPanel p = DisplayMonster(m, splitContainer1.Panel2.Width);
                    flowMonsters.Controls.Add(p);
                }
            }
            else if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                TableLayoutPanel p = DisplayMonster(activeMonsters[e.NewIndex], splitContainer1.Panel2.Width);
                flowMonsters.Controls.Add(p);
                checkGroupByName_CheckedChanged(null, null);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        public void ClearMonsters()
        {
            activeMonsters.Clear();
            flowMonsters.Controls.Clear();
        }
        public bool ToggleSuspendLayout()
        {
            mSuspendLayout = !mSuspendLayout;
            if (mSuspendLayout) flowMonsters.SuspendLayout();
            else flowMonsters.ResumeLayout();
            return mSuspendLayout;
        }
        public void AddMonster(Monster m, bool summary, int hp = -1)
        {
            if (m == null) return;

            ActiveMonster mm = new ActiveMonster();
            mm.Monster = m;
            if (hp == -1) mm.HP = m.GetHP();
            else mm.HP = hp;
            mm.Summary = summary;

            activeMonsters.Add(mm);
            
            
        }
        #endregion


        #region Events
        public event ListChangedEventHandler MonstersChanged;
        //public event ListChangedEventHandler ActiveMonstersChanged;

        void TriggerMonstersChanged()
        {
            if (MonstersChanged != null) MonstersChanged(this, null);
        }
        
        #endregion


        #region UI
        private void butReset_Click(object sender, EventArgs e)
        {
            ClearMonsters();
        }
        private void butAdd_Click(object sender, EventArgs e)
        {
            Monster m = comboMonsters.SelectedValue as Monster;
            AddMonster(m, checkSummaryOnly.Checked);
        }
        private void butPrint_Click(object sender, EventArgs e)
        {
            PrintPreviewDialog ppd = new PrintPreviewDialog();
            ppd.Document = new System.Drawing.Printing.PrintDocument();
            ppd.Document.PrintPage += Document_PrintPage;
            ppd.Document.BeginPrint += Document_BeginPrint;
            ppd.UseAntiAlias = false;
            ppd.ShowDialog();
        }
        private void butReloadMons_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.ReadOnlyChecked = true;
            dlg.ValidateNames = true;
            dlg.CheckFileExists = true;
            dlg.FileName = Properties.Settings.Default.MonsterFile;
            DialogResult r = dlg.ShowDialog();
            if (r == DialogResult.OK)
            {
                Properties.Settings.Default.MonsterFile = dlg.FileName;
                Properties.Settings.Default.Save();
                LoadMonsters(dlg.FileName);
            }

        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            flowMonsters.Controls.Clear();
            foreach (ActiveMonster m in activeMonsters)
            {
                TableLayoutPanel p = DisplayMonster(m, splitContainer1.Panel2.Width);
                flowMonsters.Controls.Add(p);
            }
        }
        private void checkGroupByName_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkGroupByName.Checked) return;
            /*
            activeMonsters.

            //sort and redisplay
            reordered = false;
            //activeMonsters.Sort(CompareActiveMonsters);
            if (reordered)
            {
                flowMonsters.Controls.Clear();
                flowMonsters.Refresh();
                foreach (ActiveMonster m in activeMonsters)
                {
                    TableLayoutPanel p = DisplayMonster(m, splitContainer1.Panel2.Width);
                    flowMonsters.Controls.Add(p);
                }
            }*/
        }
        private void butEncounter_Click(object sender, EventArgs e)
        {
            Encounter en = new Encounter();
            en.SetParent(this);
            en.Show();
        }
        private void butSaveEncounter_Click(object sender, EventArgs e)
        {
            SaveEncounter(comboEncounters.Text);
            SaveEncounterList.Add(comboEncounters.Text);
        }
        private void butLoadEncounter_Click(object sender, EventArgs e)
        {
            LoadEncounter(comboEncounters.Text);
        }
        private void butDelEncounter_Click(object sender, EventArgs e)
        {
            string enc = comboEncounters.Text;
            if (SavedEncounters.ContainsKey(enc)) { 
                SavedEncounters.Remove(comboEncounters.Text);
                SaveEncounterList.Remove(enc);
            }
            SaveAllEncounters();
        }
        #endregion

        #region Private
        enum LabelTypes { heading, subHeading, text, tiny, bold, italic };
        Label lastAdded = null;
        Label NewLabel(string text, LabelTypes t, int maxWidth)
        {
            Label l = new Label();
            l.Text = text;
            switch (t)
            {
                case LabelTypes.heading:
                    l.Font = new Font(FontFamily.GenericSansSerif, (float)16.0, FontStyle.Bold);
                    break;
                case LabelTypes.subHeading:
                    l.Font = new Font(FontFamily.GenericSansSerif, (float)10.0, FontStyle.Bold);
                    break;
                case LabelTypes.bold:
                    l.Font = new Font(FontFamily.GenericSansSerif, (float)10.0, FontStyle.Bold);
                    break;
                case LabelTypes.italic:
                    l.Font = new Font(FontFamily.GenericSansSerif, (float)10.0, FontStyle.Italic);
                    break;
                case LabelTypes.tiny:
                    l.Font = new Font(FontFamily.GenericSansSerif, (float)9.0, FontStyle.Italic);
                    break;
                case LabelTypes.text:
                default:
                    l.Font = new Font(FontFamily.GenericSerif, (float)10.0, FontStyle.Regular);
                    break;
            }
            l.Anchor = AnchorStyles.Left;
            int width = maxWidth - 10;
            l.MaximumSize = new Size(width, 0);
            l.AutoSize = true;
            lastAdded = l;
            return l;
        }

        bool mSuspendLayout = false;
        string Modifier(int ability)
        {
            int i = ability / 2;
            i *= 2;
            i -= 5;
            string s = "";
            if (i > 0) s = "+";
            s += i.ToString();
            return s;
        }
        TableLayoutPanel DisplayMonster(ActiveMonster mm, int maxWidth){
            Monster m = mm.Monster;
            TableLayoutPanel p = new TableLayoutPanel();

            //Get some typical widths
            Graphics gg = flowMonsters.CreateGraphics();
            int width6 = gg.MeasureString("WWWWWW", NewLabel("", LabelTypes.text, 100).Font).ToSize().Width;

            //p.AutoScroll = true;
            p.AutoSize = true;
            p.Dock = DockStyle.Fill;
            p.BackColor = Color.White;
            p.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            p.MaximumSize = new System.Drawing.Size(maxWidth, 0);
            p.Refresh();
            p.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
            if (mm.Summary)
            {
                TableLayoutPanel summ_panel = new TableLayoutPanel();
                summ_panel.RowCount = 1;
                summ_panel.AutoSize = true;
                summ_panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                summ_panel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                Label l1 = NewLabel(m.Name + "    ", LabelTypes.heading, maxWidth*3/4);
                summ_panel.Controls.Add(l1);

                summ_panel.Controls.Add(NewLabel("HP:", LabelTypes.subHeading, maxWidth));
                TextBox textBoxHP = new TextBox();
                textBoxHP.Text = mm.HP.ToString();
                textBoxHP.Anchor = AnchorStyles.Left;
                summ_panel.Controls.Add(textBoxHP);

                p.Controls.Add(summ_panel);
            }
            else
            {


                //p.GrowStyle = TableLayoutPanelGrowStyle.AddColumns | TableLayoutPanelGrowStyle.AddRows;
                //Name
                p.Controls.Add(NewLabel(m.Name, LabelTypes.heading, maxWidth-20));
                //type, size
                p.Controls.Add(NewLabel(m.Size + " " + m.Type + ", " + m.Alignment, LabelTypes.tiny, maxWidth-20));
                //AC, Speed, HP
                TableLayoutPanel ac_line = new TableLayoutPanel();
                ac_line.AutoSize = true;
                ac_line.RowCount = 1;
                ac_line.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                ac_line.Controls.Add(NewLabel("AC:", LabelTypes.subHeading, maxWidth));
                ac_line.Controls.Add(NewLabel(m.AC.ToString(), LabelTypes.text, width6));
                ac_line.Controls.Add(NewLabel("Speed:", LabelTypes.subHeading, maxWidth));
                ac_line.Controls.Add(NewLabel(m.Speed, LabelTypes.text, width6));
                ac_line.Controls.Add(NewLabel("MaxHP:", LabelTypes.subHeading, maxWidth));
                ac_line.Controls.Add(NewLabel(m.HP, LabelTypes.text, width6));
                ac_line.Controls.Add(NewLabel("XP:", LabelTypes.subHeading, maxWidth));
                ac_line.Controls.Add(NewLabel(m.XP.ToString(), LabelTypes.text, width6));
                ac_line.Controls.Add(NewLabel("HP:", LabelTypes.subHeading, maxWidth));
                TextBox textBoxHP = new TextBox();
                textBoxHP.Text = mm.HP.ToString();
                ac_line.Controls.Add(textBoxHP);
                p.Controls.Add(ac_line);
                //Stats1
                int i = 0;
                TableLayoutPanel stats1_panel = new TableLayoutPanel();
                stats1_panel.MaximumSize = new System.Drawing.Size(maxWidth-ac_line.Left-10,0);
                stats1_panel.AutoSize = true;
                stats1_panel.GrowStyle = TableLayoutPanelGrowStyle.AddColumns;
                stats1_panel.RowCount = 2;
                foreach (string s in Enum.GetNames(typeof(Stats)))
                {
                    stats1_panel.Controls.Add(NewLabel(s, LabelTypes.subHeading, maxWidth), i, 0);
                    stats1_panel.Controls.Add(NewLabel(m.Stat[s].ToString()+" ("+Modifier(m.Stat[s])+")", LabelTypes.text, maxWidth), i, 1);
                }
                p.Controls.Add(stats1_panel);
                //Attacks
                p.Controls.Add(NewLabel("Attacks", LabelTypes.subHeading, maxWidth));
                if (m.nAttacks != "" && m.nAttacks != "1")
                    p.Controls.Add(NewLabel("Available attacks: " + m.nAttacks, LabelTypes.text, maxWidth - width6*3-10));
                TableLayoutPanel attack_panel = new TableLayoutPanel();
                attack_panel.AutoSize = true;
                attack_panel.ColumnCount = 6;
                attack_panel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                int row = 0;
                foreach (Attack a in m.Attacks)
                {
                    attack_panel.RowCount += 2;
                    attack_panel.Controls.Add(NewLabel("   " + a.Name, LabelTypes.bold, maxWidth - width6*6), 0, row);
                    Label la = NewLabel(a.Type.ToString(), LabelTypes.italic, width6*2);
                    attack_panel.Controls.Add(la, 1, row);
                    attack_panel.Controls.Add(NewLabel(a.ToHit(), LabelTypes.text, width6), 2, row);
                    attack_panel.Controls.Add(NewLabel(a.Damage, LabelTypes.text, width6*2), 3, row);
                    if (a.Type == Attack.AttackType.Ranged)
                    {
                        if (a.MaxRangeDisadv == 0)
                            attack_panel.Controls.Add(NewLabel("Range: " + a.MaxRange.ToString(), LabelTypes.text, width6*2), 4, row);
                        else
                            attack_panel.Controls.Add(NewLabel("Range: " + a.MaxRange.ToString() +
                                " / " + a.MaxRangeDisadv.ToString(), LabelTypes.text, width6*3), 4, row);
                    }
                    if (a.Special != "")
                    {
                        attack_panel.Controls.Add(NewLabel(a.Special, LabelTypes.text, maxWidth - la.Right - 10), 1, row + 1);
                        attack_panel.SetColumnSpan(lastAdded, 4);
                    }

                    row += 2;
                }
                p.Controls.Add(attack_panel);
                //Spells
                if (m.SpellDC > 0)
                {
                    p.Controls.Add(NewLabel("Spells", LabelTypes.subHeading, maxWidth-50));
                    p.Controls.Add(NewLabel("    DC=" + m.SpellDC.ToString() + " Attack=" + Attack.ToHit(m.SpellRngAttack), LabelTypes.text, maxWidth-50));
                    TableLayoutPanel spell_panel = new TableLayoutPanel();
                    spell_panel.AutoSize = true;
                    spell_panel.RowCount = 1 + m.Spells.Count;
                    spell_panel.ColumnCount = 2;
                    spell_panel.Controls.Add(NewLabel("Cantrips:", LabelTypes.bold, maxWidth), 0, 0);
                    spell_panel.Controls.Add(NewLabel(m.SpellsCantrips, LabelTypes.italic, maxWidth - lastAdded.Right-30), 1, 0);
                    for (int lvl = 1; lvl <= m.Spells.Count; lvl++)
                    {
                        if (m.Spells[lvl].Item1 <= 0) continue;
                        spell_panel.Controls.Add(NewLabel("Level " + lvl.ToString() + " (" + m.Spells[lvl].Item1.ToString() + " slots):",
                            LabelTypes.bold, maxWidth), 0, lvl);
                        spell_panel.Controls.Add(NewLabel(m.Spells[lvl].Item2, LabelTypes.italic, maxWidth - lastAdded.Right - 30), 1, lvl);
                    }
                    p.Controls.Add(spell_panel);
                }
                //Senses, Vulns, 
                TableLayoutPanel data_panel = new TableLayoutPanel();
                data_panel.AutoSize = true;
                int data_panel_row = 0;
                if (m.Saves != "")
                {
                    data_panel.Controls.Add(NewLabel("Saves:", LabelTypes.bold, maxWidth), 0, data_panel_row);
                    data_panel.Controls.Add(NewLabel(m.Saves, LabelTypes.text, maxWidth-lastAdded.Right-20), 1, data_panel_row++);
                }
                if (m.Skills != "")
                {
                    data_panel.Controls.Add(NewLabel("Skills:", LabelTypes.bold, maxWidth), 0, data_panel_row);
                    data_panel.Controls.Add(NewLabel(m.Skills, LabelTypes.text, maxWidth - lastAdded.Right - 20), 1, data_panel_row++);
                }
                if (m.Vuln != "")
                {
                    data_panel.Controls.Add(NewLabel("Vulnerable:", LabelTypes.bold, maxWidth), 0, data_panel_row);
                    data_panel.Controls.Add(NewLabel(m.Vuln, LabelTypes.text, maxWidth - lastAdded.Right - 20), 1, data_panel_row++);
                }
                if (m.Resist != "")
                {
                    data_panel.Controls.Add(NewLabel("Resistant:", LabelTypes.bold, maxWidth), 0, data_panel_row);
                    data_panel.Controls.Add(NewLabel(m.Resist, LabelTypes.text, maxWidth - lastAdded.Right - 20), 1, data_panel_row++);
                }
                if (m.Immune != "")
                {
                    data_panel.Controls.Add(NewLabel("Immunities:", LabelTypes.bold, maxWidth), 0, data_panel_row);
                    data_panel.Controls.Add(NewLabel(m.Immune, LabelTypes.text, maxWidth - lastAdded.Right - 20), 1, data_panel_row++);
                }

                data_panel.Controls.Add(NewLabel("Senses:", LabelTypes.bold, maxWidth), 0, data_panel_row);
                string sense = "";
                if (m.Darkvision > 0) sense += "Darkvision " + m.Darkvision.ToString();
                if (m.Truesight > 0) sense += (sense != "" ? ", " : "") + "Truevision " + m.Truesight.ToString();
                if (m.Blindsight > 0) sense += (sense != "" ? ", " : "") + "Blindsight " + m.Blindsight.ToString();
                if (m.PassivePerception > 0) sense += (sense != "" ? ", " : "") + "Passive Perception " + m.PassivePerception.ToString();
                data_panel.Controls.Add(NewLabel(sense, LabelTypes.text, maxWidth - lastAdded.Right - 20), 1, data_panel_row++);
                p.Controls.Add(data_panel);
                //Feats
                if (m.Feats.Count > 0)
                {
                    p.Controls.Add(NewLabel("Feats", LabelTypes.subHeading, maxWidth));
                    TableLayoutPanel feat_panel = new TableLayoutPanel();
                    feat_panel.MaximumSize = new System.Drawing.Size(maxWidth - 10, 0);
                    feat_panel.AutoSize = true;
                    feat_panel.ColumnCount = 2;
                    feat_panel.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
                    int feat_row = 0;
                    int max_width = 0;
                    Graphics g = this.CreateGraphics();
                    foreach (string f in m.Feats.Keys)
                    {
                        Label l = NewLabel("  " + f, LabelTypes.bold, maxWidth);
                        int w = g.MeasureString(l.Text, l.Font).ToSize().Width;
                        if (max_width < w) max_width = w;
                        feat_panel.Controls.Add(l, 0, feat_row++);
                    }
                    feat_row = 0;
                    max_width = maxWidth - 40 - max_width;
                    foreach (string f in m.Feats.Keys)
                    {
                        Label l = NewLabel(m.Feats[f], LabelTypes.text, maxWidth);
                        l.MaximumSize = new System.Drawing.Size(max_width, 0);
                        feat_panel.Controls.Add(l, 1, feat_row++);
                    }
                    p.Controls.Add(feat_panel);
                }
                //Descr
                if (m.Descr != "")
                {
                    p.Controls.Add(NewLabel("Description", LabelTypes.subHeading, maxWidth));
                    p.Controls.Add(NewLabel("   " + m.Descr, LabelTypes.text, maxWidth-lastAdded.Right-20));
                }
            }
            return p;
        }

    
        void Document_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            printIdx = 0;
            
        }
        static int printIdx = 0;
        void Document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            int bottom = 0;
            for (int i = printIdx; i < activeMonsters.Count;i++)
            {
                TableLayoutPanel p = DisplayMonster(activeMonsters[i], e.MarginBounds.Width);
                p.PerformLayout();
                Size sz = p.GetPreferredSize(new Size(100, 100));
                p.SetBounds(0, 0, sz.Width, sz.Height);
                if (bottom == 0 || bottom + sz.Height < e.MarginBounds.Height)
                {
                    using (Bitmap b = new Bitmap(sz.Width, sz.Height))
                    {
                        p.DrawToBitmap(b, new Rectangle(0, 0, sz.Width, sz.Height));
                        e.Graphics.DrawImage(b, new Point(e.MarginBounds.Left, bottom+e.MarginBounds.Top));
                        bottom += sz.Height + 30;
                    }
                }
                else
                {
                    printIdx = i;
                    e.HasMorePages = true;
                    return;
                }
            }
            e.HasMorePages = false;
            return;
        }


        string GetConnectionString(string file)
        {
            Dictionary<string, string> props = new Dictionary<string, string>();

            // XLSX - Excel 2007, 2010, 2012, 2013
            props["Provider"] = "Microsoft.ACE.OLEDB.12.0";
            props["Extended Properties"] = "Excel 12.0 XML; HDR=YES; IMEX=1";
            props["Data Source"] = file;

            // XLS - Excel 2003 and Older
            //props["Provider"] = "Microsoft.Jet.OLEDB.4.0";
            //props["Extended Properties"] = "Excel 8.0";
            //props["Data Source"] = "C:\\MyExcel.xls";

            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, string> prop in props)
            {
                sb.Append(prop.Key);
                sb.Append("=\"");
                sb.Append(prop.Value);
                sb.Append("\";");
            }

            return sb.ToString();
        }
        string GetOdbcConnectionString(string folder)
        {
            string ret = "Driver={Microsoft Text Driver (*.txt; *.csv)};Dbq=";
            ret += folder + ";Extended Properties=\"Text;HDR=No;FMT=Delimited\"";
            return ret;
        }
        void LoadMonsters(string file)
        {
            Monsters = new Dictionary<string,Monster>();

            try
            {
                DataTable data = new DataTable();

                int i = file.LastIndexOf('\\');
                string fname = file.Substring(i + 1);
                string folder = file.Substring(0, i);
                using (OdbcConnection conn = new OdbcConnection(GetOdbcConnectionString(folder)))
                {
                    conn.Open();
                    //OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", conn);
                    OdbcDataAdapter adapter = new OdbcDataAdapter("SELECT * FROM [" + fname + "]", conn);
                    adapter.Fill(data);
                }
                /*            using (OleDbConnection conn = new OleDbConnection(GetConnectionString(file)))
                            {
                                conn.Open();
                                //OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [Sheet1$]", conn);
                                OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + file + "]", conn);
                                adapter.Fill(data);
                            }*/

                foreach (DataRow r in data.Rows)
                {
                    Monster m = Monster.FromDataRow(r);
                    if (m != null) Monsters.Add(m.Name, m);
                }


                //Update combo
                comboMonsters.Items.Clear();
                BindingSource bind = new BindingSource(Monsters, null);
                comboMonsters.DataSource = bind;
                comboMonsters.DisplayMember = "Key";
                comboMonsters.ValueMember = "Value";
                if (Monsters.Count > 0) comboMonsters.SelectedIndex = 0;
                else comboMonsters.SelectedIndex = -1;

                TriggerMonstersChanged();
            }
            catch (Exception) { MessageBox.Show("Couldn't load monsters", "Error"); }
        }

        

        #region Encounter save/load
        IanUtility.SortedBindingList<string> SaveEncounterList = new IanUtility.SortedBindingList<string>();
        Dictionary<string, List<Tuple<string, int, bool>>> SavedEncounters = new Dictionary<string, List<Tuple<string, int, bool>>>();
        void LoadAllEncounters()
        {
            string s = Properties.Settings.Default.Encounters;
            if (s == "" || s == null) return;

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(s)))
            {
                BinaryFormatter bf = new BinaryFormatter();
                SavedEncounters=(Dictionary<string,List<Tuple<string,int,bool>>>)bf.Deserialize(ms);

            }

            
        }
        void SaveAllEncounters()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, SavedEncounters);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                Properties.Settings.Default.Encounters = Convert.ToBase64String(buffer);
                Properties.Settings.Default.Save();
            }
        }
        void SaveEncounter(string name)
        {
            List<Tuple<string, int, bool>> enc = new List<Tuple<string, int, bool>>();
            foreach (ActiveMonster m in activeMonsters)
            {
                Tuple<string, int, bool> t = new Tuple<string, int, bool>(m.Monster.Name, m.HP, m.Summary);
                enc.Add(t);
            }
            if (SavedEncounters.ContainsKey(name))
            {
                DialogResult dlg = MessageBox.Show("An encounter with name " + name + " already exists. Overwrite?", "Warning", MessageBoxButtons.YesNo);
                if (dlg == System.Windows.Forms.DialogResult.No) return;
                SavedEncounters[name] = enc;
            }
            else SavedEncounters.Add(name, enc);

            SaveAllEncounters();
        }
        void LoadEncounter(string name)
        {
            ClearMonsters();
            if (!SavedEncounters.ContainsKey(name)) return;

            ToggleSuspendLayout();
            foreach(Tuple<string,int,bool> t in SavedEncounters[name]){
                Monster m = null;
                if (!Monsters.ContainsKey(t.Item1)) 
                    continue;
                m = Monsters[t.Item1];
                if (m == null) 
                    continue;
                AddMonster(m, t.Item3, t.Item2);
            }
            ToggleSuspendLayout();
        }
        #endregion

        #endregion

    }
}
