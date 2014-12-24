using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IanUtility
{
    
    public class SortedBindingList<T> : BindingList<T>
    {
        //TEST
        bool Sorted = false;
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemMoved)
            {
                if (Sorted)
                {
                    T o = Items[e.NewIndex];
                    if (Sort(e.NewIndex, o))
                    {
                        //if(e.ListChangedType== ListChangedType.ItemAdded) 
                        //    base.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Items.IndexOf(o)));
                        //else if (e.ListChangedType == ListChangedType.ItemMoved) 
                        //    base.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, Items.IndexOf(o), e.OldIndex ));

                        base.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
                        return;
                    }
                }
            }
            
            base.OnListChanged(e);
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            Sorted = true;
            dir = direction;
            Sort(-1, default(T));
            if(Items.Count>0) base.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        protected override bool IsSortedCore
        {
            get
            {
                return Sorted;
            }
        }
        protected override void RemoveSortCore()
        {
            Sorted = false;
        }
        ListSortDirection dir = ListSortDirection.Ascending;
        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return dir;
            }
        }
        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return base.SortPropertyCore;
            }
        }
        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        bool Sort(int idx, T lastAdded)
        {
            List<T> items = Items as List<T>;
            RaiseListChangedEvents = false;
            items.Sort(CompareFunction);
            RaiseListChangedEvents = true;
            if (lastAdded != null)
            {
                if (idx == -1 && items[items.Count - 1].Equals(lastAdded)) return false;
                else if (idx != -1 && items.Count > idx && items[idx].Equals(lastAdded)) return false;
            }
            return true;
        }
        int Order(int i)
        {
            int ret = 0;
            if (SortDirectionCore == ListSortDirection.Ascending) ret = i;
            else ret = i*-1;

            return ret;
        }

        protected virtual int CompareFunction(T a, T b)
        {
            if (a is IComparable<T> && b is IComparable<T>)
            {
                IComparable<T> aa = a as IComparable<T>;
                return aa.CompareTo(b);
            }
            if (a is IComparer<T> && b is IComparer<T>)
            {
                IComparer<T> aa = a as IComparer<T>;
                return aa.Compare(a, b);
            }
            
            return Order(String.Compare(a.ToString(), b.ToString()));
        }
    }
    
}
