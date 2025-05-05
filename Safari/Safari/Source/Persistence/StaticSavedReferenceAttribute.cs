using System;

namespace Safari.Persistence;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
public class StaticSavedReferenceAttribute : Attribute { }
