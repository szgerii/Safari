using Newtonsoft.Json;

namespace Safari.Source.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveHead {
	/// <summary>
	/// The serialized version of the gamemodel / level state
	/// </summary>
	[JsonProperty]
	public string GameCoreSerialized { get; set; }

	/// <summary>
	/// Useful information regarding this save
	/// </summary>
	[JsonProperty]
	public SaveMetadata MetaData { get; set; }

	[JsonConstructor]
	public SafariSaveHead() { }

	public SafariSaveHead(string gameCoreSerialized, SaveMetadata metadata) {
		GameCoreSerialized = gameCoreSerialized;
		MetaData = metadata;
	}
}
