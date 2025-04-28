using Microsoft.Xna.Framework;
using SafariTest.Utils;
using System;
using System.Diagnostics;

namespace SafariTest.Tests.Helpers;

[TestClass]
public class GameAssertTest : SimulationTest {
	private DateTime Now => Model.IngameDate;

	[TestMethod("InNFrames Assertions Test")]
	public void InNFramesTest() {
		int frameCount = 0, usedFrames = 0;

		GameAssert.RanFrame += (object? _, EventArgs _) => {
			frameCount++;
		};

		// TrueInNFrames
		usedFrames = GameAssert.TrueInNFrames(() => frameCount == 2, 3);
		Assert.AreEqual(2, usedFrames);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.TrueInNFrames(() => frameCount == 4, 3));

		// FalseInNFrames
		frameCount = 0;
		usedFrames = GameAssert.FalseInNFrames(() => frameCount < 2, 3);
		Assert.AreEqual(2, usedFrames);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.FalseInNFrames(() => frameCount < 5, 3));

		// AreEqualInNFrames
		frameCount = 0;
		usedFrames = GameAssert.AreEqualInNFrames(2, () => frameCount, 3);
		Assert.AreEqual(2, usedFrames);

		frameCount = 0;
		usedFrames = GameAssert.AreEqualInNFrames(1, () => frameCount, 3);
		Assert.AreEqual(1, usedFrames);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreEqualInNFrames(4, () => frameCount, 3));

		// AreNotEqualInNFrames
		frameCount = 0;
		usedFrames = GameAssert.AreNotEqualInNFrames(0, () => frameCount, 3);
		Assert.AreEqual(1, usedFrames);

		frameCount = 0;
		usedFrames = GameAssert.AreNotEqualInNFrames(1, () => frameCount, 3);
		Assert.AreEqual(2, usedFrames);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreNotEqualInNFrames(0, () => 0, 3));

		// IsNullInNFrames
		string? nullable;
		bool toNull = true;
		GameAssert.RanFrame += (object? _, EventArgs _) => {
			if (frameCount == 5) {
				nullable = toNull ? null : "";
			}
		};

		nullable = "";
		frameCount = 0;
		usedFrames = GameAssert.IsNullInNFrames(() => nullable, 10);
		Assert.AreEqual(5, usedFrames);

		nullable = "";
		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNullInNFrames(() => nullable, 3));

		// IsNotNullInNFrames
		toNull = false;

		nullable = null;
		frameCount = 0;
		usedFrames = GameAssert.IsNotNullInNFrames(() => nullable, 10);
		Assert.AreEqual(5, usedFrames);

		nullable = null;
		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNotNullInNFrames(() => nullable, 3));
	}

	[TestMethod("ForNFrames Assertions Test")]
	public void ForNFramesTest() {
		int frameCount = 0;

		GameAssert.RanFrame += (object? _, EventArgs _) => {
			frameCount++;
		};

		// TrueForNFrames
		frameCount = 0;
		GameAssert.TrueForNFrames(() => frameCount < 5, 3);

		Assert.ThrowsException<AssertFailedException>(() => GameAssert.TrueForNFrames(() => frameCount < 2, 3));

		// FalseForNFrames
		frameCount = 0;
		GameAssert.FalseForNFrames(() => frameCount > 5, 3);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.FalseForNFrames(() => frameCount >= 2, 3));

		// AreEqualForNFrames
		frameCount = 0;
		GameAssert.AreEqualForNFrames(2, () => 2, 3);

		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreEqualForNFrames(4, () => 2, 3));
		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreEqualForNFrames(1, () => frameCount, 3));

		// AreNotEqualForNFrames
		frameCount = 0;
		GameAssert.AreNotEqualForNFrames(5, () => frameCount, 3);

		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreNotEqualForNFrames(0, () => 0, 3));
		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreNotEqualForNFrames(2, () => frameCount, 3));

		// IsNullForNFrames
		string? nullable;
		bool toNull = false;
		GameAssert.RanFrame += (object? _, EventArgs _) => {
			if (frameCount == 5) {
				nullable = toNull ? null : "";
			}
		};

		frameCount = 0;
		GameAssert.IsNullForNFrames<string>(() => null, 3);
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNullForNFrames(() => "", 3));

		nullable = "";
		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNullForNFrames(() => nullable, 8));

		// IsNotNullForNFrames
		toNull = true;

		GameAssert.IsNotNullForNFrames(() => "", 3);
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNotNullForNFrames<string>(() => null, 3));

		nullable = "";
		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNotNullForNFrames(() => nullable, 8));
	}

	[TestMethod("Before Assertion Tests")]
	public void BeforeTest() {
		DateTime before = Now;

		int frameCount = 0, usedFrames = 0;

		GameAssert.RanFrame += (object? _, EventArgs _) => {
			frameCount++;
		};

		TimeSpan hour = TimeSpan.FromHours(1);
		TimeSpan maxStep = 4 * hour;

		RunOneFrame();
		before = Now;
		RunOneFrame();
		TimeSpan step = 3 * (Now - before);
		if (step > maxStep) {
			Debug.WriteLine("Skipped Before assertions tests due to slow execution environment");
			return;
		}

		frameCount = 0;

		// TrueBefore
		before = Now;
		GameAssert.TrueBefore(() => Now - before > step, 2 * step);
		before = Now;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.TrueBefore(() => Now - before > 2 * step, step));

		// FalseBefore
		before = Now;
		GameAssert.FalseBefore(() => Now - before < step, 2 * step);
		before = Now;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.FalseBefore(() => Now - before < 2 * step, step));

		// AreEqualBefore
		frameCount = 0;
		GameAssert.AreEqualBefore(2, () => frameCount, step);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreEqualBefore(-1, () => frameCount, step));

		// AreNotEqualBefore
		frameCount = 0;
		GameAssert.AreNotEqualBefore(-1, () => frameCount, step);

		frameCount = 0;
		usedFrames = GameAssert.AreNotEqualBefore(1, () => frameCount, step);
		Assert.AreEqual(2, usedFrames);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreNotEqualBefore(0, () => 0, step));

		// IsNullBefore
		GameAssert.IsNullBefore<string>(() => null, step);
		before = Now;
		GameAssert.IsNullBefore(() => Now - before > 5 * step ? null : "", 10 * step);
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNullBefore(() => "", step));

		// IsNotNullBefore
		GameAssert.IsNotNullBefore(() => "", step);
		before = Now;
		GameAssert.IsNotNullBefore(() => Now - before > 5 * step ? "" : null, 10 * step);
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNotNullBefore<string>(() => null, step));
	}

	[TestMethod("Until Assertion Tests")]
	public void UntilTest() {
		int frameCount = 0;

		GameAssert.RanFrame += (object? _, EventArgs _) => {
			frameCount++;
		};

		DateTime before;
		TimeSpan hour = TimeSpan.FromHours(1);
		TimeSpan maxStep = 2 * TimeSpan.FromSeconds(1);

		RunOneFrame();
		before = Now;
		RunOneFrame();
		TimeSpan step = (Now - before) * 3;
		if (step > maxStep) {
			Debug.WriteLine("Skipped Until assertions tests due to slow execution environment");
			return;
		}

		frameCount = 0;

		// TrueUntil
		before = Now;
		GameAssert.TrueUntil(() => Now - before < 2 * step, step);
		before = Now;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.TrueUntil(() => Now - before < step, 2 * step));

		// FalseUntil
		before = Now;
		GameAssert.FalseUntil(() => Now - before > 2 * step, step);
		before = Now;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.FalseUntil(() => Now - before > step, 2 * step));

		// AreEqualUntil
		GameAssert.AreEqualUntil(0, () => 0, step);

		frameCount = 0;
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreEqualUntil(1, () => frameCount, 2 * step));

		// AreNotEqualUntil
		frameCount = 0;
		GameAssert.AreNotEqualUntil(100, () => frameCount, step);

		Assert.ThrowsException<AssertFailedException>(() => GameAssert.AreNotEqualUntil(0, () => 0, step));

		// IsNullUntil
		GameAssert.IsNullUntil<string>(() => null, step);
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNullUntil(() => "", step));
		before = Now;
		Assert.ThrowsException<AssertFailedException>(
			() => GameAssert.IsNullUntil(() => Now - before > 5 * step ? "" : null, 10 * step)
		);

		// IsNotNullUntil
		GameAssert.IsNotNullUntil(() => "", step);
		Assert.ThrowsException<AssertFailedException>(() => GameAssert.IsNotNullUntil<string>(() => null, step));
		before = Now;
		Assert.ThrowsException<AssertFailedException>(
			() => GameAssert.IsNotNullUntil(() => Now - before > 5 * step ? null : "", 10 * step)
		);
	}
}
