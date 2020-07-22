using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ClassifyFiles.UI.Util
{
    public class TreeViewHelper<TModel>
    {
        public TreeViewHelper(TreeView treeView, Func<TModel, TModel> getParent, Func<TModel, IList<TModel>> getSubItems)
        {
            TreeView = treeView;
            GetParent = getParent;
            GetSubItems = getSubItems;
        }

        public TreeView TreeView { get; }
        public Func<TModel, TModel> GetParent { get; }
        public Func<TModel, IList<TModel>> GetSubItems { get; }

        public void SelectItemWhileLoaded(TModel node, IList<TModel> rootNodes)
        {
            if (TreeView.IsLoaded)
            {
                SelectItem(node, rootNodes);
            }
            else
            {
                TreeView.Loaded += TreeView_Loaded;
                void TreeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
                {
                    TreeView.Loaded -= TreeView_Loaded;
                    SelectItem(node, rootNodes);
                }
            }
        }


        public void SelectItem(TModel node, IList<TModel> rootNodes)
        {
            Stack<TModel> nodes = new Stack<TModel>();
            //首先把结点从子到父放进一个站里，栈顶将是根节点
            while (!rootNodes.Contains(node))
            {
                nodes.Push(node);
                node = GetParent(node);
            }
            TreeViewItem treeViewItem = TreeView.ItemContainerGenerator
                .ContainerFromItem(node) as TreeViewItem;
            if (nodes.Count == 0)
            {
                //该项在最高层
                treeViewItem.IsSelected = true;
                treeViewItem.BringIntoView();
                return;
            }
            Expanded(true);
            void Expanded(bool top)
            {
                if (!top)
                {
                    treeViewItem = treeViewItem.ItemContainerGenerator
                        .ContainerFromItem(node) as TreeViewItem;
                    if (nodes.Count == 0)
                    {
                        treeViewItem.IsSelected = true;
                        treeViewItem.BringIntoView();
                        return;
                    }
                }
                node = nodes.Pop();
                treeViewItem.IsExpanded = true;
                if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    Expanded(true);
                }
                else
                {
                    //如果是懒加载，那么一般来说数据是还没有加载好的，那么就需要注册一个加载完成的事件
                    treeViewItem.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
                }
            }
            void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
            {
                if (treeViewItem.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    treeViewItem.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    Expanded(false);
                }
            }
        }
    }
}
