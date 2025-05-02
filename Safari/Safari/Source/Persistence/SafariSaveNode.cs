using Newtonsoft.Json;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveNode {
	[JsonProperty]
	public string FullTypeName { get; set; }
	[JsonProperty]
	public string GameObjectSerialized { get; set; }

	[JsonConstructor]
	public SafariSaveNode() { }

	public SafariSaveNode(string fullTypeName, string gameObjectSerialized) {
		FullTypeName = fullTypeName;
		GameObjectSerialized = gameObjectSerialized;
	}
}
