using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safari.Persistence;

[AttributeUsage(AttributeTargets.Method)]	
public class PostPersistenceSetupAttribute : Attribute { }
