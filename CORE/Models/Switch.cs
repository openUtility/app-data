using System.Text.Json.Serialization;

namespace app_data_core.Models;

public class Switch {

    public Switch(string Name) => (this.Name) = (Name);

    public Switch(string Name,
                  bool Value) : this(Name) => (this.Value) = (Value);

    public Switch(string Name,
                  string Tag,
                  bool Value) : this(Name) {
        this.Tags = new Dictionary<string, bool>();
        this.Tags.Add(Tag, Value);
    }
    
    public string Name { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IDictionary<string, bool>? Tags { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Value { get; set; }
}
