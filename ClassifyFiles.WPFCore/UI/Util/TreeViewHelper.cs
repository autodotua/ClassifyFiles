using ClassifyFiles.UI.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        public async Task SelectItemWhileLoadedAsync(TModel node, IList<TModel> rootNodes)
        {
            while (!TreeView.IsLoaded)
            {
                await Task.Delay(1);
            }
            await SelectItemAsync(node, rootNodes);
        }

        public async Task SelectItemAsync(TModel node, IList<TModel> rootNodes)
        {
            var treeViewItem = await GetItemAsync(node, rootNodes);
            treeViewItem.IsSelected = true;
            treeViewItem.BringIntoView();
            return;
        }

        public async Task<TreeViewItem> GetItemAsync(TModel node, IList<TModel> rootNodes)
        {
            Stack<TModel> nodes = new Stack<TModel>();
            //首先把结点从子到父放进一个站里，栈顶将是根节点
            while (!rootNodes.Contains(node))
            {
                nodes.Push(node);
                if(GetParent(node)==null)
                {
                    throw new Exception("树型数据有误");
                }
                node = GetParent(node);
            }
            ItemsControl treeViewItem = TreeView;

            while (true)
            {
                treeViewItem = treeViewItem.ItemContainerGenerator
                       .ContainerFromItem(node) as TreeViewItem;
                if (nodes.Count == 0)
                {
                    return treeViewItem as TreeViewItem;
                }
                node = nodes.Pop();
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
            }
            else
            {
                //树状图只能删除一条，之后的虽然从数据库中删除了，但是不会从视图中删除
                //目前不知道原因
                var pItem =await GetItemAsync(GetParent(node), rootNodes);
                var items = GetSubItems(GetParent(node));
                items.Remove(node);
                //所以目前只能够重新绑定进行刷新视图，投机取巧我喜欢
                pItem.ItemsSource = items;
                if (items.Count>0)
                {
                    await SelectItemAsync(items[0], rootNodes);
                }
            }
        }
    }
}
