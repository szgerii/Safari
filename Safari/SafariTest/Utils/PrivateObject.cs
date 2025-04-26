using System.Reflection;

namespace SafariTest.Utils;

internal class PrivateObject {
	private readonly object obj;

	public PrivateObject(object target) {
		obj = target;
	}

	public object? Invoke(string methodName, params object?[]? args) {
		MethodInfo? mi = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

		if (mi == null) {
			throw new ArgumentException($"Unknown instance method '{methodName}' in class '{obj.GetType()}'");
		}

		return mi.Invoke(obj, args);
	}
}