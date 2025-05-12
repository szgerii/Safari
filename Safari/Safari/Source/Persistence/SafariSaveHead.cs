using Newtonsoft.Json;
using System.Collections.Generic;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveHead {
	/// <summary>
	/// The serialized version of the gamemodel / level state
	/// </summary>
	[JsonProperty]
	public string? GameCoreSerialized { get; set; }

	/// <summary>
	/// Useful information regarding this save
	/// </summary>
	[JsonProperty]
	public SaveMetadata? MetaData { get; set; }

	[JsonProperty]
	public List<SafariSaveNode>? SaveNodes { get; set; }

	[JsonProperty]
	public List<SafariSaveStaticNode>? StaticNodes { get; set; }

	[JsonConstructor]
	public SafariSaveHead() { }

	public SafariSaveHead(string gameCoreSerialized, SaveMetadata metadata, List<SafariSaveNode> saveNodes, List<SafariSaveStaticNode> staticNodes) {
		GameCoreSerialized = gameCoreSerialized;
		MetaData = metadata;
		SaveNodes = saveNodes;
		StaticNodes = staticNodes;
	}
}
