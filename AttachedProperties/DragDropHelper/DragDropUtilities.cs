using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapEditor.AttachedProperties
{
    public static class DragDropUtilities
	{
		// Finds the orientation of the panel of the ItemsControl that contains the itemContainer passed as a parameter.
		// The orientation is needed to figure out where to draw the adorner that indicates where the item will be dropped.
		public static Orientation GetOrientation(DependencyObject itemContainer, Orientation defaultOrientation)
		{
            if (itemContainer == null)
            {
                return defaultOrientation;
            }
			var parent = VisualTreeHelper.GetParent(itemContainer);
            if (parent == null)
            {
                return defaultOrientation;
            }
            var orientationProperty = GetDependencyPropertyByName(parent.GetType(), "OrientationProperty");
            if (orientationProperty == null)
            {
                return defaultOrientation;
            }
            var orientation = parent.GetValue(orientationProperty);
            if (orientation == null || orientation == DependencyProperty.UnsetValue)
            {
                return defaultOrientation;
            }
            return (Orientation)orientation;
		}

        public static DependencyProperty GetDependencyPropertyByName(Type dependencyObjectType, string dpName)
        {
            var fieldInfo = dependencyObjectType.GetField(dpName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return (fieldInfo != null ? fieldInfo.GetValue(null) : null) as DependencyProperty;
        }

        public static void InsertItemIntoItemsControl(ItemsControl itemsControl, object itemToInsert, int insertionIndex)
		{
            if (itemToInsert == null)
            {
                return;
            }
			var itemsSource = itemsControl.ItemsSource;
			if (itemsSource == null)
			{
                // Not bound, so add directly to items
				itemsControl.Items.Insert(insertionIndex, itemToInsert);
                return;
			}
            var itemsSourceIList = itemsSource as IList;
            if (itemsSourceIList != null)
            {
                // Supports non-generic IList (most things do)
                itemsSourceIList.Insert(insertionIndex, itemToInsert);
                return;
            }
			var type = itemsSource.GetType();
			if (type.GetInterface("IList`1") != null)
			{
                // Some unknown generic which only supports IList<T>
				type.GetMethod("Insert").Invoke(itemsSource, new object[] { insertionIndex, itemToInsert });
			}
		}

		public static int RemoveItemFromItemsControl(ItemsControl itemsControl, object itemToRemove)
		{
			if (itemToRemove == null)
            {
                return -1;
            }
	        var indexToBeRemoved = itemsControl.Items.IndexOf(itemToRemove);
            if (indexToBeRemoved == -1)
            {
                return indexToBeRemoved;
            }
			var itemsSource = itemsControl.ItemsSource;
			if (itemsSource == null)
			{
                // Not bound, so remove directly from items
                itemsControl.Items.RemoveAt(indexToBeRemoved);
                return indexToBeRemoved;
			}
            var itemsSourceIList = itemsSource as IList;
            if (itemsSourceIList != null)
			{
                // Supports non-generic IList (most things do)
                itemsSourceIList.RemoveAt(indexToBeRemoved);
                return indexToBeRemoved;
			}
            var type = itemsSource.GetType();
            if (type.GetInterface("IList`1") != null)
            {
                // Some unknown generic which only supports IList<T>
                type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { indexToBeRemoved });
                return indexToBeRemoved;
            }
            return -1;
		}

		public static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, Orientation orientation)
		{
			 return orientation == Orientation.Vertical ?
				clickedPoint.Y < container.ActualHeight / 2 :
			    clickedPoint.X < container.ActualWidth / 2;
		}

		public static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
		{
			return 
                Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
			    Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
		}

        public static FrameworkElement FindParent(DependencyObject obj, string name)
        {
            if (obj == null)
            {
                return null;
            }
            // Cope with breaks in the logical tree (content controls) by
            // resorting to the visual tree
            var parent = LogicalTreeHelper.GetParent(obj);
            if (parent == null)
            {
                parent = VisualTreeHelper.GetParent(obj);
            }
            var namedParent = parent as FrameworkElement;
            if (namedParent != null && namedParent.Name == name)
            {
                return namedParent;
            }
            return FindParent(parent, name);
        }
    }
}
