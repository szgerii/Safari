using System.Reflection;

namespace SafariTest.Utils;

internal class PrivateType {
	private readonly Type type;

	public PrivateType(Type target) {
		type = target;
	}

	public object? InvokeStatic(string methodName, params object?[]? args) {
		MethodInfo? mi = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);

		if (mi == null) {
			throw new ArgumentException($"Unknown static method '{methodName}' in class '{type}'");
		}

		return mi.Invoke(null, args);
	}

	public void SetProperty(string propName, object? value) {
		PropertyInfo? prop = type.GetProperty(propName, BindingFlags.Static | BindingFlags.Public);

		if (prop == null) {
			throw new ArgumentException($"Unknown static property '{propName}' in class '{type}'");
		}
		
		prop.SetValue(null, value);
	}
}
