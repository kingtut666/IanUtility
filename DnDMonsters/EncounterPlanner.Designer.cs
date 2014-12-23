namespace DnDMonsters
{
    partial class EncounterPlanner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.butReset = new System.Windows.Forms.Button();
            this.butAdd = new System.Windows.Forms.Button();
            this.comboMonsters = new System.Windows.Forms.ComboBox();
            this.butReloadMons = new System.Windows.Forms.Button();
            this.butPrint = new System.Windows.Forms.Button();
            this.flowMonsters = new System.Windows.Forms.FlowLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkSummaryOnly = new System.Windows.Forms.CheckBox();
            this.checkGroupByName = new System.Windows.Forms.CheckBox();
            this.butEncounter = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.butSaveEncounter = new System.Windows.Forms.Button();
            this.butLoadEncounter = new System.Windows.Forms.Button();
            this.butDelEncounter = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butReset
            // 
            this.butReset.Location = new System.Drawing.Point(3, 5);
            this.butReset.Name = "butReset";
            this.butReset.Size = new System.Drawing.Size(75, 23);
            this.butReset.TabIndex = 0;
            this.butReset.Text = "Reset List";
            this.butReset.UseVisualStyleBackColor = true;
            this.butReset.Click += new System.EventHandler(this.butReset_Click);
            // 
            // butAdd
            // 
            this.butAdd.Location = new System.Drawing.Point(84, 5);
            this.butAdd.Name = "butAdd";
            this.butAdd.Size = new System.Drawing.Size(75, 23);
            this.butAdd.TabIndex = 1;
            this.butAdd.Text = "Add";
            this.butAdd.UseVisualStyleBackColor = true;
            this.butAdd.Click += new System.EventHandler(this.butAdd_Click);
            // 
            // comboMonsters
            // 
            this.comboMonsters.FormattingEnabled = true;
            this.comboMonsters.Location = new System.Drawing.Point(165, 7);
            this.comboMonsters.Name = "comboMonsters";
            this.comboMonsters.Size = new System.Drawing.Size(183, 21);
            this.comboMonsters.TabIndex = 2;
            // 
            // butReloadMons
            // 
            this.butReloadMons.Location = new System.Drawing.Point(774, 7);
            this.butReloadMons.Name = "butReloadMons";
            this.butReloadMons.Size = new System.Drawing.Size(134, 23);
            this.butReloadMons.TabIndex = 3;
            this.butReloadMons.Text = "Load/Reload Monsters";
            this.butReloadMons.UseVisualStyleBackColor = true;
            this.butReloadMons.Click += new System.EventHandler(this.butReloadMons_Click);
            // 
            // butPrint
            // 
            this.butPrint.Location = new System.Drawing.Point(693, 9);
            this.butPrint.Name = "butPrint";
            this.butPrint.Size = new System.Drawing.Size(75, 23);
            this.butPrint.TabIndex = 4;
            this.butPrint.Text = "Print";
            this.butPrint.UseVisualStyleBackColor = true;
            this.butPrint.Click += new System.EventHandler(this.butPrint_Click);
            // 
            // flowMonsters
            // 
            this.flowMonsters.AutoScroll = true;
            this.flowMonsters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowMonsters.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowMonsters.Location = new System.Drawing.Point(0, 0);
            this.flowMonsters.Name = "flowMonsters";
            this.flowMonsters.Size = new System.Drawing.Size(920, 447);
            this.flowMonsters.TabIndex = 5;
            this.flowMonsters.WrapContents = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.butDelEncounter);
            this.splitContainer1.Panel1.Controls.Add(this.butLoadEncounter);
            this.splitContainer1.Panel1.Controls.Add(this.butSaveEncounter);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox1);
            this.splitContainer1.Panel1.Controls.Add(this.butEncounter);
            this.splitContainer1.Panel1.Controls.Add(this.checkGroupByName);
            this.splitContainer1.Panel1.Controls.Add(this.checkSummaryOnly);
            this.splitContainer1.Panel1.Controls.Add(this.butPrint);
            this.splitContainer1.Panel1.Controls.Add(this.butReset);
            this.splitContainer1.Panel1.Controls.Add(this.butAdd);
            this.splitContainer1.Panel1.Controls.Add(this.butReloadMons);
            this.splitContainer1.Panel1.Controls.Add(this.comboMonsters);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.flowMonsters);
            this.splitContainer1.Size = new System.Drawing.Size(920, 511);
            this.splitContainer1.SplitterDistance = 60;
            this.splitContainer1.TabIndex = 6;
            // 
            // checkSummaryOnly
            // 
            this.checkSummaryOnly.AutoSize = true;
            this.checkSummaryOnly.Location = new System.Drawing.Point(367, 9);
            this.checkSummaryOnly.Name = "checkSummaryOnly";
            this.checkSummaryOnly.Size = new System.Drawing.Size(93, 17);
            this.checkSummaryOnly.TabIndex = 5;
            this.checkSummaryOnly.Text = "Summary Only";
            this.checkSummaryOnly.UseVisualStyleBackColor = true;
            // 
            // checkGroupByName
            // 
            this.checkGroupByName.AutoSize = true;
            this.checkGroupByName.Checked = true;
            this.checkGroupByName.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkGroupByName.Location = new System.Drawing.Point(466, 9);
            this.checkGroupByName.Name = "checkGroupByName";
            this.checkGroupByName.Size = new System.Drawing.Size(100, 17);
            this.checkGroupByName.TabIndex = 6;
            this.checkGroupByName.Text = "Group by Name";
            this.checkGroupByName.UseVisualStyleBackColor = true;
            this.checkGroupByName.CheckedChanged += new System.EventHandler(this.checkGroupByName_CheckedChanged);
            // 
            // butEncounter
            // 
            this.butEncounter.Location = new System.Drawing.Point(613, 8);
            this.butEncounter.Name = "butEncounter";
            this.butEncounter.Size = new System.Drawing.Size(75, 23);
            this.butEncounter.TabIndex = 7;
            this.butEncounter.Text = "Encounter";
            this.butEncounter.UseVisualStyleBackColor = true;
            this.butEncounter.Click += new System.EventHandler(this.butEncounter_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 36);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(183, 21);
            this.comboBox1.TabIndex = 8;
            // 
            // butSaveEncounter
            // 
            this.butSaveEncounter.Location = new System.Drawing.Point(201, 34);
            this.butSaveEncounter.Name = "butSaveEncounter";
            this.butSaveEncounter.Size = new System.Drawing.Size(99, 23);
            this.butSaveEncounter.TabIndex = 9;
            this.butSaveEncounter.Text = "Save Encounter";
            this.butSaveEncounter.UseVisualStyleBackColor = true;
            this.butSaveEncounter.Click += new System.EventHandler(this.butSaveEncounter_Click);
            // 
            // butLoadEncounter
            // 
            this.butLoadEncounter.Location = new System.Drawing.Point(306, 34);
            this.butLoadEncounter.Name = "butLoadEncounter";
            this.butLoadEncounter.Size = new System.Drawing.Size(96, 23);
            this.butLoadEncounter.TabIndex = 10;
            this.butLoadEncounter.Text = "Load Encounter";
            this.butLoadEncounter.UseVisualStyleBackColor = true;
            this.butLoadEncounter.Click += new System.EventHandler(this.butLoadEncounter_Click);
            // 
            // butDelEncounter
            // 
            this.butDelEncounter.Location = new System.Drawing.Point(408, 34);
            this.butDelEncounter.Name = "butDelEncounter";
            this.butDelEncounter.Size = new System.Drawing.Size(90, 23);
            this.butDelEncounter.TabIndex = 11;
            this.butDelEncounter.Text = "Del Encounter";
            this.butDelEncounter.UseVisualStyleBackColor = true;
            this.butDelEncounter.Click += new System.EventHandler(this.butDelEncounter_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 511);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butReset;
        private System.Windows.Forms.Button butAdd;
        private System.Windows.Forms.ComboBox comboMonsters;
        private System.Windows.Forms.Button butReloadMons;
        private System.Windows.Forms.Button butPrint;
        private System.Windows.Forms.FlowLayoutPanel flowMonsters;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.CheckBox checkSummaryOnly;
        private System.Windows.Forms.CheckBox checkGroupByName;
        private System.Windows.Forms.Button butEncounter;
        private System.Windows.Forms.Button butDelEncounter;
        private System.Windows.Forms.Button butLoadEncounter;
        private System.Windows.Forms.Button butSaveEncounter;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

