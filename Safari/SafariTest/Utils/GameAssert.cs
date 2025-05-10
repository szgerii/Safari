using Safari.Scenes;

namespace SafariTest.Utils;

/// <summary>
/// Useful for assertions for simulation checking (or any game logic that happens across multiple frames)
/// </summary>
internal static class GameAssert {
	/// <summary>
	/// The game instance to step forward frame by frame <br/>
	/// Assertions will throw <see cref="InvalidOperationException"/> if this is not set before they are called
	/// </summary>
	internal static Safari.Game? GameInstance { get; set; }

	/// <summary>
	/// The maximum amount of frames the ...Before and ...Until will calculate by default (safeguard mechanism against infinite loops)
	/// </summary>
	internal static int DefaultFrameLimit { get; set; } = 1000;

	internal static event EventHandler? RanFrame;

	#region InNFrames
	/// <summary>
	/// Tests whether a condition is ever met during the next n frames, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int TrueInNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		int result = RunInstanceFor(n, (int idx) => condition() ? idx : null);

		return result != -1 ? result : throw new AssertFailedException($"Condition stayed false through {n} frames");
	}

	/// <summary>
	/// Tests whether a condition ever fails during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int FalseInNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		int result = RunInstanceFor(n, (int idx) => condition() ? null : idx);

		return result != -1 ? result : throw new AssertFailedException($"Condition stayed true through {n} frames");
	}

	/// <summary>
	/// Tests whether a value ever becomes an expected value during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="expected">The expected value</param>
	/// <param name="actual">The function that determines the value to check every frame</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int AreEqualInNFrames<T>(T? expected, Func<T?> actual, int n) {
		AssertGameInstanceNotNull();

		T? curr = default;
		int result = RunInstanceFor(n, (int idx) => {
			curr = actual();

			if (expected?.Equals(curr) ?? (curr == null)) {
				return idx;
			}

			return null;
		});

		if (result == -1) {
			string msg =
				$"Actual value was never equal to the expected value in {n} frames\n" +
				$"Expected: {expected}\n" +
				$"Actual (after last frame): {curr}";

			throw new AssertFailedException(msg);
		}

		return result;
	}

	/// <summary>
	/// Tests whether a value ever becomes anything else than the expected value during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="notExpected">The value <paramref name="notExpected"/> needs to differ from</param>
	/// <param name="actual">The function that determines the value to check every frame</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int AreNotEqualInNFrames<T>(T? notExpected, Func<T?> actual, int n) {
		AssertGameInstanceNotNull();

		T? curr = default;
		int result = RunInstanceFor(n, (int idx) => {
			curr = actual();

			if (!(notExpected?.Equals(curr) ?? (curr == null))) {
				return idx;
			}

			return null;
		});

		if (result == -1) {
			string msg =
				$"Actual value was always equal to the expected value in {n} frames\n" +
				$"Expected: {notExpected}\n" +
				$"Actual (after last frame): {curr}";

			throw new AssertFailedException(msg);
		}

		return result;
	}

	/// <summary>
	/// Tests whether a value ever becomes null during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int IsNullInNFrames<T>(Func<T?> getValue, int n) {
		AssertGameInstanceNotNull();

		int result = RunInstanceFor(n, (int idx) => {
			if (getValue() == null) {
				return idx;
			}

			return null;
		});

		if (result == -1) {
			string msg =
				$"Value didn't become null in {n} frames\n" +
				$"Value (after last frame): {getValue()}";

			throw new AssertFailedException(msg);
		}

		return result;
	}

	/// <summary>
	/// Tests whether a value ever becomes not-null during the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="n">The maximum number of frame steps to perform</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int IsNotNullInNFrames<T>(Func<T?> getValue, int n) {
		AssertGameInstanceNotNull();

		int result = RunInstanceFor(n, (int idx) => getValue() != null ? idx : null);

		if (result == -1) {
			string msg = $"Value never became not null in {n} frames";
			throw new AssertFailedException(msg);
		}

		return result;
	}
	#endregion

	#region ForNFrames
	/// <summary>
	/// Tests whether a condition is continously met for the next n frames, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The number of frame steps to test</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void TrueForNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		RunInstanceFor(n, (int idx) => {
			return condition() ?
				null :
				throw new AssertFailedException($"Condition was not met after {idx} frames (expected: {n} frames)");
		});
	}

	/// <summary>
	/// Tests whether a condition is continously not met for the next n frames, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="n">The number of frame steps to test</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void FalseForNFrames(Func<bool> condition, int n) {
		AssertGameInstanceNotNull();

		RunInstanceFor(n, (int idx) => {
			return !condition() ?
				null :
				throw new AssertFailedException($"Condition was not met after {idx} frames (expected: {n} frames)");
		});
	}

	/// <summary>
	/// Tests whether a value stays as an expected value for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="expected">The expected value</param>
	/// <param name="actual">The function that determines the value to check every frame</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void AreEqualForNFrames<T>(T? expected, Func<T?> actual, int n) {
		AssertGameInstanceNotNull();

		RunInstanceFor(n, (int idx) => {
			T? curr = actual();

			if (!(expected?.Equals(curr) ?? (curr == null))) {
				string msg =
					$"Actual value differed from the expected value after {idx} frames (expected: {n} frames)\n" +
					$"Expected: {expected}\n" +
					$"Actual (after failed frame): {curr}";

				throw new AssertFailedException(msg);
			}

			return null;
		});
	}

	/// <summary>
	/// Tests whether a value stays different from an expected value for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="forbidden">The value <paramref name="actual"/> needs to differ from</param>
	/// <param name="actual">The function that determines the value to check every frame</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void AreNotEqualForNFrames<T>(T? forbidden, Func<T?> actual, int n) {
		AssertGameInstanceNotNull();

		RunInstanceFor(n, (int idx) => {
			T? curr = actual();

			if (forbidden?.Equals(curr) ?? curr == null) {
				string msg =
					$"Actual value was the same as the forbidden value after {idx} frames (expected: {n} frames)\n" +
					$"Forbidden: {forbidden}\n" +
					$"Actual (after failed frame): {curr}";

				throw new AssertFailedException(msg);
			}

			return null;
		});
	}

	/// <summary>
	/// Tests whether a value stays null for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void IsNullForNFrames<T>(Func<T?> getValue, int n) {
		AssertGameInstanceNotNull();

		RunInstanceFor(n, (int idx) => {
			if (getValue() != null) {
				string msg =
					$"Actual value was not null after {idx} frames (expected: {n} frames)\n" +
					$"Actual (after failed frame): {getValue()}";

				throw new AssertFailedException(msg);
			}

			return null;
		});
	}

	/// <summary>
	/// Tests whether a value stays not null for the next n frames, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="n">The number of frame steps to perform</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void IsNotNullForNFrames<T>(Func<T?> getValue, int n) {
		AssertGameInstanceNotNull();

		RunInstanceFor(n, (int idx) => {
			if (getValue() == null) {
				string msg = $"Actual value became null after {idx} frames (expected: {n} frames)";
				throw new AssertFailedException(msg);
			}

			return null;
		});
	}
	#endregion

	#region Before
	/// <summary>
	/// Tests whether a condition is ever met before a given ingame time, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="endDate">The ingame time before the condition must be met</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int TrueBefore(Func<bool> condition, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => condition() ? idx : null, maxFrames ?? DefaultFrameLimit);

		return result != -1 ? result : throw new AssertFailedException($"Condition stayed false until {endDate}");
	}

	/// <summary>
	/// Tests whether a condition ever fails before a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="endDate">The ingame time before the condition must fail</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int FalseBefore(Func<bool> condition, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => condition() ? null : idx, maxFrames ?? DefaultFrameLimit);

		return result != -1 ? result : throw new AssertFailedException($"Condition stayed true until {endDate}");
	}

	/// <summary>
	/// Tests whether a value ever becomes an expected value before a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="expected">The expected value</param>
	/// <param name="actual">The function that determines the value to compare every frame</param>
	/// <param name="endDate">The ingame time before the value must become equal to <paramref name="expected"/></param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int AreEqualBefore<T>(T? expected, Func<T?> actual, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		T? curr = default;
		int result = RunInstanceUntil(endDate, (int idx) => {
			curr = actual();

			if (expected?.Equals(curr) ?? (curr == null)) {
				return idx;
			}

			return null;
		}, maxFrames ?? DefaultFrameLimit);

		if (result != -1) return result;

		string msg =
			$"Actual value was never equal to the expected value before {endDate}\n" +
			$"Expected: {expected}\n" +
			$"Actual (after last frame): {curr}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value ever becomes anything else than the expected value before a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="notExpected">The value <paramref name="notExpected"/> needs to differ from</param>
	/// <param name="actual">The function that determines the value to compare every frame</param>
	/// <param name="endDate">The ingame time before the value must become different from <paramref name="notExpected"/></param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int AreNotEqualBefore<T>(T? notExpected, Func<T?> actual, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		T? curr = default;
		int result = RunInstanceUntil(endDate, (int idx) => {
			curr = actual();

			if (!(notExpected?.Equals(curr) ?? (curr == null))) {
				return idx;
			}

			return null;
		}, maxFrames ?? DefaultFrameLimit);

		if (result != -1) return result;

		string msg =
			$"Actual value was always equal to the expected value before {endDate}\n" +
			$"Expected: {notExpected}\n" +
			$"Actual (after last frame): {curr}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value ever becomes null before a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="endDate">The ingame time before the value must become null</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int IsNullBefore<T>(Func<T?> getValue, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => getValue() == null ? idx : null, maxFrames ?? DefaultFrameLimit);

		if (result != -1) return result;

		string msg =
			$"Value didn't become null before {endDate}\n" +
			$"Value (after last frame): {getValue()}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value ever becomes not-null before a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="endDate">The ingame time before the value must become not-null</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <returns>The number of frame steps that were performed</returns>
	/// <exception cref="AssertFailedException"></exception>
	public static int IsNotNullBefore<T>(Func<T?> getValue, DateTime endDate, int? maxFrames) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => getValue() != null ? idx : null, maxFrames ?? DefaultFrameLimit);

		if (result != -1) return result;

		string msg = $"Value never became not null before {endDate}";
		throw new AssertFailedException(msg);
	}

	public static int TrueBefore(Func<bool> condition, TimeSpan duration, int? maxFrames = null)
		=> TrueBefore(condition, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static int FalseBefore(Func<bool> condition, TimeSpan duration, int? maxFrames = null)
		=> FalseBefore(condition, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static int AreEqualBefore<T>(T? expected, Func<T?> actual, TimeSpan duration, int? maxFrames = null)
		=> AreEqualBefore(expected, actual, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static int AreNotEqualBefore<T>(T? notExpected, Func<T?> actual, TimeSpan duration, int? maxFrames = null)
		=> AreNotEqualBefore(notExpected, actual, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static int IsNullBefore<T>(Func<T?> getValue, TimeSpan duration, int? maxFrames = null)
		=> IsNullBefore(getValue, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static int IsNotNullBefore<T>(Func<T?> getValue, TimeSpan duration, int? maxFrames = null)
		=> IsNotNullBefore(getValue, GameScene.Active.Model.IngameDate + duration, maxFrames);
	#endregion

	#region Until
	/// <summary>
	/// Tests whether a condition is continously met until a given ingame time, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check after every frame step</param>
	/// <param name="endDate">The ingame time until the condition must remain true</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void TrueUntil(Func<bool> condition, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => condition() ? null : idx, maxFrames ?? DefaultFrameLimit);

		if (result != -1) {
			throw new AssertFailedException(
				$"Condition became false after {result} frames\n" +
				$"Current date: {GameScene.Active.Model.IngameDate}\n" +
				$"Expected until: {endDate}"
			);
		}
	}

	/// <summary>
	/// Tests whether a condition continously fails before a given ingame time, and throws an exception if it isn't
	/// </summary>
	/// <param name="condition">The condition to check for failure after every frame step</param>
	/// <param name="endDate">The ingame time until the condition must remain false</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void FalseUntil(Func<bool> condition, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => condition() ? idx : null, maxFrames ?? DefaultFrameLimit);

		if (result != -1) {
			throw new AssertFailedException(
				$"Condition became true after {result} frames\n" +
				$"Current date: {GameScene.Active.Model.IngameDate}\n" +
				$"Expected until: {endDate}"
			);
		}
	}

	/// <summary>
	/// Tests whether a value continously remains equal to an expected value until a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="expected">The expected value</param>
	/// <param name="actual">The function that determines the value to compare every frame</param>
	/// <param name="endDate">The ingame time until the value must remain equal to <paramref name="expected"/></param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void AreEqualUntil<T>(T? expected, Func<T?> actual, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		T? curr = default;
		int result = RunInstanceUntil(endDate, (int idx) => {
			curr = actual();

			if (expected?.Equals(curr) ?? (curr == null)) {
				return null;
			}

			return idx;
		}, maxFrames ?? DefaultFrameLimit);

		if (result == -1) return;

		string msg =
			$"Actual value differed from the expected value after {result} frames\n" +
			$"Expected: {expected}\n" +
			$"Actual (after last frame): {curr}\n" +
			$"Current date: {GameScene.Active.Model.IngameDate}\n" +
			$"Expected until: {endDate}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value remains different from the expected value until a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="notExpected">The value <paramref name="notExpected"/> needs to differ from</param>
	/// <param name="actual">The function that determines the value to compare every frame</param>
	/// <param name="endDate">The ingame time until the value must differ from <paramref name="notExpected"/></param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void AreNotEqualUntil<T>(T? notExpected, Func<T?> actual, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		T? curr = default;
		int result = RunInstanceUntil(endDate, (int idx) => {
			curr = actual();

			if (!(notExpected?.Equals(curr) ?? (curr == null))) {
				return null;
			}

			return idx;
		}, maxFrames ?? DefaultFrameLimit);

		if (result == -1) return;

		string msg =
			$"Actual value was equal to the expected value after {result} frames\n" +
			$"Expected: {notExpected}\n" +
			$"Current data: {GameScene.Active.Model.IngameDate}\n" +
			$"Expected until: {endDate}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value remains null until a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="endDate">The ingame time until the value must remain null</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void IsNullUntil<T>(Func<T?> getValue, DateTime endDate, int? maxFrames = null) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => getValue() == null ? null : idx, maxFrames ?? DefaultFrameLimit);

		if (result == -1) return;

		string msg =
			$"Value became not-null after {result} frames\n" +
			$"Value (after last frame): {getValue()}\n" +
			$"Current date: {GameScene.Active.Model.IngameDate}\n" +
			$"Expected until: {endDate}";

		throw new AssertFailedException(msg);
	}

	/// <summary>
	/// Tests whether a value remains not-null until a given ingame time, and throws an exception if it doesn't
	/// </summary>
	/// <param name="getValue">The function that determines the value to check every frame</param>
	/// <param name="endDate">The ingame time until the value must remain not-null</param>
	/// <param name="maxFrames">The maximum number of frames to run for (uses <see cref="DefaultFrameLimit"/> by default)</param>
	/// <exception cref="AssertFailedException"></exception>
	public static void IsNotNullUntil<T>(Func<T?> getValue, DateTime endDate, int? maxFrames) {
		AssertGameInstanceNotNull();

		int result = RunInstanceUntil(endDate, (int idx) => getValue() != null ? null : idx, maxFrames ?? DefaultFrameLimit);

		if (result == -1) return;

		string msg =
			$"Value became null after {result} frames\n" +
			$"Current date: {GameScene.Active.Model.IngameDate}\n" +
			$"Expected until: {endDate}";
		throw new AssertFailedException(msg);
	}

	public static void TrueUntil(Func<bool> condition, TimeSpan duration, int? maxFrames = null)
		=> TrueUntil(condition, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static void FalseUntil(Func<bool> condition, TimeSpan duration, int? maxFrames = null)
		=> FalseUntil(condition, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static void AreEqualUntil<T>(T? expected, Func<T?> actual, TimeSpan duration, int? maxFrames = null)
		=> AreEqualUntil(expected, actual, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static void AreNotEqualUntil<T>(T? notExpected, Func<T?> actual, TimeSpan duration, int? maxFrames = null)
		=> AreNotEqualUntil(notExpected, actual, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static void IsNullUntil<T>(Func<T?> getValue, TimeSpan duration, int? maxFrames = null)
		=> IsNullUntil(getValue, GameScene.Active.Model.IngameDate + duration, maxFrames);
	public static void IsNotNullUntil<T>(Func<T?> getValue, TimeSpan duration, int? maxFrames = null)
		=> IsNotNullUntil(getValue, GameScene.Active.Model.IngameDate + duration, maxFrames);
	#endregion

	#region RunInstanceFor
	private delegate int? RunCycleIteration(int frameIdx);
	private delegate int? RunCycleIteraionNoArgs();

	private static int RunInstanceFor(int n, RunCycleIteration iteration) {
		AssertGameInstanceNotNull();

		for (int i = 1; i <= n; i++) {
			GameInstance!.RunOneFrameNoDraw();
			RanFrame?.Invoke(null, EventArgs.Empty);

			int? result = iteration(i);
			if (result != null) {
				return (int)result;
			}
		}

		return -1;
	}

	private static int RunInstanceFor(int n, RunCycleIteraionNoArgs iteration) {
		return RunInstanceFor(n, (int _) => iteration());
	}

	private static int RunInstanceUntil(DateTime endDate, RunCycleIteration iteration, int maxFrames) {
		AssertGameInstanceNotNull();

		DateTime currentDate = GameScene.Active.Model.IngameDate;
		for (int i = 1; i <= maxFrames && currentDate < endDate; i++) {
			GameInstance!.RunOneFrameNoDraw();
			RanFrame?.Invoke(null, EventArgs.Empty);
			currentDate = GameScene.Active.Model.IngameDate;

			if (currentDate <= endDate) {
				int? result = iteration(i);

				if (result != null) {
					return (int)result;
				}
			}
		}

		return -1;
	}

	private static int RunInstanceUntil(DateTime endDate, RunCycleIteration iteration)
		=> RunInstanceUntil(endDate, iteration, DefaultFrameLimit);

	private static int RunInstanceUntil(DateTime endDate, RunCycleIteraionNoArgs iteration, int maxFrames)
		=> RunInstanceUntil(endDate, (int _) => iteration(), maxFrames);

	private static int RunInstanceUntil(DateTime endDate, RunCycleIteraionNoArgs iteration)
		=> RunInstanceUntil(endDate, (int _) => iteration());
	#endregion

	private static void AssertGameInstanceNotNull() {
		if (GameInstance == null) {
			throw new InvalidOperationException("Cannot use GameAssert before initalizing GameInstace");
		}
	}
}
