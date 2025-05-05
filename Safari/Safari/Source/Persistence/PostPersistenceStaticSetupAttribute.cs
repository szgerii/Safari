using System;

namespace Safari.Persistence;

[AttributeUsage(AttributeTargets.Method)]
public class PostPersistenceStaticSetupAttribute : Attribute { }
