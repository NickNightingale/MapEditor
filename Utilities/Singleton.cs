namespace MapEditor.Utilities
{
	public abstract class Singleton<T> where T : class, new()
	{
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new T();
				}
				return _instance;
			}
		}
	}

	public abstract class LockedSingleton<T> where T : class, new()
	{
		static object _instanceLock = new object();
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_instanceLock)
					{
						if (_instance == null)
						{
							_instance = new T();
						}
					}
				}
				return _instance;
			}
		}
	}
}
