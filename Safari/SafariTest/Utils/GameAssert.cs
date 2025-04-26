namespace SafariTest.Utils;

internal static class GameAssert {
	/// <summary>
	/// The game instance to step forward frame by frame <br/>
	/// Assertions will throw <see cref="InvalidOperationException"/> if this is not set before they are called
	/// </summary>
	public static Safari.Game? GameInstance { get; set; }

	/// <summary>
	/// Tests whether a condition is ever met during the next n frames, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int TrueInNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1;  i <= n; i++) {
			GameInstance!.RunOneFrame();
			
			if (condition()) {
				return i;
			}
		}

		throw new AssertFailedException($"Condition stayed false through {n} frames");
	}

	/// <summary>
	/// Tests whether a condition ever fails during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int FalseInNFrames(Func<bool> condition, int n) {
		try {
			int i = TrueInNFrames(() => !condition(), n);
			return i;
		} catch (AssertFailedException) {
			throw new AssertFailedException($"Condition stayed true through {n} frames");
		}
	}

	/// <summary>
	/// Tests whether a value ever becomes an expected value during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="expected">The expected value</param>
	/// <param name="actual">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int AreEqualInNFrames<T>(T? expected, ref T? actual, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (expected?.Equals(actual) ?? (actual == null)) {
				return i;
			}
		}

		string msg =
			$"Actual value was never equal to the expected value in {n} frames" +
			$"Expected: {expected}" +
			$"Actual (after last frame): {actual}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value ever becomes anything else than the expected value during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="notExpected">The value <paramref name="notExpected"/> needs to differ from</param>
	/// <param name="actual">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int AreNotEqualInNFrames<T>(T? notExpected, ref T? actual, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (!(notExpected?.Equals(actual) ?? (actual == null))) {
				return i;
			}
		}

		string msg =
			$"Actual value was always equal to the expected value in {n} frames" +
			$"Expected: {notExpected}" +
			$"Actual (after last frame): {actual}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value ever becomes null during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="observed">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int IsNullInNFrames<T>(ref T? observed, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (observed == null) {
				return i;
			}
		}

		string msg =
			$"Value didn't become null in {n} frames" +
			$"Value (after last frame): {observed}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value ever becomes not-null during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="observed">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int IsNotNullInNFrames<T>(ref T? observed, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (observed != null) {
				return i;
			}
		}

		string msg = $"Value never became not null in {n} frames";
		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a condition is continously met for the next n frames, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The number of frame steps to test</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void TrueForNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			GameInstance!.RunOneFrame();

			if (!condition()) {
				throw new AssertFailedException($"Condition was not met after {i} frames (expected: {n} frames)");
			}
		}
	}

	/// <summary>
	/// Tests whether a condition is continously not met for the next n frames, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The number of frame steps to test</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void FalseForNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			GameInstance!.RunOneFrame();

			if (condition()) {
				throw new AssertFailedException($"Condition was met after {i} frames (expected: {n} frames)");
			}
		}
	}

	/// <summary>
	/// Tests whether a value stays as an expected value for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="expected">The expected value</param>
	/// <param name="actual">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void AreEqualForNFrames<T>(T? expected, ref T? actual, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (!(expected?.Equals(actual) ?? (actual == null))) {
				string msg =
					$"Actual value differed from the expected value after {i} frames (expected: {n} frames)" +
					$"Expected: {expected}" +
					$"Actual (after failed frame): {actual}";

				throw new AssertFailedException(msg);
			}
		}
	}

	/// <summary>
	/// Tests whether a value stays different from an expected value for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="forbidden">The value <paramref name="actual"/> needs to differ from</param>
	/// <param name="actual">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void AreNotEqualForNFrames<T>(T? forbidden, ref T? actual, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (forbidden?.Equals(actual) ?? actual == null) {
				string msg =
					$"Actual value was the same as the forbidden value after {i} frames (expected: {n} frames)" +
					$"Forbidden: {forbidden}" +
					$"Actual (after failed frame): {actual}";

				throw new AssertFailedException(msg);
			}
		}
	}

	/// <summary>
	/// Tests whether a value stays null for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="actual">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void IsNullForNFrames<T>(ref T? actual, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (actual != null) {
				string msg = 
					$"Actual value was not null after {i} frames (expected: {n} frames)" +
					$"Actual (after failed frame): {actual}";

				throw new AssertFailedException(msg);
			}
		}
	}
	
	/// <summary>
	/// Tests whether a value stays not null for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="actual">A reference to the variable to check (pass the observed var/field/prop directly so that its state remains valid)</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void IsNotNullForNFrames<T>(ref T? actual, int n) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			if (actual == null) {
				string msg = $"Actual value became null after {i} frames (expected: {n} frames)";
				throw new AssertFailedException(msg);
			}
		}
	}

	private static void AssertGameInstanceNotNull() {
		if (GameInstance == null) {
			throw new InvalidOperationException("Cannot use GameAssert before initalizing GameInstace");
		}
	}
}
