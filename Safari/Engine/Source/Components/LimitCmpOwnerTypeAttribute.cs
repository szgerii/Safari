using System;

namespace Engine.Components;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class LimitCmpOwnerTypeAttribute : Attribute {
	public Type[] AllowedTypes { get; init; }

	public LimitCmpOwnerTypeAttribute(params Type[] types) {
		AllowedTypes = types;
	}

	public bool IsAllowedType(Type type) {
		foreach (Type t in AllowedTypes) {
			if (type == t || type.IsSubclassOf(t)) return true;
		}

		return false;
	}
}