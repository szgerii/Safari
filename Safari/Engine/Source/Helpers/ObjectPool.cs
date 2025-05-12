using System;
using System.Collections.Concurrent;

namespace Engine.Helpers; 

/// <summary>
/// Basic class for pooling objects of a type
/// </summary>
/// <typeparam name="T">The type of the pooled objects</typeparam>
public class ObjectPool<T> where T : class {
	private readonly ConcurrentBag<T> pool = [];
	private readonly Func<T> generate;
	private readonly Action<T>? reset;

	/// <param name="generatorFn">Function that generates and initializes a new instance of type <typeparamref name="T"/></param>
	/// <param name="resetFn">Function for resetting an object before its return to the pool (leave on null if no resetting is necessary)</param>
	/// <param name="initialPoolSize">The number of objects to generate initially</param>
	public ObjectPool(Func<T> generatorFn, Action<T>? resetFn = null, int initialPoolSize = 0) {
		generate = generatorFn;
		reset = resetFn;

		for (int i = 0; i < initialPoolSize; i++) {
			pool.Add(generate());
		}
	}

	/// <summary>
	/// Retrieves and removes an object from the pool, or generates a new one if the pool is empty
	/// </summary>
	/// <returns>The newly generated or previously resetted object</returns>
	public T Borrow() => pool.TryTake(out T? pooled) ? pooled : generate();

	/// <summary>
	/// Resets and returns a borrowed object to the pool
	/// </summary>
	/// <param name="obj">The previously borrowed object to return</param>
	public void Return(T obj) {
		reset?.Invoke(obj);
		pool.Add(obj);
	}
}
