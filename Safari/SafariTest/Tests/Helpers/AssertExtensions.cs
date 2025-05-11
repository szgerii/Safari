namespace SafariTest.Tests.Helpers;

public static class AssertExtensions {
	public static void DoesNotThrow<T>(this Assert assert, Action action) where T : Exception {
		try {
			action();
		} catch (T) {
			Assert.Fail("Expected no {0} exceptions to be thrown", typeof(T).Name);
		}
	}
	
	public static void DoesNotThrow(this Assert assert, Action action) {
		try {
			action();
		} catch (Exception e) {
			Assert.Fail("Expected no exceptions to be thrown, caught: ", e.GetType().Name);
		}
	}
}
