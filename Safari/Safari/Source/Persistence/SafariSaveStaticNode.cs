using Newtonsoft.Json;
using System.Collections.Generic;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveStaticNode {
	[JsonProperty]
	public string? FullTypeName { get; set; }

	[JsonProperty]
	public List<SafariSaveStaticProp>? Props { get; set; }

	[JsonProperty]
	public List<SafariSaveRefNode>? Refs { get; set; }

	[JsonConstructor]
	public SafariSaveStaticNode() { }

	public SafariSaveStaticNode(string fullTypeName, List<SafariSaveStaticProp> props, List<SafariSaveRefNode> refs) {
		FullTypeName = fullTypeName;
		Props = props;
		Refs = refs;
	}
}
