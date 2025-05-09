using System.Reflection;

namespace SafariTest.Utils;

/// <summary>
/// Provides access to the private static methods of a guess
/// </summary>
internal class PrivateType {
	public Type Type { get; private init; }

	public PrivateType(Type target) {
		Type = target;
	}

	public object? InvokeStatic(string methodName, params object?[]? args) {
		MethodInfo? mi = Type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

		if (mi == null) {
			throw new ArgumentException($"Unknown static method '{methodName}' in class '{Type}'");
		}

		return mi.Invoke(null, args);
	}

	public void SetProperty(string propName, object? value) {
		PropertyInfo? prop = Type.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Static) ??
							 Type.GetProperty(propName, BindingFlags.Public | BindingFlags.Static);

		if (prop == null) {
			throw new ArgumentException($"Unknown static property '{propName}' in class '{Type}'");
		}

		prop.SetValue(null, value);
	}

	public object? GetProperty(string propName) {
		PropertyInfo? prop = Type.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Static) ??
							 Type.GetProperty(propName, BindingFlags.Public | BindingFlags.Static);

		if (prop == null) {
			throw new ArgumentException($"Unknown static property '{propName}' in class '{Type}'");
		}

		return prop.GetValue(null);
	}

	public void SetField(string propName, object? value) {
		FieldInfo? field = Type.GetField(propName, BindingFlags.NonPublic | BindingFlags.Static) ??
						  Type.GetField(propName, BindingFlags.Public | BindingFlags.Static);

		if (field == null) {
			throw new ArgumentException($"Unknown static field '{propName}' in class '{Type}'");
		}

		field.SetValue(null, value);
	}

	public object? GetField(string propName) {
		FieldInfo? field = Type.GetField(propName, BindingFlags.NonPublic | BindingFlags.Static) ??
						  Type.GetField(propName, BindingFlags.Public | BindingFlags.Static);

		if (field == null) {
			throw new ArgumentException($"Unknown static field '{propName}' in class '{Type}'");
		}

		return field.GetValue(null);
	}
}
