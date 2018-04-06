using MapEditor.Utilities;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MapEditor.AttachedProperties
{
    public class DragDropHelper : Singleton<DragDropHelper>
	{
		// Source and Target
		private DataFormat _format = DataFormats.GetDataFormat("DragDropItemsControl");
        // Remember our mouse down to start dragging
		private Point _initialMousePosition;
        // Caches the data clicked until we have dragged it far enough
		private object _draggedData;
        // Displays a representation of the dragged data (using template)
		private DraggedAdorner _draggedAdorner;
        // Displays a line where the data will be inserted
		private InsertionAdorner _insertionAdorner;

		// *** Source *** //
        // The whole source items control
		private ItemsControl _sourceItemsControl;
        // The individual container for the source item under the cursor
        private FrameworkElement _sourceItemContainer;

        // *** Target (might be the same as the source) *** //
        // The whole target items control
        private ItemsControl _targetItemsControl;
        // The individual container for the target item under the cursor
        private FrameworkElement _targetItemContainer;
        // Where (in the list) to add the dropped item
		private int _targetInsertionIndex;
        // The orientation of the control under the cursor
        private Orientation _targetOrientation;
		// Where we are in the _TargetItemContainer
		private bool _isInFirstHalf;

        public static readonly DependencyProperty IsDragSourceProperty =
            DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDragSourceChanged));

        public static bool GetIsDragSource(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDragSourceProperty);
		}

		public static void SetIsDragSource(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDragSourceProperty, value);
		}

        private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dragSource = obj as ItemsControl;
            if (dragSource != null)
            {
                if (Equals(e.NewValue, true))
                {
                    dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
                }
                else
                {
                    dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
                }
            }
        }

        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDropTargetChanged));

        public static bool GetIsDropTarget(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDropTargetProperty);
		}

		public static void SetIsDropTarget(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDropTargetProperty, value);
		}

        private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dropTarget = obj as ItemsControl;
            if (dropTarget != null)
            {
                if (Equals(e.NewValue, true))
                {
                    // Don't use preview events because we want "Bubbling" behaviour
                    // to cope with items lists inside items lists
                    dropTarget.AllowDrop = true;
                    // No events if the target has a null background
                    if (dropTarget.Background == null)
                    {
                        dropTarget.Background = Brushes.Transparent;
                    }
                    dropTarget.Drop += Instance.DropTarget_Drop;
                    dropTarget.DragEnter += Instance.DropTarget_DragEnter;
                    dropTarget.DragOver += Instance.DropTarget_DragOver;
                    dropTarget.DragLeave += Instance.DropTarget_DragLeave;
                }
                else
                {
                    dropTarget.AllowDrop = false;
                    dropTarget.Drop -= Instance.DropTarget_Drop;
                    dropTarget.DragEnter -= Instance.DropTarget_DragEnter;
                    dropTarget.DragOver -= Instance.DropTarget_DragOver;
                    dropTarget.DragLeave -= Instance.DropTarget_DragLeave;
                }
            }
        }

        public static readonly DependencyProperty DragDropTemplateProperty =
            DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(DragDropHelper), new UIPropertyMetadata(null));

        public static DataTemplate GetDragDropTemplate(DependencyObject obj)
		{
			return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
		}

		public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
		{
			obj.SetValue(DragDropTemplateProperty, value);
		}

        public static readonly DependencyProperty DragParentProperty =
            DependencyProperty.RegisterAttached("DragParent", typeof(UIElement), typeof(DragDropHelper), new UIPropertyMetadata(null));

        public static UIElement GetDragParent(DependencyObject obj)
        {
            return (UIElement)obj.GetValue(DragParentProperty);
        }

        public static void SetDragParent(DependencyObject obj, UIElement value)
        {
            obj.SetValue(DragParentProperty, value);
        }

        public static readonly DependencyProperty DropSelfOnlyProperty =
            DependencyProperty.RegisterAttached("DropSelfOnly", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false));

        public static bool GetDropSelfOnly(DependencyObject obj)
        {
            return (bool)obj.GetValue(DropSelfOnlyProperty);
        }

        public static void SetDropSelfOnly(DependencyObject obj, bool value)
        {
            obj.SetValue(DropSelfOnlyProperty, value);
        }

        public static readonly DependencyProperty CanDropSelfProperty =
            DependencyProperty.RegisterAttached("CanDropSelf", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(true));

        public static bool GetCanDropSelf(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanDropSelfProperty);
        }

        public static void SetCanDropSelf(DependencyObject obj, bool value)
        {
            obj.SetValue(CanDropSelfProperty, value);
        }

		public static readonly DependencyProperty DefaultOrientationProperty =
			DependencyProperty.RegisterAttached("DefaultOrientation", typeof(Orientation), typeof(DragDropHelper), new UIPropertyMetadata(Orientation.Vertical));

		public static Orientation GetDefaultOrientation(DependencyObject obj)
		{
			return (Orientation)obj.GetValue(DefaultOrientationProperty);
		}

		public static void SetDefaultOrientation(DependencyObject obj, Orientation value)
		{
			obj.SetValue(DefaultOrientationProperty, value);
		}

		// DragSource

		private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
            _sourceItemsControl = (ItemsControl)sender;
            _sourceItemContainer = _sourceItemsControl.ContainerFromElement(e.OriginalSource as DependencyObject) as FrameworkElement;
            _draggedData = _sourceItemContainer != null ? _sourceItemContainer.DataContext : null;
            _initialMousePosition = e.GetPosition(_sourceItemsControl);
        }

        // Drag = mouse down + move by a certain amount
        private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
		{
            if (_draggedData == null)
            {
                return;
            }
            // Only drag when user moved the mouse by a reasonable amount.
            if (!DragDropUtilities.IsMovementBigEnough(_initialMousePosition, e.GetPosition(_sourceItemsControl)))
            {
                return;
            }
            // Definately was our event
            e.Handled = true;
            // Get the data ready to be dropped
            var data = new DataObject(_format.Name, _draggedData);
            // From now on, get the data from the DataObject
            _draggedData = null;

            // Adding events to the parent to make sure the dragged adorner comes up when mouse is not over a drop target
            var parent = GetDragParent(_sourceItemsControl);
            DependencyProperty backgroundProperty = null;
            bool setBackground = false;
            bool previousAllowDrop = false;

            if (parent != null)
            {
                backgroundProperty = DragDropUtilities.GetDependencyPropertyByName(parent.GetType(), "BackgroundProperty");
                // If the parent does not draw a background (like a grid by default) then you will not get drag events
                // Unfortunately Background is defined in several classes e.g. Control, Panel not a single base class
                // so use reflection to get and set the current value
                if (backgroundProperty != null)
                {
                    var background = parent.GetValue(backgroundProperty) as Brush;
                    if (background == null)
                    {
                        parent.SetValue(backgroundProperty, Brushes.Transparent);
                        setBackground = true;
                    }
                    previousAllowDrop = parent.AllowDrop;
                    parent.AllowDrop = true;
                    parent.DragEnter += Parent_DragEnter;
                    parent.DragOver += Parent_DragOver;
                    parent.DragLeave += Parent_DragLeave;
                }
            }


            var effects = DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);

			// Without this call, there would be a bug in the following scenario: Click on a data item, and drag
			// the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
			// the Window leave event, and the dragged adorner is left behind.
			// With this call, the dragged adorner will disappear when we release the mouse outside of the window,
			// which is when the DoDragDrop synchronous method returns.
			RemoveDraggedAdorner();

            if (parent != null)
            {
                if (backgroundProperty != null)
                {
                    if (setBackground)
                    {
                        parent.ClearValue(backgroundProperty);
                        setBackground = false;
                    }
                    parent.AllowDrop = previousAllowDrop;
                    parent.DragEnter -= Parent_DragEnter;
                    parent.DragOver -= Parent_DragOver;
                    parent.DragLeave -= Parent_DragLeave;
                }
            }
		}

        private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Effectively "releases" the mouse capture if we mouse up without dragging
            _draggedData = null;
        }

        // DropTarget

        private void DropTarget_DragEnter(object sender, DragEventArgs e)
		{
            e.Handled = true;
            _targetItemsControl = (ItemsControl)sender;
            var draggedItem = e.Data.GetData(_format.Name);
            DecideDropTarget(e, draggedItem);
            if (draggedItem == null)
            {
                return;
            }
            ShowDraggedAdorner(e.GetPosition(_sourceItemsControl), draggedItem);
			CreateInsertionAdorner();
		}

		private void DropTarget_DragOver(object sender, DragEventArgs e)
		{
            e.Handled = true;
            var draggedItem = e.Data.GetData(_format.Name);
            DecideDropTarget(e, draggedItem);
            if (draggedItem == null)
            {
                return;
            }
            ShowDraggedAdorner(e.GetPosition(_sourceItemsControl), draggedItem);
            UpdateInsertionAdornerRelativePosition();
		}


        private void DropTarget_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
            var draggedItem = e.Data.GetData(_format.Name);
            DecideDropTarget(e, draggedItem);
            if (draggedItem == null)
            {
                return;
            }
            // We have to remove the dragged adorner to cope with there being no
            // parent border around the target (or no parent defined) otherwise the
            // adorner gets left behind
            RemoveDraggedAdorner();
            RemoveInsertionAdorner();
        }

        private void DropTarget_Drop(object sender, DragEventArgs e)
		{
            e.Handled = true;
            var draggedItem = e.Data.GetData(_format.Name);
            if (draggedItem == null)
            {
                return;
            }
            var indexRemoved = -1;
            if ((e.Effects & DragDropEffects.Move) != 0)
			{
				indexRemoved = DragDropUtilities.RemoveItemFromItemsControl(_sourceItemsControl, draggedItem);
			}
			// This happens when we drag an item to a later position within the same ItemsControl.
			if (indexRemoved != -1 && _sourceItemsControl == _targetItemsControl && indexRemoved < _targetInsertionIndex)
			{
                _targetInsertionIndex--;
			}
            DragDropUtilities.InsertItemIntoItemsControl(_targetItemsControl, draggedItem, _targetInsertionIndex);
			RemoveDraggedAdorner();
			RemoveInsertionAdorner();
		}


		// If the types of the dragged data and ItemsControl's source are compatible, 
		// there are 3 situations to have into account when deciding the drop target:
		// 1. mouse is over an items container
		// 2. mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
		// 3. mouse is over an empty ItemsControl.
		// The goal of this method is to decide on the values of the following properties: 
		// targetItemContainer, insertionIndex and isInFirstHalf.
		private void DecideDropTarget(DragEventArgs e, object draggedItem)
		{
            _isInFirstHalf = false;
            _targetItemContainer = null;
            _targetOrientation = Orientation.Vertical;
            _targetInsertionIndex = -1;
            if (!IsDropDataAllowed(draggedItem))
            {
                // Mismatched type
                e.Effects = DragDropEffects.None;
                return;
            }
			var targetItemsControlCount = _targetItemsControl.Items.Count;
			if (targetItemsControlCount == 0)
            {
                // Over an empty items control, insert at the beginning
                _targetInsertionIndex = 0;
                return;
            }
            // Find the items panel
            _targetOrientation = DragDropUtilities.GetOrientation(
				_targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0),
				GetDefaultOrientation(_targetItemsControl));
            // This is the element we are pointing to (as long as it has a background)
            _targetItemContainer = _targetItemsControl.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;
			if (_targetItemContainer != null)
			{
				var positionRelativeToItemContainer = e.GetPosition(_targetItemContainer);
                _isInFirstHalf = DragDropUtilities.IsInFirstHalf(_targetItemContainer, positionRelativeToItemContainer, _targetOrientation);
                _targetInsertionIndex = _targetItemsControl.ItemContainerGenerator.IndexFromContainer(_targetItemContainer);
				if (!_isInFirstHalf)
				{
                    // Insert after the target if more than half way through
                    _targetInsertionIndex++;
				}
                return;
			}
            // Fallback to the last item in the container (always show at bottom)
            _targetItemContainer = _targetItemsControl.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
            _targetInsertionIndex = targetItemsControlCount;
		}

		// Can the dragged data be added to the destination collection?
		// It can if destination is bound to IList<allowed type>, IList or not data bound.
		private bool IsDropDataAllowed(object draggedItem)
		{
            if (draggedItem == null)
            {
                // Nothing to drop
                return false;
            }
            if (GetDropSelfOnly(_targetItemsControl) && _targetItemsControl != _sourceItemsControl)
            {
                // Only allowed to drop onto ourself
                return false;
            }
            if (!GetCanDropSelf(_targetItemsControl) && _targetItemsControl == _sourceItemsControl)
            {
                // Only allowed to drop onto ourself
                return false;
            }
            var collectionSource = _targetItemsControl.ItemsSource;
            if (collectionSource == null)
            {
                // Unbound (just gui items) always allow
                return true;
            }
			var collectionType = collectionSource.GetType();
			var genericIListType = collectionType.GetInterface("IList`1");
			if (genericIListType != null)
			{
                // If it is a generic list, check the template type
                var draggedType = draggedItem.GetType();
                var genericArguments = genericIListType.GetGenericArguments();
				return genericArguments[0].IsAssignableFrom(draggedType);
			}
            // Use the non-generic interface (can't check item type)
            return (typeof(IList).IsAssignableFrom(collectionType));
		}

		// Parent (adorner visible moving between itemscontrols)

		private void Parent_DragEnter(object sender, DragEventArgs e)
		{
            e.Handled = true;
			ShowDraggedAdorner(e.GetPosition(_sourceItemsControl), e.Data.GetData(_format.Name));
			e.Effects = DragDropEffects.None;
		}

		private void Parent_DragOver(object sender, DragEventArgs e)
		{
            e.Handled = true;
            ShowDraggedAdorner(e.GetPosition(_sourceItemsControl), e.Data.GetData(_format.Name));
			e.Effects = DragDropEffects.None;
		}

		private void Parent_DragLeave(object sender, DragEventArgs e)
		{
            e.Handled = true;
            RemoveDraggedAdorner();
		}

		// Adorners

		// Creates or updates the dragged Adorner. 
		private void ShowDraggedAdorner(Point currentPosition, object draggedData)
		{
			if (_draggedAdorner == null)
			{
                _draggedAdorner = new DraggedAdorner(
                    draggedData, 
                    GetDragDropTemplate(_sourceItemsControl),
                    _sourceItemsControl,
                    AdornerLayer.GetAdornerLayer(_sourceItemsControl));
			}
            _draggedAdorner.SetPosition(currentPosition);
		}

		private void RemoveDraggedAdorner()
		{
			if (_draggedAdorner != null)
			{
                _draggedAdorner.Detach();
                _draggedAdorner = null;
			}
		}

		private void CreateInsertionAdorner()
		{
			if (_targetItemContainer != null)
			{
                // Here, I need to get adorner layer from targetItemContainer and not targetItemsControl. 
				// This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
                // If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
                _insertionAdorner = new InsertionAdorner(
                    // If the target is vertical, the adorner should be horizontal
                    _targetOrientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal, 
                    _isInFirstHalf ? RelativePosition.Before : RelativePosition.After, 
                    _targetItemContainer,
                    AdornerLayer.GetAdornerLayer(_targetItemContainer));
            }
        }

		private void UpdateInsertionAdornerRelativePosition()
		{
			if (_insertionAdorner != null)
			{
                _insertionAdorner.RelativePosition = _isInFirstHalf ? RelativePosition.Before : RelativePosition.After;
			}
		}

		private void RemoveInsertionAdorner()
		{
			if (_insertionAdorner != null)
			{
                _insertionAdorner.Detach();
                _insertionAdorner = null;
			}
		}
	}
}
