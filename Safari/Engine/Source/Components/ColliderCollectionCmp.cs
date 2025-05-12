using Engine.Collision;
using System.Collections.Generic;

namespace Engine.Components;

public class ColliderCollectionCmp : Component {
	public CollisionTags DefaultTag { get; set; }
	public CollisionTags DefaultTargets { get; set; }

	private readonly List<CollisionCmp> components = new();

	public void AddCollider(CollisionCmp cmp) {
		if (cmp.Tags == 0) {
			cmp.Tags = DefaultTag;
		}

		if (cmp.Targets == 0) {
			cmp.Targets = DefaultTargets;
		}

		cmp.Owner = Owner;
		components.Add(cmp);

		if (Owner!.Loaded) {
			cmp.Load();
		}
	}

	public void RemoveCollider(CollisionCmp cmp) {
		if (Owner!.Loaded) {
			cmp.Unload();
		}
		
		cmp.Owner = null;
		components.Remove(cmp);
	}

	public void Clear() {
		if (Owner!.Loaded) {
			foreach (CollisionCmp cmp in components) {
				cmp.Unload();
			}
		}

		components.Clear();
	}

	public override void Load() {
        foreach (CollisionCmp cmp in components) {
			cmp.Load();
        }

        base.Load();
	}

	public override void Unload() {
		foreach (CollisionCmp cmp in components) {
			cmp.Unload();
		}

		base.Unload();
	}
}