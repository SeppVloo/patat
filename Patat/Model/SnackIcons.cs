namespace Patat.Model;

/// <summary>Hand-drawn SVG icons (viewBox 0 0 64 64) for Dutch snacks, plus a name-based guess for new snacks.</summary>
public static class SnackIcons
{
    public record IconInfo(string Key, string Title, string Svg);

    private const string Crumbs = "<g fill='#8a4b12' opacity='.45'><circle cx='20' cy='30' r='1.2'/><circle cx='28' cy='35' r='1'/><circle cx='36' cy='29' r='1.2'/><circle cx='44' cy='34' r='1'/><circle cx='24' cy='38' r='.9'/><circle cx='40' cy='38' r='.9'/><circle cx='32' cy='31' r='.8'/></g>";

    public static readonly IReadOnlyList<IconInfo> All =
    [
        new("patat", "Patat",
            "<path d='M20 26 L16 12 M25 25 L23 8 M31 25 L31 6 M37 25 L40 9 M43 26 L48 13' stroke='#f4b400' stroke-width='5' stroke-linecap='round'/>" +
            "<path d='M20 26 L16 12 M31 25 L31 6 M43 26 L48 13' stroke='#e09a00' stroke-width='2' stroke-linecap='round' opacity='.5'/>" +
            "<path d='M12 24 H52 L46 58 H18 Z' fill='#d62f2f'/><path d='M12 24 H52 L51 30 H13 Z' fill='#b71f1f'/>" +
            "<path d='M24 36 h16 M26 44 h12' stroke='#fff' stroke-width='3' stroke-linecap='round' opacity='.85'/>" +
            "<ellipse cx='42' cy='22' rx='7' ry='4' fill='#fffbe8' stroke='#efe3b0'/>"),
        new("frikandel", "Frikandel",
            "<rect x='6' y='26' width='52' height='13' rx='6.5' fill='#7a3f1d'/>" +
            "<rect x='9' y='28' width='46' height='4' rx='2' fill='#a35a2c' opacity='.8'/>" +
            "<path d='M18 34 q2 2 4 0 M30 35 q2 2 4 0 M42 34 q2 2 4 0' stroke='#5b2d13' stroke-width='1.2' fill='none'/>"),
        new("frikandel-speciaal", "Frikandel speciaal",
            "<path d='M6 44 Q32 52 58 44 L56 48 Q32 58 8 48 Z' fill='#f1f1f1' stroke='#ddd'/>" +
            "<rect x='6' y='26' width='52' height='13' rx='6.5' fill='#7a3f1d'/>" +
            "<path d='M10 29 q4 -4 8 0 t8 0 t8 0 t8 0 t8 0' stroke='#fffbe8' stroke-width='3.2' fill='none' stroke-linecap='round'/>" +
            "<path d='M12 33 q4 -3 8 0 t8 0 t8 0 t8 0 t6 0' stroke='#c0392b' stroke-width='2.4' fill='none' stroke-linecap='round'/>" +
            "<g fill='#f7f3e3' stroke='#c9bfa0' stroke-width='.6'><rect x='16' y='22' width='3' height='3' rx='.6'/><rect x='27' y='21' width='3' height='3' rx='.6'/><rect x='38' y='22' width='3' height='3' rx='.6'/><rect x='46' y='21' width='3' height='3' rx='.6'/></g>"),
        new("kroket", "Kroket",
            "<rect x='8' y='22' width='48' height='20' rx='10' fill='#c47a2c'/>" +
            "<rect x='11' y='24' width='42' height='6' rx='3' fill='#dd9a4a' opacity='.9'/>" + Crumbs +
            "<ellipse cx='50' cy='46' rx='6' ry='3' fill='#e3b400'/>"),
        new("kipcorn", "Kipcorn",
            "<path d='M6 33 Q8 24 20 24 H48 Q58 26 58 33 Q58 40 48 42 H20 Q8 42 6 33 Z' fill='#e08a1e'/>" +
            "<g fill='#f6c15a'><path d='M14 28 l4 -2 l2 3 z'/><path d='M24 27 l5 -1 l1 4 z'/><path d='M35 28 l4 -2 l2 4 z'/><path d='M45 28 l4 -1 l1 3 z'/><path d='M18 36 l4 1 l-1 3 z'/><path d='M30 36 l5 0 l-2 3 z'/><path d='M42 36 l4 1 l-2 3 z'/></g>" +
            "<g fill='#b4630f'><path d='M20 32 l3 1 l-2 2 z'/><path d='M38 32 l3 1 l-2 2 z'/></g>"),
        new("kaassouffle", "Kaassoufflé",
            "<path d='M10 22 H54 Q57 22 57 25 V41 Q57 44 54 44 H10 Q7 44 7 41 V25 Q7 22 10 22 Z' fill='#e9b44c'/>" +
            "<path d='M9 24 H55 V30 H9 Z' fill='#f5cf75' opacity='.8'/>" +
            "<path d='M7 26 l2 2 l-2 2 l2 2 l-2 2 l2 2 l-2 2 l2 2 M57 26 l-2 2 l2 2 l-2 2 l2 2 l-2 2 l2 2 l-2 2' stroke='#c98f2a' stroke-width='1.2' fill='none'/>" +
            "<path d='M24 44 q2 8 5 0 q3 6 5 0' fill='#ffd84a' stroke='#e8b800' stroke-width='.8'/>"),
        new("bitterbal", "Bitterbal",
            "<ellipse cx='32' cy='50' rx='26' ry='6' fill='#ececec'/>" +
            "<circle cx='20' cy='38' r='11' fill='#9a5521'/><circle cx='44' cy='38' r='11' fill='#9a5521'/><circle cx='32' cy='28' r='11' fill='#ad6127'/>" +
            "<circle cx='17' cy='34' r='3' fill='#c47a3c' opacity='.7'/><circle cx='41' cy='34' r='3' fill='#c47a3c' opacity='.7'/><circle cx='29' cy='24' r='3' fill='#d08a48' opacity='.7'/>" +
            "<ellipse cx='54' cy='50' rx='5' ry='3' fill='#e3b400'/>"),
        new("berenklauw", "Berenklauw",
            "<path d='M6 32 H58' stroke='#c9a46b' stroke-width='2.5' stroke-linecap='round'/>" +
            "<g fill='#8b3d1b'><rect x='12' y='22' width='8' height='20' rx='3'/><rect x='28' y='22' width='8' height='20' rx='3'/><rect x='44' y='22' width='8' height='20' rx='3'/></g>" +
            "<g fill='none' stroke='#f3e3c3' stroke-width='3'><ellipse cx='24' cy='32' rx='3' ry='9'/><ellipse cx='40' cy='32' rx='3' ry='9'/></g>"),
        new("loempia", "Loempia",
            "<rect x='6' y='22' width='52' height='20' rx='8' fill='#d99a3e'/>" +
            "<path d='M14 22 L22 42 M26 22 L34 42 M38 22 L46 42 M50 22 L56 34' stroke='#b9772a' stroke-width='2'/>" +
            "<rect x='9' y='24' width='46' height='5' rx='2.5' fill='#efc06c' opacity='.7'/>"),
        new("mexicano", "Mexicano",
            "<path d='M6 34 Q6 26 14 26 H50 Q58 26 58 34 Q58 42 50 42 H14 Q6 42 6 34 Z' fill='#6e2a16'/>" +
            "<g fill='#d63a1f'><circle cx='16' cy='31' r='1.4'/><circle cx='26' cy='36' r='1.2'/><circle cx='34' cy='30' r='1.4'/><circle cx='44' cy='36' r='1.2'/><circle cx='50' cy='31' r='1.1'/></g>" +
            "<g fill='#e8c34a'><circle cx='21' cy='37' r='1'/><circle cx='39' cy='32' r='1'/></g>" +
            "<path d='M50 18 q6 -2 6 6 q-4 -2 -6 -6 z' fill='#2e8b3a'/><path d='M44 20 q4 -4 10 2 q-2 6 -10 -2 z' fill='#d63a1f'/>"),
        new("viandel", "Viandel",
            "<rect x='6' y='25' width='52' height='15' rx='7.5' fill='#b8692a'/>" +
            "<rect x='9' y='27' width='46' height='5' rx='2.5' fill='#d38a45' opacity='.85'/>" + Crumbs),
        new("bamischijf", "Bamischijf",
            "<ellipse cx='32' cy='38' rx='24' ry='10' fill='#b5651d'/><ellipse cx='32' cy='34' rx='24' ry='10' fill='#e0913a'/>" +
            "<path d='M16 33 q6 -5 12 0 t12 0 t10 0 M18 37 q6 -4 12 0 t12 0' stroke='#f4c06a' stroke-width='1.8' fill='none'/>"),
        new("nasibal", "Nasibal",
            "<ellipse cx='32' cy='54' rx='20' ry='4' fill='#ececec'/>" +
            "<circle cx='32' cy='34' r='18' fill='#d9822b'/><circle cx='26' cy='28' r='6' fill='#eba04c' opacity='.7'/>" +
            "<g fill='#f6e3b4'><ellipse cx='24' cy='38' rx='1.6' ry='.8'/><ellipse cx='36' cy='28' rx='1.6' ry='.8'/><ellipse cx='40' cy='40' rx='1.6' ry='.8'/><ellipse cx='30' cy='44' rx='1.6' ry='.8'/></g>"),
        new("kipnuggets", "Kipnuggets",
            "<path d='M8 36 q2 -10 12 -9 q8 -1 10 7 q1 9 -10 10 q-11 1 -12 -8 z' fill='#e2a046'/>" +
            "<path d='M30 30 q3 -9 13 -8 q10 1 11 9 q0 10 -12 10 q-11 -1 -12 -11 z' fill='#d48c32'/>" +
            "<path d='M20 46 q2 -6 9 -5 q7 1 7 6 q-1 6 -8 6 q-8 0 -8 -7 z' fill='#e9ad55'/>" +
            "<g fill='#f6cf86' opacity='.8'><circle cx='16' cy='33' r='2'/><circle cx='40' cy='27' r='2.2'/><circle cx='27' cy='45' r='1.6'/></g>"),
        new("burger", "Burger",
            "<path d='M10 30 Q10 12 32 12 Q54 12 54 30 Z' fill='#e2a24a'/>" +
            "<g fill='#fff6dc'><ellipse cx='24' cy='20' rx='1.6' ry='.8'/><ellipse cx='34' cy='17' rx='1.6' ry='.8'/><ellipse cx='42' cy='22' rx='1.6' ry='.8'/></g>" +
            "<path d='M8 32 H56 L52 36 H12 Z' fill='#5fae3a'/><rect x='9' y='35' width='46' height='8' rx='4' fill='#6b3216'/>" +
            "<path d='M10 43 H54 V46 Q54 52 46 52 H18 Q10 52 10 46 Z' fill='#d8933e'/>"),
        new("saus", "Saus",
            "<path d='M16 30 H48 L44 54 H20 Z' fill='#ffffff' stroke='#d9d9d9' stroke-width='2'/>" +
            "<path d='M18 30 Q24 18 32 24 Q40 16 46 30 Z' fill='#fffbe8' stroke='#efe3b0'/>"),
        new("snack", "Overig",
            "<ellipse cx='32' cy='44' rx='26' ry='8' fill='#ececec'/><ellipse cx='32' cy='42' rx='20' ry='5' fill='#fafafa'/>" +
            "<rect x='14' y='30' width='36' height='12' rx='6' fill='#c47a2c'/>"),
    ];

