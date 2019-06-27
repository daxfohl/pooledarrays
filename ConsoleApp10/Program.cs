using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp10
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine($"GCSettings.IsServerGC = {System.Runtime.GCSettings.IsServerGC}");
			Console.WriteLine($"GCSettings.LatencyMode = {System.Runtime.GCSettings.LatencyMode}");
			Console.WriteLine($"GCSettings.LargeObjectHeapCompactionMode = {System.Runtime.GCSettings.LargeObjectHeapCompactionMode}");
			new Thread(() =>
			{
				var timer = new Stopwatch();
				while (true)
				{
					timer.Restart();
					Thread.Sleep(1);
					timer.Stop();
					// allow a little bit of leeway
					if (timer.ElapsedMilliseconds > 2)
					{
						// Record the pause
						Console.WriteLine($"Pause {timer.ElapsedMilliseconds}");
					}
				}
			}).Start();
			var random = new Random();
			var start = 0;
			var end = 256;
			var maxLen = 2000;
			var stringCache = new LRUCache<int, string>(20000);
			var bytesCache = new LRUCache<int, byte[]>(20000);
			//var bytes2Cache = new LRUCache<int, byte[]>(10000);
			for (var i = 0; i < 5; ++i)
			{
				new Thread(() =>
				{
					var threadCounter = 0;
					while (true)
					{
						var text = new string((char)random.Next(start, end + 1), random.Next(maxLen));
						stringCache.Set(text.GetHashCode(), text);

						var bytes = new byte[80 * 1024];
						random.NextBytes(bytes);
						bytesCache.Set(bytes.GetHashCode(), bytes);

						var bytes2 = new byte[90 * 1024];
						//random.NextBytes(bytes2);
						//bytes2Cache.Set(bytes2.GetHashCode(), bytes2);

						threadCounter++;
						Thread.Sleep(1); // So we don't thrash the CPU!!!!
					}
				}).Start();
			}

			Console.ReadKey();
		}
	}
	public class LRUCache<K, V>
	{
		private int capacity;
		private Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>> cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem<K, V>>>();
		private LinkedList<LRUCacheItem<K, V>> lruList = new LinkedList<LRUCacheItem<K, V>>();

		public LRUCache(int capacity)
		{
			this.capacity = capacity;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public V Get(K key)
		{
			LinkedListNode<LRUCacheItem<K, V>> node;
			if (cacheMap.TryGetValue(key, out node))
			{
				V value = node.Value.value;
				lruList.Remove(node);
				lruList.AddLast(node);
				return value;
			}
			return default(V);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Set(K key, V val)
		{
			if (cacheMap.Count >= capacity)
			{
				RemoveFirst();
			}

			LRUCacheItem<K, V> cacheItem = new LRUCacheItem<K, V>(key, val);
			LinkedListNode<LRUCacheItem<K, V>> node = new LinkedListNode<LRUCacheItem<K, V>>(cacheItem);
			lruList.AddLast(node);
			try
			{
				cacheMap.Add(key, node);
			}
			catch { }
		}

		private void RemoveFirst()
		{
			// Remove from LRUPriority
			LinkedListNode<LRUCacheItem<K, V>> node = lruList.First;
			lruList.RemoveFirst();

			// Remove from cache
			cacheMap.Remove(node.Value.key);
		}
	}

	class LRUCacheItem<K, V>
	{
		public LRUCacheItem(K k, V v)
		{
			key = k;
			value = v;
		}
		public K key;
		public V value;
	}
}
