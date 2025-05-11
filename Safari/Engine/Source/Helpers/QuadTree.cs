using Engine.Graphics.Stubs.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Engine.Helpers;

/// <summary>
/// Class for the spatial hashing of objects with AABB components
/// </summary>
/// <typeparam name="T">The type of the <see cref="ISpatial"></see> object to store</typeparam>
public class QuadTree<T> where T : class, ISpatial {
	/// <summary>
	/// Upper bound for how far the quadtree can deepen
	/// </summary>
	public int MaxDepth { get; init; }
	/// <summary>
	/// The number of elements that can be concurrently stored in a single node before it tries to split
	/// </summary>
	public int Threshold { get; init; }

	private Vectangle _bounds;
	private Vector2 center;
	/// <summary>
	/// The bounds of the quadtree (node)
	/// </summary>
	public Vectangle Bounds {
		get => _bounds;
		private set {
			_bounds = value;
			center = _bounds.Location + (_bounds.Size / 2);
		}
	}

	/// <summary>
	/// How deep the quadtree (node) is from the root
	/// </summary>
	public int Level { get; private set; } = 0;

	/// <summary>
	/// True if the quadtree (node) doesn't have any child quadrants
	/// </summary>
	[MemberNotNullWhen(false, nameof(nw), nameof(ne), nameof(sw), nameof(se))]
	public bool IsLeaf => nw == null;

	/// <summary>
	/// The number used for initalizing list capacity for <see cref="CalculateCollisions(T, Func{T, T, bool})"></see>
	/// </summary>
	public int ExpectedCollisionCount { get; set; } = 50;

	private readonly List<T> elements = [];
	private QuadTree<T>? nw, ne, sw, se;
	private int subElementCount = 0;
	
	private static readonly ObjectPool<Queue<QuadTree<T>>> queuePool = new(() => new(32), queue => queue.Clear(), 10);
	private static readonly ObjectPool<Stack<QuadTree<T>>> stackPool = new(() => new(32), stack => stack.Clear(), 30);
	private ITexture2D? boundsTex;

	/// <param name="bounds">The bounds the quadtree (node) can store AABBs in</param>
	public QuadTree(Vectangle bounds) {
		Bounds = bounds;
	}

	/// <param name="bounds">The bounds the quadtree (node) can store AABBs in</param>
	/// <param name="threshold">Value for <see cref="Threshold"></see></param>
	/// <param name="maxDepth">Value for <see cref="MaxDepth"></see></param>
	/// <param name="level">Value for <see cref="Level"></see></param>
	public QuadTree(Vectangle bounds, int threshold, int maxDepth, int level = 0) {
		Bounds = bounds;
		Threshold = threshold;
		MaxDepth = maxDepth;
		Level = level;
	}

	/// <summary>
	/// Inserts an element into the quadtree and splits the quadtree if necessary
	/// </summary>
	/// <param name="element">The element to insert</param>
	public void Insert(T element) {
		if (element.Bounds.Size == Vector2.Zero) return;

		if (IsLeaf && subElementCount >= Threshold) {
			Split();
		}

		QuadTree<T>? containingChild = GetContainingChild(element);
		subElementCount++;
		if (containingChild == null) {
			elements.Add(element);
		} else {
			containingChild.Insert(element);
		}
	}

	/// <summary>
	/// Inserts an element into the quadtree without trying to split
	/// </summary>
	/// <param name="element">The element to insert</param>
	private void InsertInternal(T element) {
		if (element.Bounds.Size == Vector2.Zero) return;

		QuadTree<T>? containingChild = GetContainingChild(element);
		subElementCount++;

		if (containingChild == null) {
			elements.Add(element);
		} else {
			containingChild.InsertInternal(element);
		}
	}

	/// <summary>
	/// Removes an element from the quadtree and performs merging if necessary
	/// </summary>
	/// <param name="element">The element to remove</param>
	/// <returns>Whether the element was successfully removed</returns>
	public bool Remove(T element) {
		if (element.Bounds.Size == Vector2.Zero) return false;

		QuadTree<T>? containingChild = GetContainingChild(element);

		bool removed = containingChild?.Remove(element) ?? elements.Remove(element);

		if (removed) {
			subElementCount--;
			
			if (subElementCount <= Threshold) {
				Merge();
			}
		}

		return removed;
	}
	
