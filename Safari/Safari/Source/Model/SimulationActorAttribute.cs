using System;

namespace Safari.Model;

/// <summary>
/// Attribute for marking something as part of the game simulation, and synchronizing to its speed
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SimulationActorAttribute : Attribute { }
