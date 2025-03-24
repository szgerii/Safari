using System.Collections.Generic;

namespace Engine.Debug;

public class PerformanceCalculator {
	public int Capacity { get; init; }
	public int Count { get; private set; } = 0;
	public double Sum { get; private set; } = 0;

	public double Average => Sum / Count;
	public double Max { get; private set; } = 0;
	private int maxLifetime = 0;

	private readonly Queue<double> values;

	public PerformanceCalculator(int capacity) {
		values = new Queue<double>(capacity);
		Capacity = capacity;
	}

	public void AddValue(double value) {
		Sum += value;

		if (Count == Capacity) {
			Sum -= values.Dequeue();
		} else {
			Count++;
		}

		if (value > Max) {
			Max = value;
			maxLifetime = Capacity;
		}

		maxLifetime--;
		if (maxLifetime == 0) {
			Max = 0;
		}

		values.Enqueue(value);
	}

	public void Clear() {
		Sum = 0;
		Count = 0;
		Max = 0;
		maxLifetime = 0;
		values.Clear();
	}
}