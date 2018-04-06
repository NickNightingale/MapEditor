using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using MapEditor.ExtensionMethods;

namespace System.Collections.ObjectModel
{
	/// <summary>
	/// A copy of ObservableCollection<T> with added AddRange method
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Serializable]
	public class ObservableCollection2<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
	{
		// Fields
		private SimpleMonitor _monitor;
		private const string CountString = "Count";
		private const string IndexerName = "Item[]";

		// Events
		[field: NonSerialized]
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		[field: NonSerialized]
		protected event PropertyChangedEventHandler PropertyChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add
			{
				PropertyChanged += value;
			}
			remove
			{
				PropertyChanged -= value;
			}
		}

		// Methods
		public ObservableCollection2()
		{
			_monitor = new SimpleMonitor();
		}

		public ObservableCollection2(IEnumerable<T> collection)
		{
			_monitor = new SimpleMonitor();
			if (collection == null)
			{
				throw new ArgumentNullException("collection");
			}
			CopyFrom(collection);
		}

		public ObservableCollection2(List<T> list)
		{
			_monitor = new SimpleMonitor();
			CopyFrom(list);
		}

		protected IDisposable BlockReentrancy()
		{
			this._monitor.Enter();
			return this._monitor;
		}

		protected void CheckReentrancy()
		{
			if ((_monitor.Busy && (CollectionChanged != null)) && (CollectionChanged.GetInvocationList().Length > 1))
			{
				throw new InvalidOperationException("ObservableCollection2ReentrancyNotAllowed");
			}
		}

		protected override void ClearItems()
		{
			CheckReentrancy();
			base.ClearItems();
			if (!_updating)
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionReset();
			}
		}

		private void CopyFrom(IEnumerable<T> collection)
		{
			if (collection != null)
			{
				AddRange(collection);
			}
		}

		protected override void InsertItem(int index, T item)
		{
			CheckReentrancy();
			base.InsertItem(index, item);
			if (!_updating)
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
			}
		}

		public void Move(int oldIndex, int newIndex)
		{
			MoveItem(oldIndex, newIndex);
		}

		protected virtual void MoveItem(int oldIndex, int newIndex)
		{
			CheckReentrancy();
			T item = base[oldIndex];
			base.RemoveItem(oldIndex);
			base.InsertItem(newIndex, item);
			if (!_updating)
			{
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex);
			}
		}

		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				using (BlockReentrancy())
				{
					CollectionChanged(this, e);
				}
			}
		}

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
		}

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
		}

		private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
		}

		private void OnCollectionReset()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, e);
			}
		}

		private void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		protected override void RemoveItem(int index)
		{
			CheckReentrancy();
			T item = base[index];
			base.RemoveItem(index);
			if (!_updating)
			{
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
			}
		}

		protected override void SetItem(int index, T item)
		{
			CheckReentrancy();
			T oldItem = base[index];
			base.SetItem(index, item);
			if (!_updating)
			{
				OnPropertyChanged(IndexerName);
				OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
			}
		}

		#region Additional funtionality

		bool _updating;

		public virtual void AddRange(IEnumerable<T> collection)
		{
			if (collection.IsNullOrEmpty())
			{
				return;
			}
			CheckReentrancy();
			// Materialize the enumerable so that adding to this doesn't cause the list
			// for OnCollectionChangedMultiple to be enumerated again with potentially different results
			var list = collection.ToList();
			int count = Count;
			try
			{
				_updating = true;
				foreach (T item in list)
				{
					// Can't call base.InsertItem because this will not work
					// when InsertItem is overridden (e.g. ObservableContentCollection)
					InsertItem(Count, item);
				}
			}
			finally
			{
				_updating = false;
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChangedMultiple(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list, count));
			}
		}


		public virtual void RemoveRange(IEnumerable<T> collection)
		{
			if (collection.IsNullOrEmpty())
			{
				return;
			}
			// Materialize the enumerable so that removing from this doesn't cause the list
			// for OnCollectionChangedMultiple to be enumerated again with potentially different results
			var list = collection.ToList();
			try
			{
				_updating = true;
				foreach (T item in list)
				{
					Remove(item);
				}
			}
			finally
			{
				_updating = false;
				OnPropertyChanged(CountString);
				OnPropertyChanged(IndexerName);
				OnCollectionChangedMultiple(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, list, Count));
			}
		}


		protected virtual void OnCollectionChangedMultiple(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				foreach (NotifyCollectionChangedEventHandler handler in CollectionChanged.GetInvocationList())
				{
					if (handler.Target is ICollectionView)
					{
						((ICollectionView)handler.Target).Refresh();

					}
					else
					{
						handler(this, e);
					}
				}
			}
		}

		/// <summary> 
		/// Sorts the items in the collection using the provided key selector. 
		/// </summary> 
		/// <typeparam name="TKey">Key type returned by the key selector.</typeparam> 
		/// <param name="selector">Function to retrieve the key from an item.</param> 
		public void Sort<TKey>(Func<T, TKey> selector)
		{
			Reset(this.OrderBy(selector));
		}

		/// <summary> 
		/// Sorts the items in the collection using the provided key selector. 
		/// </summary> 
		/// <typeparam name="TKey">Key type returned by the key selector.</typeparam> 
		/// <param name="selector">Function to retrieve the key from an item.</param> 
		/// <param name="comparer">A <see cref="IComparer{T}"/> to compare keys.</param> 
		public void Sort<TKey>(Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			Reset(this.OrderBy(selector, comparer));
		}

		/// <summary> 
		/// Sorts the items in the collection using the provided key selector. 
		/// </summary> 
		/// <typeparam name="TKey">Key type returned by the key selector.</typeparam> 
		/// <param name="selector">Function to retrieve the key from an item.</param> 
		public void SortDescending<TKey>(Func<T, TKey> selector)
		{
			Reset(this.OrderByDescending(selector));
		}

		/// <summary> 
		/// Sorts the items in the collection using the provided key selector. 
		/// </summary> 
		/// <typeparam name="TKey">Key type returned by the key selector.</typeparam> 
		/// <param name="selector">Function to retrieve the key from an item.</param> 
		/// <param name="comparer">A <see cref="IComparer{T}"/> to compare keys.</param> 
		public void SortDescending<TKey>(Func<T, TKey> selector, IComparer<TKey> comparer)
		{
			Reset(this.OrderByDescending(selector, comparer));
		}

		/// <summary> 
		/// Moves items in the inner collection to match the positions of the items provided. 
		/// </summary> 
		/// <param name="items"> 
		/// A <see cref="IEnumerable{T}"/> to provide the positions of the items. 
		/// </param> 
		public void Reset(IEnumerable<T> items)
		{
			// Can't call base.InsertItem or base.ClearItems because the might have been overridden
			try
			{
				_updating = true;
				var collection = items.ToArray();
				ClearItems();
				foreach (var item in collection)
				{
					InsertItem(Count, item);
				}
			}
			finally
			{
				_updating = false;
				OnPropertyChanged(IndexerName);
				// Have to reset this as a whole list reset as Move only deals with one item or block of items moving from one place to another
				OnCollectionChangedMultiple(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		#endregion

		// Nested Types
		[Serializable]
		private class SimpleMonitor : IDisposable
		{
			// Fields
			private int _busyCount;

			// Methods
			public void Dispose()
			{
				_busyCount--;
			}

			public void Enter()
			{
				_busyCount++;
			}

			// Properties
			public bool Busy
			{
				get
				{
					return (_busyCount > 0);
				}
			}
		}
	}
}

