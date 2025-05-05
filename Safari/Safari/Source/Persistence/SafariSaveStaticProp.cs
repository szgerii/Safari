using Newtonsoft.Json;

namespace Safari.Persistence;

[JsonObject(MemberSerialization.OptIn)]
public class SafariSaveStaticProp {
	[JsonProperty]
	public string FullTypeName { get; set; }

	[JsonProperty]
	public string PropName { get; set; }

	[JsonProperty]
	public bool IsProp { get; set; }

	[JsonProperty]
	public string PropSerialized { get; set; }

	[JsonConstructor]
	public SafariSaveStaticProp() { }

	public SafariSaveStaticProp(string fullTypeName, string propName, bool isProp, string propSerialized) {
		FullTypeName = fullTypeName;
		PropName = propName;
		IsProp = isProp;
		PropSerialized = propSerialized;
	}
}