	/// <summary>
	/// Splits the quadtree (node) into four child quadrants if possible
	/// </summary>
	private void Split() {
		if (!IsLeaf || Level + 1 > MaxDepth) return;

		float w = Bounds.Width / 2;
		float h = Bounds.Height / 2;

		nw = new QuadTree<T>(new Vectangle(Bounds.X, Bounds.Y, w, h), Threshold, MaxDepth, Level + 1);
		ne = new QuadTree<T>(new Vectangle(Bounds.X + w, Bounds.Y, w, h), Threshold, MaxDepth, Level + 1);
		sw = new QuadTree<T>(new Vectangle(Bounds.X, Bounds.Y + h, w, h), Threshold, MaxDepth, Level + 1);
		se = new QuadTree<T>(new Vectangle(Bounds.X + w, Bounds.Y + h, w, h), Threshold, MaxDepth, Level + 1);

		for (int i = elements.Count - 1; i >= 0; i--) {
			QuadTree<T>? containingChild = GetContainingChild(elements[i]);

			if (containingChild != null) {
				T element = elements[i];

				elements[i] = elements[^1];
				elements.RemoveAt(elements.Count - 1);
				containingChild.InsertInternal(element);
			}
		}
	}

	/// <summary>
	/// Merges the quadtree's child quadrants if possible
	/// </summary>
	private void Merge() {
		if (IsLeaf) return;

		int totalCount = nw.subElementCount + ne.subElementCount + sw.subElementCount + se.subElementCount + elements.Count;
		if (totalCount >= Threshold) return;
		elements.Capacity = totalCount;

		elements.AddRange(nw.elements);
		elements.AddRange(ne.elements);
		elements.AddRange(sw.elements);
		elements.AddRange(se.elements);
		
		nw.Merge();
		ne.Merge();
		sw.Merge();
		se.Merge();

		nw = ne = sw = se = null;
	}

	/// <summary>
	/// Retrieves a list of elements that a specified element is colliding with
	/// </summary>
	/// <param name="element">The element to check collisions against</param>
	/// <param name="predicate">The predicate that needs to evaluate to true for an element to be even considered for collision (leave on null for no predicate checking)</param>
	/// <returns>A list of elements that <paramref name="element" /> is colliding with</returns>
	public List<T> CalculateCollisions(T element, Func<T, T, bool>? predicate = null) {
		Vectangle bounds = element.Bounds;
		if (!bounds.Intersects(Bounds)) return [];

		List<T> collisions = new(ExpectedCollisionCount);
		Stack<QuadTree<T>> nodeStack = stackPool.Borrow();

		nodeStack.Push(this);

		while (nodeStack.Count > 0) {
			QuadTree<T> node = nodeStack.Pop();

			for (int i = 0; i < node.elements.Count; i++) {
				if (!ReferenceEquals(node.elements[i], element) && (predicate == null || predicate(element, node.elements[i])) && bounds.Intersects(node.elements[i].Bounds)) {
					collisions.Add(node.elements[i]);
				}
			}

			if (!node.IsLeaf) {
				if (bounds.Intersects(node.nw.Bounds)) nodeStack.Push(node.nw);
				if (bounds.Intersects(node.ne.Bounds)) nodeStack.Push(node.ne);
				if (bounds.Intersects(node.sw.Bounds)) nodeStack.Push(node.sw);
				if (bounds.Intersects(node.se.Bounds)) nodeStack.Push(node.se);
			}
		}

		stackPool.Return(nodeStack);

		return collisions;
	}

	/// <summary>
	/// Retrieves a list of elements whose bounds intersect with a specified area
	/// </summary>
	/// <param name="area">The area bounds to check collisions against</param>
	/// <returns>A list of elements that <paramref name="area" /> is colliding with</returns>
	public List<T> CalculateCollisions(Vectangle area) {
		if (!area.Intersects(Bounds)) return [];

		List<T> collisions = new(ExpectedCollisionCount);
		Stack<QuadTree<T>> nodeStack = stackPool.Borrow();

		nodeStack.Push(this);

		while (nodeStack.Count > 0) {
			QuadTree<T> node = nodeStack.Pop();

			for (int i = 0; i < node.elements.Count; i++) {
				if (area.Intersects(node.elements[i].Bounds)) {
					collisions.Add(node.elements[i]);
				}
			}

			if (!node.IsLeaf) {
				if (area.Intersects(node.nw.Bounds)) nodeStack.Push(node.nw);
				if (area.Intersects(node.ne.Bounds)) nodeStack.Push(node.ne);
				if (area.Intersects(node.sw.Bounds)) nodeStack.Push(node.sw);
				if (area.Intersects(node.se.Bounds)) nodeStack.Push(node.se);
			}
		}

		stackPool.Return(nodeStack);

		return collisions;
	}

