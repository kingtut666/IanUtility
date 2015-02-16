using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IanUtility
{
    public class TreeViewHelper
    {
        public static TreeNode AddNewTreeNode(TreeNode parent, string name, bool check, object tag=null)
        {
            TreeNode tn = new TreeNode(name);
            tn.Checked = check;
            tn.Tag = tag;
            parent.Nodes.Add(tn);
            return tn;
        }
        public static TreeNode AddNewTreeNode(TreeView parent, string name, bool check, object tag=null)
        {
            TreeNode tn = new TreeNode(name);
            tn.Checked = check;
            tn.Tag = tag;
            parent.Nodes.Add(tn);
            return tn;
        }
        public static TreeNode AddSubTreeNode<T>(TreeNode ultimateParent, Dictionary<T,TreeNode> parents, string name, bool check, object tag, T parentKey, string parentName="")
        {
            string mParentName = "";
            if (parentName == "" && parentKey!=null) mParentName = parentKey.ToString();
            else mParentName = parentName;
            TreeNode t = new TreeNode(name);
            t.Tag = tag;
            t.Checked = check;
            if (parents != null)
            {
                if (!parents.ContainsKey(parentKey))
                {
                    TreeNode parent = new TreeNode(mParentName);
                    parent.Checked = true;
                    ultimateParent.Nodes.Add(parent);
                    parents.Add(parentKey, parent);
                }
                parents[parentKey].Nodes.Add(t);
            }
            else ultimateParent.Nodes.Add(t);
            return t;
        }

        public enum UpdateType {  Checked, Disabled }
        public static void UpdateSetting<T>(Dictionary<T,TreeNode> root, bool newVal, T key, object tag, UpdateType type = UpdateType.Checked){
            if (root.ContainsKey(key))
            {
                foreach (TreeNode n in root[key].Nodes)
                {
                    if (n.Tag == tag)
                    {
                        if (type == UpdateType.Checked)
                        {
                            if (n.Checked != newVal)
                                n.Checked = newVal;
                        }
                        else if (type == UpdateType.Disabled)
                        {
                            System.Drawing.Color col;
                            if(newVal) col = System.Drawing.Color.Gray;
                            else col = System.Drawing.Color.Black;
                            if (n.ForeColor != col) n.ForeColor = col;
                        }
                        break;
                    }
                }
            }
        }
        public static void UpdateSetting(TreeNode root, bool newVal, object tag, UpdateType type = UpdateType.Checked)
        {
            foreach (TreeNode n in root.Nodes)
            {
                if (n.Tag == tag)
                {
                    if (type == UpdateType.Checked)
                    {
                        if (n.Checked != newVal)
                            n.Checked = newVal;
                    }
                    else if (type == UpdateType.Disabled)
                    {
                        System.Drawing.Color col;
                        if (newVal) col = System.Drawing.Color.Gray;
                        else col = System.Drawing.Color.Black;
                        if (n.ForeColor != col) n.ForeColor = col;
                    }
                    break;
                }
            }
        }
        public static void UpdateSupertree(TreeNode root)
        {
            bool check = false;
            bool nocheck = false;
            foreach (TreeNode n in root.Nodes)
            {
                if (check && nocheck) break;
                if (n.Checked) check = true;
                else nocheck = true;
            }
#pragma warning disable 0642
            if (check && nocheck) ;//gray
            else if ((check && !root.Checked) || (nocheck && root.Checked)) root.Checked = check;
#pragma warning restore 0642
        }
        public static void UpdateSupertree<T>(Dictionary<T,TreeNode> nodeDict, TreeNode root)
        {
            bool check = false;
            bool nocheck = false;
            foreach (TreeNode n in nodeDict.Values)
            {
                check = nocheck = false;
                foreach (TreeNode nn in n.Nodes)
                {
                    if (check && nocheck) break;
                    if (nn.Checked) check = true;
                    else nocheck = true;
                }
#pragma warning disable 0642
                if (check && nocheck) ;//gray
                else if ((check && !n.Checked) || (nocheck && n.Checked)) n.Checked = check;
#pragma warning restore 0642

            }
            UpdateSupertree(root);
        }



    
    }
}
