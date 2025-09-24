using System;
using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.VisualTree;
using gip.ext.design.avui;

namespace LinqToVisualTree
{
  /// <summary>
  /// Adapts a AvaloniaObject to provide methods required for generate
  /// a Linq To Tree API
  /// </summary>
  public class VisualTreeAdapter : ILinqTree<AvaloniaObject>
  {
    private AvaloniaObject _item;

    public VisualTreeAdapter(AvaloniaObject item)
    {
      _item = item;
    }

    public IEnumerable<AvaloniaObject> Children()
    {
        if (_item is Visual vs)
            return vs.GetVisualChildren().OfType<AvaloniaObject>();
        return null;
    }

    public AvaloniaObject Parent
    {
      get
      {
        return VisualTreeHelper.GetParent(_item);
      }
    }
  }
}

namespace LinqToVisualTree
{
    /// <summary>
    /// Defines an interface that must be implemented to generate the LinqToTree methods
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILinqTree<T>
    {
        IEnumerable<T> Children();

        T Parent { get; }
    }

    public static class TreeExtensions
    {
        /// <summary>
        /// Returns a collection of descendant elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Descendants(this AvaloniaObject item)
        {
            ILinqTree<AvaloniaObject> adapter = new VisualTreeAdapter(item);
            foreach (var child in adapter.Children())
            {
                yield return child;

                foreach (var grandChild in child.Descendants())
                {
                    yield return grandChild;
                }
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> DescendantsAndSelf(this AvaloniaObject item)
        {
            yield return item;

            foreach (var child in item.Descendants())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of ancestor elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Ancestors(this AvaloniaObject item)
        {
            ILinqTree<AvaloniaObject> adapter = new VisualTreeAdapter(item);

            var parent = adapter.Parent;
            while (parent != null)
            {
                yield return parent;
                adapter = new VisualTreeAdapter(parent);
                parent = adapter.Parent;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> AncestorsAndSelf(this AvaloniaObject item)
        {
            yield return item;

            foreach (var ancestor in item.Ancestors())
            {
                yield return ancestor;
            }
        }

        /// <summary>
        /// Returns a collection of child elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Elements(this AvaloniaObject item)
        {
            ILinqTree<AvaloniaObject> adapter = new VisualTreeAdapter(item);
            foreach (var child in adapter.Children())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node, in document order.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsBeforeSelf(this AvaloniaObject item)
        {
            if (item.Ancestors().FirstOrDefault() == null)
                yield break;
            foreach (var child in item.Ancestors().First().Elements())
            {
                if (child.Equals(item))
                    break;
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of the after elements after this node, in document order.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsAfterSelf(this AvaloniaObject item)
        {
            if (item.Ancestors().FirstOrDefault() == null)
                yield break;
            bool afterSelf = false;
            foreach (var child in item.Ancestors().First().Elements())
            {
                if (afterSelf)
                    yield return child;

                if (child.Equals(item))
                    afterSelf = true;
            }
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsAndSelf(this AvaloniaObject item)
        {
            yield return item;

            foreach (var child in item.Elements())
            {
                yield return child;
            }
        }

        /// <summary>
        /// Returns a collection of descendant elements which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Descendants<T>(this AvaloniaObject item)
        {
            return item.Descendants().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection of the sibling elements before this node, in document order
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsBeforeSelf<T>(this AvaloniaObject item)
        {
            return item.ElementsBeforeSelf().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection of the after elements after this node, in document order
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsAfterSelf<T>(this AvaloniaObject item)
        {
            return item.ElementsAfterSelf().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> DescendantsAndSelf<T>(this AvaloniaObject item)
        {
            return item.DescendantsAndSelf().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection of ancestor elements which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Ancestors<T>(this AvaloniaObject item)
        {
            return item.Ancestors().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> AncestorsAndSelf<T>(this AvaloniaObject item)
        {
            return item.AncestorsAndSelf().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection of child elements which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Elements<T>(this AvaloniaObject item)
        {
            return item.Elements().Where(i => i is T).Cast<AvaloniaObject>();
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsAndSelf<T>(this AvaloniaObject item)
        {
            return item.ElementsAndSelf().Where(i => i is T).Cast<AvaloniaObject>();
        }

    }

    public static class EnumerableTreeExtensions
    {
        /// <summary>
        /// Applies the given function to each of the items in the supplied
        /// IEnumerable.
        /// </summary>
        private static IEnumerable<AvaloniaObject> DrillDown(this IEnumerable<AvaloniaObject> items,
            Func<AvaloniaObject, IEnumerable<AvaloniaObject>> function)
        {
            foreach (var item in items)
            {
                foreach (var itemChild in function(item))
                {
                    yield return itemChild;
                }
            }
        }

        /// <summary>
        /// Applies the given function to each of the items in the supplied
        /// IEnumerable, which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> DrillDown<T>(this IEnumerable<AvaloniaObject> items,
            Func<AvaloniaObject, IEnumerable<AvaloniaObject>> function)
            where T : AvaloniaObject
        {
            foreach (var item in items)
            {
                foreach (var itemChild in function(item))
                {
                    if (itemChild is T)
                    {
                        yield return (T)itemChild;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a collection of descendant elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Descendants(this IEnumerable<AvaloniaObject> items)
        {
            return items.DrillDown(i => i.Descendants());
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> DescendantsAndSelf(this IEnumerable<AvaloniaObject> items)
        {
            return items.DrillDown(i => i.DescendantsAndSelf());
        }

        /// <summary>
        /// Returns a collection of ancestor elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Ancestors(this IEnumerable<AvaloniaObject> items)
        {
            return items.DrillDown(i => i.Ancestors());
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> AncestorsAndSelf(this IEnumerable<AvaloniaObject> items)
        {
            return items.DrillDown(i => i.AncestorsAndSelf());
        }

        /// <summary>
        /// Returns a collection of child elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Elements(this IEnumerable<AvaloniaObject> items)
        {
            return items.DrillDown(i => i.Elements());
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsAndSelf(this IEnumerable<AvaloniaObject> items)
        {
            return items.DrillDown(i => i.ElementsAndSelf());
        }

        /// <summary>
        /// Returns a collection of descendant elements which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Descendants<T>(this IEnumerable<AvaloniaObject> items)
            where T : AvaloniaObject
        {
            return items.DrillDown<T>(i => i.Descendants());
        }

        /// <summary>
        /// Returns a collection containing this element and all descendant elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> DescendantsAndSelf<T>(this IEnumerable<AvaloniaObject> items)
            where T : AvaloniaObject
        {
            return items.DrillDown<T>(i => i.DescendantsAndSelf());
        }

        /// <summary>
        /// Returns a collection of ancestor elements which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Ancestors<T>(this IEnumerable<AvaloniaObject> items)
            where T : AvaloniaObject
        {
            return items.DrillDown<T>(i => i.Ancestors());
        }

        /// <summary>
        /// Returns a collection containing this element and all ancestor elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> AncestorsAndSelf<T>(this IEnumerable<AvaloniaObject> items)
            where T : AvaloniaObject
        {
            return items.DrillDown<T>(i => i.AncestorsAndSelf());
        }

        /// <summary>
        /// Returns a collection of child elements which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> Elements<T>(this IEnumerable<AvaloniaObject> items)
            where T : AvaloniaObject
        {
            return items.DrillDown<T>(i => i.Elements());
        }

        /// <summary>
        /// Returns a collection containing this element and all child elements.
        /// which match the given type.
        /// </summary>
        public static IEnumerable<AvaloniaObject> ElementsAndSelf<T>(this IEnumerable<AvaloniaObject> items)
            where T : AvaloniaObject
        {
            return items.DrillDown<T>(i => i.ElementsAndSelf());
        }
    }
}