	/// <summary>
	/// Checks whether a specified element is currently in collision with any other <br>
	/// NOTE: usually faster than <see cref="CalculateCollisions(T, Func{T, T, bool})"/> due to early returning, so use this instead wherever possible
	/// </summary>
	/// <param name="element">The element to check collisions against</param>
	/// <param name="predicate">The predicate that needs to evaluate to true for an element to be even considered for collision (leave on null for no predicate checking)</param>
	/// <returns>True if <paramref name="element"/> is colliding with any other element, false otherwise</returns>
	public bool Collides(T element, Func<T, T, bool>? predicate = null) {
		Vectangle bounds = element.Bounds;
		if (!bounds.Intersects(Bounds)) return false;

		Stack<QuadTree<T>> nodeStack = stackPool.Borrow();
		nodeStack.Push(this);

		while (nodeStack.Count > 0) {
			QuadTree<T> node = nodeStack.Pop();

			for (int i = 0; i < node.elements.Count; i++) {
				if (!ReferenceEquals(node.elements[i], element) && (predicate == null || predicate(element, node.elements[i])) && bounds.Intersects(node.elements[i].Bounds)) {
					stackPool.Return(nodeStack);
					return true;
				}
			}

			if (!node.IsLeaf) {
				if (bounds.Intersects(node.nw.Bounds)) nodeStack.Push(node.nw);
				if (bounds.Intersects(node.ne.Bounds)) nodeStack.Push(node.ne);
				if (bounds.Intersects(node.sw.Bounds)) nodeStack.Push(node.sw);
				if (bounds.Intersects(node.se.Bounds)) nodeStack.Push(node.se);
			}
		}

		stackPool.Return(nodeStack);

		return false;
	}

	/// <summary>
	/// Traverses the quadtree, executing a provided Action on each node <br/>
	/// NOTE: node traversal order is BFS-like but if order really matters, you should probably implement your own traversal logic
	/// </summary>
	/// <param name="callback">The Action to execute on each node</param>
	public void Traverse(Action<QuadTree<T>> callback) {
		Queue<QuadTree<T>> nodeQueue = queuePool.Borrow();

		nodeQueue.Enqueue(this);

		while (nodeQueue.Count > 0) {
			QuadTree<T> node = nodeQueue.Dequeue();

			callback(node);

			if (!node.IsLeaf) {
				nodeQueue.Enqueue(node.nw);
				nodeQueue.Enqueue(node.ne);
				nodeQueue.Enqueue(node.sw);
				nodeQueue.Enqueue(node.se);
			}
		}

		queuePool.Return(nodeQueue);
	}

	/// <summary>
	/// Returns the (direct) child quadrant that contains the specified element, or null if there isn't one <br/>
	/// NOTE: null can either mean that the element falls completely outside the bounds of the quadtree, or that it doesn't entirely fit in any quadrant of it
	/// </summary>
	/// <param name="element">The element to check</param>
	/// <returns>The containing child quadrant, or null if there isn't one</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private QuadTree<T>? GetContainingChild(T element) {
		Vectangle bounds = element.Bounds;
		
		bool left = bounds.Right < center.X;
		bool right = bounds.Left >= center.X;
		bool top = bounds.Bottom < center.Y;
		bool bottom = bounds.Top >= center.Y;

		if (left) {
			if (top) return nw;
			else if (bottom) return sw;
		} else if (right) {
			if (top) return ne;
			else if (bottom) return se;
		}

		return null;
	}

	/// <summary>
	/// Draws the quadtree (node)'s bounds to the screen, coloring it based on its current saturation<br/>
	/// NOTE: needs to be called with <see cref="Traverse(Action{QuadTree{T}})">Traverse</see> on the root node to display every single quadrant
	/// </summary>
	/// <param name="gameTime">The current game time information</param>
	public void PostDraw(GameTime gameTime) {
		boundsTex ??= Utils.GenerateTexture(Math.Max(1, (int)Bounds.Width), Math.Max(1, (int)Bounds.Height), Color.LightGreen, true);

		Rectangle dest = (Rectangle)Bounds;
		float filledRatio = Math.Min(1, elements.Count / (float)Threshold);
		Game.SpriteBatch!.Draw(boundsTex.ToTexture2D(), dest, null, Color.Lerp(Color.White, Color.Red, filledRatio), 0, Vector2.Zero, SpriteEffects.None, 0);
	}
}
