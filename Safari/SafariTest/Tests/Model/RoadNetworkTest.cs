using Microsoft.Xna.Framework;
using NSubstitute;
using Safari.Model;

namespace SafariTest.Tests.Model;

[TestClass]
public class RoadNetworkTest {

	[TestInitialize]
	public void InitRandom() {
		Safari.Game.Random = NSubstitute.Substitute.For<Random>();
		Safari.Game.Random.Next().ReturnsForAnyArgs(0);
	}

	[TestMethod("Roadnetwork init, bounds")]
	public void RoadNetworkInit() {
		RoadNetwork network = new RoadNetwork(4, 4, new(0, 0), new(3, 0));
		Assert.AreEqual(network.Start, new(0, 0));
		Assert.AreEqual(network.End, new(3, 0));
		// In bounds operations
		bool exceptionThrown = false;
		try {
			network.AddRoad(new(1, 0));
			network.GetRoad(new(1, 0));
			network.ClearRoad(new(1, 0));
		} catch {
			exceptionThrown = true;
		}
		Assert.IsFalse(exceptionThrown);
		// Out of bounds operations

		exceptionThrown = false;
		try {
			network.AddRoad(new(4, 0));
		} catch {
			exceptionThrown = true;
		}
		Assert.IsTrue(exceptionThrown);

		exceptionThrown = false;
		try {
			network.AddRoad(new(-1, 0));
		} catch {
			exceptionThrown = true;
		}
	}

	[TestMethod("Roadnetwork editing & fetching roads")]
	public void RoadNetworkEdit() {
		RoadNetwork network = new RoadNetwork(4, 4, new(0, 0), new(3, 0));

		// Add a road, check it, then remove it
		Point p = new(0, 1);
		Assert.IsFalse(network.GetRoad(p));
		network.AddRoad(p);
		Assert.IsTrue(network.GetRoad(p));
		network.ClearRoad(p);
		Assert.IsFalse(network.GetRoad(p));

		// Check event firing / not firing
		bool eventFired = false;
		bool changeType = false;
		network.RoadChanged += (object? sender, RoadChangedEventArgs e) => {
			eventFired = true;
			changeType = e.ChangeType;
		};
		Point p2 = new(0, 2);
		network.ClearRoad(p2);
		eventFired = false;
		changeType = false;
		Assert.IsFalse(eventFired);
		eventFired = false;
		changeType = false;
		network.AddRoad(p2);
		Assert.IsTrue(eventFired);
		Assert.IsTrue(changeType); // true means a new road was built
		eventFired = false;
		changeType = false;
		network.AddRoad(p2);
		Assert.IsFalse(eventFired);
		eventFired = false;
		changeType = false;
		network.ClearRoad(p2);
		Assert.IsTrue(eventFired);
		Assert.IsFalse(changeType); // false means a road was demolished
	}

	[TestMethod("Fetch a return route (from end to start), shortest if possible")]
	public void FetchReturnRoute() {
		RoadNetwork network = new RoadNetwork(3, 3, new(0, 0), new(2, 0));
		Assert.AreEqual(0, network.ReturnRoute.Count); // Count=0 means no route was found
		network.AddRoad(new(1, 0));
		List<Point> shortReturnRoute = new List<Point> { new(2, 0), new(1, 0), new(0, 0)};
		List<Point> longReturnRoute = new List<Point> { new(2, 0), new(2, 1), new(1, 1), new(0, 1), new(0, 0)};
		CollectionAssert.AreEqual(shortReturnRoute, network.ReturnRoute); // we find the route
		network.ClearRoad(new(1, 0));
		Assert.AreEqual(0, network.ReturnRoute.Count); // no route, because we deleted it
		network.AddRoad(new(0, 1));
		network.AddRoad(new(1, 1));
		network.AddRoad(new(2, 1));
		CollectionAssert.AreEqual(longReturnRoute, network.ReturnRoute); // we find the long route
		network.AddRoad(new(1, 0));
		CollectionAssert.AreEqual(shortReturnRoute, network.ReturnRoute); // shortest route is preferred
	}

	[TestMethod("Fetch a path in the network")]
	public void FetchPath() {
		RoadNetwork network = new RoadNetwork(3, 3, new(0, 0), new(2, 2));
		Point from = new(0, 1);
		Point to = new(2, 0);
		Assert.AreEqual(0, network.GetPath(from, to).Count); // no path
		network.AddRoad(new(0, 1));
		network.AddRoad(new(2, 0));
		Assert.AreEqual(0, network.GetPath(from, to).Count); // no path
		network.AddRoad(new(1, 1));
		network.AddRoad(new(2, 1));
		List<Point> path = new List<Point> {new(0, 1), new(1, 1), new(2, 1), new(2, 0) };
		CollectionAssert.AreEqual(path, network.GetPath(from, to)); // found the path
		network.ClearRoad(new(1, 1));
		Assert.AreEqual(0, network.GetPath(from, to).Count);
	}

	[TestMethod("Fetch all routes, and a random route, in the network (routes used by jeeps)")]
	public void FetchRoutes() {
		RoadNetwork network = new RoadNetwork(3, 3, new(0, 0), new(2, 2));
		Assert.AreEqual(0, network.Routes.Count); // no routes
		Assert.AreEqual(0, network.RandomRoute.Count); // no routes

		List<Point> route1 = new List<Point>() { new(0, 0), new(1, 0), new(2, 0), new(2, 1), new(2, 2) };
		List<Point> route2 = new List<Point>() { new(0, 0), new(0, 1), new(0, 2), new(1, 2), new(2, 2) };

		network.AddRoad(new(1, 0));
		network.AddRoad(new(2, 0));
		network.AddRoad(new(2, 1));
		Assert.AreEqual(1, network.Routes.Count);
		CollectionAssert.AreEqual(route1, network.Routes[0]); // route gets found
		CollectionAssert.AreEqual(route1, network.RandomRoute); // and is returned (random is mocked to give 0)

		network.AddRoad(new(0, 1));
		network.AddRoad(new(0, 2));
		network.AddRoad(new(1, 2));
		Assert.AreEqual(2, network.Routes.Count);
		CollectionAssert.AreEqual(route1, network.Routes[0]);
		CollectionAssert.AreEqual(route2, network.Routes[1]); // both routes get found
		CollectionAssert.AreEqual(route1, network.RandomRoute); // and the first one is returned
	}
}
