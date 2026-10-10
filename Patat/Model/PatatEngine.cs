using System.Text.Json;

namespace Patat.Model;

public class Snack
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Emoji { get; set; } = "🍴";
    /// <summary>Key of a drawn icon in <see cref="SnackIcons"/>; empty = guess from the name, else fall back to Emoji.</summary>
    public string Icon { get; set; } = "";
    public string Name { get; set; } = "";
    public int Stock { get; set; }
    public int FryMinutes { get; set; } = 4;
    /// <summary>Number of pieces in one package; used to add a whole pack to the stock at once.</summary>
    public int PackSize { get; set; } = 1;
    /// <summary>Snacks with the same (non-empty) group can go into the fryer together.</summary>
    public string FryGroup { get; set; } = "";
    /// <summary>Barcodes of packages of this snack (several brands/pack sizes possible); scanning one adds that pack.</summary>
    public List<Barcode> Barcodes { get; set; } = [];
}

[System.Text.Json.Serialization.JsonConverter(typeof(BarcodeConverter))]
public class Barcode
{
    public string Code { get; set; } = "";
    /// <summary>Pieces in a package with this code.</summary>
    public int PackSize { get; set; } = 1;
    /// <summary>Optional brand/product name, e.g. from Open Food Facts.</summary>
    public string Label { get; set; } = "";
    /// <summary>Product photo URL (Open Food Facts).</summary>
    public string Image { get; set; } = "";
    /// <summary>Calories per piece; 0 = unknown.</summary>
    public int Kcal { get; set; }
    /// <summary>Nutri-Score A-E; empty = unknown.</summary>
    public string Nutri { get; set; } = "";
    /// <summary>Package contents as printed, e.g. "10 x 70 g".</summary>
    public string Qty { get; set; } = "";
}

public class Order
{
    public string Person { get; set; } = "";
    public Dictionary<string, int> Items { get; set; } = [];
    public string Note { get; set; } = "";
    /// <summary>Unique per submit, so a client can see its latest version has arrived.</summary>
    public string Stamp { get; set; } = "";
    public DateTime Time { get; set; } = DateTime.UtcNow;
}

public class Batch
{
    public string Key { get; set; } = "";
    public DateTime? StartedUtc { get; set; }
    public bool Done { get; set; }
}

public class Round
{
    public DateTime Time { get; set; } = DateTime.UtcNow;
    public List<Order> Orders { get; set; } = [];
    /// <summary>Snack id -> name at the time of the round, so history stays readable after renames/removals.</summary>
    public Dictionary<string, string> Names { get; set; } = [];
}

public class PatatState
{
    /// <summary>Incremented on every change by the baker; the highest version wins when devices meet.</summary>
    public long Version { get; set; }
    public bool Open { get; set; } = true;
    /// <summary>Announce the family code on the baker's wifi network so new devices there can join.</summary>
    public bool WifiJoin { get; set; } = true;
    public List<Snack> Snacks { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public List<Batch> Batches { get; set; } = [];
    /// <summary>Order stamp -> reason, so a remote device learns why its order was refused.</summary>
    public Dictionary<string, string> Rejected { get; set; } = [];
    /// <summary>Finished rounds of the last year.</summary>
    public List<Round> History { get; set; } = [];
    /// <summary>Fry groups in the order the baker wants them shown (unlisted groups follow alphabetically).</summary>
    public List<string> GroupOrder { get; set; } = ["Patat"];
}

public record PlannedBatch(string Key, string Title, int Minutes, List<(Snack Snack, int Count)> Items);

public class PatatEngine
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public PatatState State { get; private set; } = new() { Snacks = DefaultSnacks() };

    public static List<Snack> DefaultSnacks() =>
    [
        new() { Id = "patat", Icon = "patat", Emoji = "🍟", Name = "Patat (portie)", Stock = 6, FryMinutes = 6, FryGroup = "Patat", PackSize = 4 },
        new() { Id = "frikandel", Icon = "frikandel", Emoji = "🌭", Name = "Frikandel", Stock = 10, FryMinutes = 4, FryGroup = "Vlees", PackSize = 10 },
        new() { Id = "kroket", Icon = "kroket", Emoji = "🥖", Name = "Kroket", Stock = 6, FryMinutes = 4, FryGroup = "Vlees", PackSize = 6 },
        new() { Id = "kipcorn", Icon = "kipcorn", Emoji = "🍗", Name = "Kipcorn", Stock = 4, FryMinutes = 5, FryGroup = "Vlees", PackSize = 4 },
        new() { Id = "kaassouffle", Icon = "kaassouffle", Emoji = "🧀", Name = "Kaassoufflé", Stock = 4, FryMinutes = 3, FryGroup = "Kaas", PackSize = 4 },
        new() { Id = "bitterbal", Icon = "bitterbal", Emoji = "🟤", Name = "Bitterbal", Stock = 20, FryMinutes = 4, FryGroup = "", PackSize = 20 },
    ];

    public string ToJson() => JsonSerializer.Serialize(State, Json);

    public void Reset() => State = new() { Snacks = DefaultSnacks() };

