using Newtonsoft.Json;
using System.Collections.Generic;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveNode {
	[JsonProperty]
	public string? FullTypeName { get; set; }

	[JsonProperty]
	public string? GameObjectSerialized { get; set; }

	[JsonProperty]
	public int GameObjectID { get; set; }

	[JsonProperty]
	public List<SafariSaveRefNode>? Refs { get; set; }

	[JsonConstructor]
	public SafariSaveNode() { }

	public SafariSaveNode(string fullTypeName, string gameObjectSerialized, int gameObjectID, List<SafariSaveRefNode> refs) {
		FullTypeName = fullTypeName;
		GameObjectSerialized = gameObjectSerialized;
		GameObjectID = gameObjectID;
		Refs = refs;
	}
}
