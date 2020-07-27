using ClassifyFiles.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ClassifyFiles.UI.Util
{
    /// <summary>
    /// 树状图选择帮助类。不支持虚拟化的树状图！
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class TreeViewSelectorHelper<TModel>
    {
        public TreeViewSelectorHelper(TreeView treeView,
            Func<TModel, TModel> getParent,
            Func<TModel, IList<TModel>> getSubItems,
             Action<TModel, TModel> setParent)
        {
            TreeView = treeView;
            GetParent = getParent;
            GetSubItems = getSubItems;
            SetParent = setParent;
        }

        public TreeView TreeView { get; }
        public Func<TModel, TModel> GetParent { get; }
        public Func<TModel, IList<TModel>> GetSubItems { get; }
        public Action<TModel, TModel> SetParent { get; }

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
        public async Task SelectItemWhileLoadedAsync(TModel node, IList<TModel> rootNodes)
        {
            while (!TreeView.IsLoaded)
            {
                await Task.Delay(1);
            }
            await SelectItemAsync(node, rootNodes);
        }

        public async Task<bool> ClearSelectionAsync(IList<TModel> rootNodes)
        {
            if (TreeView.SelectedItem == null)
            {
                return false;
            }
            var treeViewItem = await GetItemAsync((TModel)TreeView.SelectedItem, rootNodes); if (treeViewItem != null)
            {
                treeViewItem.IsSelected = false;
                return true;
            }
            return false;
        }

        public async Task<bool> SelectItemAsync(TModel node, IList<TModel> rootNodes)
        {
            var treeViewItem = await GetItemAsync(node, rootNodes);
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                treeViewItem.BringIntoView();
                return true;
            }
            return false;
        }

        public async Task<TreeViewItem> GetItemAsync(TModel node, IList<TModel> rootNodes)
        {
            Stack<TModel> nodes = new Stack<TModel>();
            //首先把结点从子到父放进一个站里，栈顶将是根节点
            while (!rootNodes.Contains(node))
            {
                nodes.Push(node);
                if (GetParent(node) == null)
                {
                    throw new Exception("树型数据有误");
                }
                node = GetParent(node);
            }
            ItemsControl treeViewItem = TreeView;

            while (true)
            {
                var a = treeViewItem.ItemContainerGenerator.Items.OfType<TreeViewItem>().FirstOrDefault(p => p.DataContext .Equals(node));
                treeViewItem = treeViewItem.ItemContainerGenerator
                       .ContainerFromItem(node) as TreeViewItem;
                if(treeViewItem==null)
                {
                    return null;
                }
                if (nodes.Count == 0)
                {
                    return treeViewItem as TreeViewItem;
                }
                node = nodes.Pop();
                treeViewItem.BringIntoView();
                (treeViewItem as TreeViewItem).IsExpanded = true;
                while (treeViewItem.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                {
                    await Task.Delay(1);
                }
            }

        }

        public async Task RemoveItemAsync(TModel node, IList<TModel> rootNodes)
        {
            if (rootNodes.Contains(node))
            {
                rootNodes.Remove(node);
                SetParent(node, default);
                //TreeView.UpdateLayout();
                //TreeView.Items.Refresh();
            }
            else
            {
                var items = GetSubItems(GetParent(node));
                items.Remove(node);
                SetParent(node, default);
                if (items.Count > 0)
                {
                    await SelectItemAsync(items[0], rootNodes);
                }
            }
        }
    }
}