    /// <summary>Takes over the given state if it is newer than the current one. Returns true if adopted.</summary>
    public bool TryAdopt(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            var other = JsonSerializer.Deserialize<PatatState>(json, Json);
            if (other is null || other.Version <= State.Version) return false;
            State = other;
            return true;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException) { return false; }
    }

    /// <summary>Returns false if the json is present but unreadable (state is then left untouched).</summary>
    public bool Load(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            State = JsonSerializer.Deserialize<PatatState>(json, Json) ?? State;
            foreach (var s in State.Snacks)
                foreach (var b in s.Barcodes.Where(b => b.PackSize < 1)) b.PackSize = Math.Max(1, s.PackSize);
            return true;
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException) { return false; }
    }

    /// <summary>All fry groups in display order: the baker's order first, then the rest alphabetically, ungrouped last.</summary>
    public List<string> Groups()
    {
        var used = State.Snacks.Select(s => s.FryGroup?.Trim() ?? "").Where(g => g != "").Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var ordered = State.GroupOrder.Where(g => used.Contains(g, StringComparer.OrdinalIgnoreCase)).ToList();
        ordered.AddRange(used.Where(g => !ordered.Contains(g, StringComparer.OrdinalIgnoreCase)).Order(StringComparer.CurrentCultureIgnoreCase));
        return ordered;
    }

    /// <summary>Moves a group one place up (-1) or down (+1) in the display order.</summary>
    public void MoveGroup(string group, int delta)
    {
        var list = Groups();
        var i = list.FindIndex(g => string.Equals(g, group, StringComparison.OrdinalIgnoreCase));
        var j = i + delta;
        if (i < 0 || j < 0 || j >= list.Count) return;
        (list[i], list[j]) = (list[j], list[i]);
        State.GroupOrder = list;
    }

    /// <summary>Snacks sorted: available first, then by group order, then alphabetically.</summary>
    public List<Snack> Sorted(Func<Snack, bool> available)
    {
        var groups = Groups();
        int Rank(Snack s)
        {
            var i = groups.FindIndex(g => string.Equals(g, s.FryGroup?.Trim(), StringComparison.OrdinalIgnoreCase));
            return i < 0 ? int.MaxValue : i;
        }
        return State.Snacks.OrderBy(s => available(s) ? 0 : 1).ThenBy(Rank)
            .ThenBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    public Snack? Find(string id) => State.Snacks.FirstOrDefault(s => s.Id == id);

    public (Snack Snack, Barcode Code)? ByBarcode(string code) =>
        State.Snacks.SelectMany(s => s.Barcodes.Select(b => ((Snack Snack, Barcode Code)?)(s, b))).FirstOrDefault(x => SameCode(x!.Value.Code.Code, code));

    /// <summary>Same product code, ignoring non-digits and leading zeros (UPC-A 12 digits = EAN-13 with a leading 0).</summary>
    public static bool SameCode(string a, string b)
    {
        static string N(string c)
        {
            var d = new string(c.Where(char.IsDigit).ToArray()).TrimStart('0');
            return d == "" ? c.Trim() : d;
        }
        return N(a) == N(b);
    }

    /// <summary>Snack whose name (or icon) best matches a product description, e.g. from Open Food Facts.</summary>
    public Snack? MatchSnack(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var t = text.ToLowerInvariant();
        var byName = State.Snacks
            .Select(s => (s, words: s.Name.ToLowerInvariant().Split([' ', '(', ')', '-', ','], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length >= 4).ToList()))
            .Select(x => (x.s, score: x.words.Count(w => t.Contains(w) || t.Contains(w.TrimEnd('s')))))
            .Where(x => x.score > 0)
            .OrderByDescending(x => x.score).ThenByDescending(x => x.s.Name.Length)
            .Select(x => x.s).FirstOrDefault();
        if (byName is not null) return byName;
        var icon = SnackIcons.Guess(t);
        return icon == "" ? null : State.Snacks.FirstOrDefault(s => SnackIcons.Guess(s.Name) == icon || s.Icon == icon);
    }

    /// <summary>Number of pieces in a pack from a product text like "10 stuks", "10 x 70 g", "8st"; 0 if unknown.</summary>
    public static int GuessPack(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        var t = text.ToLowerInvariant();
        foreach (var pattern in new[] { @"(\d{1,3})\s*(?:stuks|stuk|st\b|pcs|pieces|x\s*\d)", @"(\d{1,3})\s*(?:frikandellen|kroketten|snacks|bitterballen|kipcorns)" })
        {
            var m = System.Text.RegularExpressions.Regex.Match(t, pattern);
            if (m.Success && int.TryParse(m.Groups[1].Value, out var n) && n is > 0 and <= 200) return n;
        }
        return 0;
    }

    public Order? OrderOf(string person) =>
        State.Orders.FirstOrDefault(o => string.Equals(o.Person, person, StringComparison.OrdinalIgnoreCase));

    /// <summary>How many of a snack this person could order (stock plus what he already reserved).</summary>
    public int MaxFor(string snackId, string person) =>
        (Find(snackId)?.Stock ?? 0) + (OrderOf(person)?.Items.GetValueOrDefault(snackId) ?? 0);

    /// <summary>Places or replaces the order of a person. Stock is reserved immediately. Returns an error or null.</summary>
    public string? PlaceOrder(Order order)
    {
        if (!State.Open) return "De bestellingen zijn gesloten.";
        order.Person = order.Person.Trim();
        if (order.Person.Length == 0) return "Vul je naam in.";
        order.Items = order.Items.Where(kv => kv.Value > 0 && Find(kv.Key) is not null).ToDictionary();

        foreach (var (id, n) in order.Items)
            if (n > MaxFor(id, order.Person))
                return $"Niet genoeg {Find(id)!.Name} op voorraad (nog {MaxFor(id, order.Person)}).";

        Cancel(order.Person);
        if (order.Items.Count == 0) return null;
        foreach (var (id, n) in order.Items) Find(id)!.Stock -= n;
        order.Time = DateTime.UtcNow;
        State.Orders.Add(order);
        ResetBatchesFor(order.Items.Keys);
        return null;
    }

    public void Cancel(string person)
    {
        var old = OrderOf(person);
        if (old is null) return;
        foreach (var (id, n) in old.Items)
            if (Find(id) is { } s) s.Stock += n;
        State.Orders.Remove(old);
    }

    /// <summary>New round: orders are cleared, stock stays as it is (already used).</summary>
    public void Finish()
    {
        if (State.Orders.Count > 0)
            State.History.Add(new Round
            {
                Orders = [.. State.Orders],
                Names = State.Snacks.ToDictionary(s => s.Id, s => s.Name),
            });
        PruneHistory();
        State.Orders.Clear();
        State.Batches.Clear();
        State.Rejected.Clear();
    }

    public Dictionary<string, int> Totals()
    {
        var t = new Dictionary<string, int>();
        foreach (var o in State.Orders)
            foreach (var (id, n) in o.Items)
                t[id] = t.GetValueOrDefault(id) + n;
        return t;
    }

    public void PruneHistory() => State.History.RemoveAll(r => r.Time < DateTime.UtcNow.AddYears(-1));

    /// <summary>Only snacks of the same fry group AND the same fry time can share a basket.</summary>
    public static string GroupKey(Snack s) => string.IsNullOrWhiteSpace(s.FryGroup) ? "#" + s.Id : s.FryGroup.Trim().ToLowerInvariant() + "@" + s.FryMinutes;

    /// <summary>Batches for the fryer: snacks of the same group together, longest batch first.</summary>
    public List<PlannedBatch> Plan()
    {
        var totals = Totals();
        return State.Snacks
            .Where(s => totals.GetValueOrDefault(s.Id) > 0)
            .GroupBy(GroupKey)
            .Select(g => new PlannedBatch(
                g.Key,
                string.IsNullOrWhiteSpace(g.First().FryGroup) ? g.First().Name : g.First().FryGroup.Trim(),
                g.Max(s => s.FryMinutes),
                g.Select(s => (s, totals[s.Id])).ToList()))
            .OrderByDescending(b => b.Minutes)
            .ToList();
    }

    public Batch BatchState(string key)
    {
        var b = State.Batches.FirstOrDefault(x => x.Key == key);
        if (b is null) State.Batches.Add(b = new Batch { Key = key });
        return b;
    }

    private void ResetBatchesFor(IEnumerable<string> snackIds)
    {
        var keys = snackIds.Select(Find).OfType<Snack>().Select(GroupKey).ToHashSet();
        State.Batches.RemoveAll(b => keys.Contains(b.Key) && b.Done);
    }
}

