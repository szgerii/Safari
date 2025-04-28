using System.Reflection;

namespace SafariTest.Utils;

/// <summary>
/// Provides access to an object's private methods
/// </summary>
internal class PrivateObject {
	public object Object { get; private init; }

	public PrivateObject(object target) {
		Object = target;
	}

	public object? Invoke(string methodName, params object?[]? args) {
		MethodInfo? mi = Object.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

		if (mi == null) {
			throw new ArgumentException($"Unknown instance method '{methodName}' in object of type '{Object.GetType()}'");
		}

		return mi.Invoke(Object, args);
	}

	public void SetProperty(string propName, object? value) {
		PropertyInfo? prop = Object.GetType().GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance) ??
							 Object.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);

		if (prop == null) {
			throw new ArgumentException($"Unknown property '{propName}' in object of type '{Object.GetType()}'");
		}

		prop.SetValue(Object, value);
	}

	public void SetField(string fieldName, object? value) {
		FieldInfo? field = Object.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance) ??
						   Object.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);

		if (field == null) {
			throw new ArgumentException($"Unknown field '{fieldName}' in object of type '{Object.GetType()}'");
		}

		field.SetValue(Object, value);
	}
}