    private static readonly Dictionary<string, IconInfo> ByKey = All.ToDictionary(i => i.Key);

    public static IconInfo? Find(string? key) => key is not null && ByKey.TryGetValue(key, out var i) ? i : null;

    public const string EmojiKey = "emoji";

    /// <summary>Icon for a snack: its chosen icon, else a guess from the name, else the generic icon. Null = show the emoji.</summary>
    public static IconInfo? For(Snack s) =>
        s.Icon == EmojiKey ? null : Find(s.Icon) ?? Find(Guess(s.Name)) ?? Find("snack");

    private static readonly (string[] Words, string Key)[] Rules =
    [
        (["speciaal", "spesiaal"], "frikandel-speciaal"),
        (["mexicano"], "mexicano"),
        (["viandel"], "viandel"),
        (["frikandel", "frikadel"], "frikandel"),
        (["kroket", "croquet"], "kroket"),
        (["kipcorn"], "kipcorn"),
        (["nugget", "kipstick", "kipkorn"], "kipnuggets"),
        (["kaas"], "kaassouffle"),
        (["bitterbal"], "bitterbal"),
        (["berenklauw", "sate", "saté", "spies"], "berenklauw"),
        (["loempia", "springroll", "kiploempia"], "loempia"),
        (["bami"], "bamischijf"),
        (["nasi"], "nasibal"),
        (["burger"], "burger"),
        (["saus", "mayo", "ketchup", "curry", "pinda"], "saus"),
        (["patat", "friet", "frites"], "patat"),
    ];

    /// <summary>Best matching icon key for a snack name, or "" if nothing matches.</summary>
    public static string Guess(string? name)
    {
        var n = (name ?? "").ToLowerInvariant();
        foreach (var (words, key) in Rules)
            if (words.Any(n.Contains)) return key;
        return "";
    }
}