/// <summary>Reads barcodes saved as plain strings (older versions) as well as objects.</summary>
public class BarcodeConverter : System.Text.Json.Serialization.JsonConverter<Barcode>
{
    public override Barcode? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String) return new Barcode { Code = reader.GetString() ?? "", PackSize = 0 };
        using var doc = JsonDocument.ParseValue(ref reader);
        var r = doc.RootElement;
        string Str(string n) => r.TryGetProperty(n, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
        var pack = r.TryGetProperty("packSize", out var p) && p.TryGetInt32(out var n) ? n : 1;
        var kcal = r.TryGetProperty("kcal", out var k) && k.TryGetInt32(out var kv) ? kv : 0;
        return new Barcode { Code = Str("code"), Label = Str("label"), PackSize = Math.Max(1, pack), Image = Str("image"), Kcal = kcal, Nutri = Str("nutri"), Qty = Str("qty") };
    }

    public override void Write(Utf8JsonWriter writer, Barcode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("code", value.Code);
        writer.WriteNumber("packSize", value.PackSize);
        writer.WriteString("label", value.Label);
        if (value.Image != "") writer.WriteString("image", value.Image);
        if (value.Kcal > 0) writer.WriteNumber("kcal", value.Kcal);
        if (value.Nutri != "") writer.WriteString("nutri", value.Nutri);
        if (value.Qty != "") writer.WriteString("qty", value.Qty);
        writer.WriteEndObject();
    }
}
