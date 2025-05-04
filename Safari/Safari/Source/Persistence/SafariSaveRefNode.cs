using Newtonsoft.Json;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveRefNode {
	[JsonProperty]
	public string Name { get; set; }

	[JsonProperty]
	public string ContentSerialized { get; set; }

	[JsonConstructor]
	public SafariSaveRefNode() { }

	public SafariSaveRefNode(string name, string contentSerialized) {
		Name = name;
		ContentSerialized = contentSerialized;
	}
}
