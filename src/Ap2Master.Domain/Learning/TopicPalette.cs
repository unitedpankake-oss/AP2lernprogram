using Ap2Master.Domain;
using Ap2Master.Domain.Entities;

namespace Ap2Master.Domain.Learning;

/// <summary>
/// Stabile Farben je Prüfungsteil und Thema. Teil 1 blau, Teil 2 violett, WiSo warm —
/// innerhalb eines Teils bekommen Themen unterscheidbare Akzente, damit Karten nicht in einer Reihe verschwimmen.
/// </summary>
public static class TopicPalette
{
    private static readonly string[] Teil1 =
        ["#3B82F6", "#0EA5E9", "#06B6D4", "#2563EB", "#38BDF8", "#0284C7", "#22D3EE", "#1D4ED8"];
    private static readonly string[] Teil2 =
        ["#8B5CF6", "#A855F7", "#D946EF", "#EC4899", "#6366F1", "#7C3AED", "#C026D3", "#4F46E5"];
    private static readonly string[] WiSo =
        ["#F59E0B", "#F97316", "#EAB308", "#FB7185", "#F43F5E", "#D97706", "#EA580C", "#FBBF24"];

    public static string PartAccent(ExamPart part) => part switch
    {
        ExamPart.Teil1 => "#3B82F6",
        ExamPart.Teil2 => "#8B5CF6",
        ExamPart.WiSo => "#F59E0B",
        _ => "#64748B"
    };

    public static string LocKey(ExamPart part) => part switch
    {
        ExamPart.Teil1 => "Part.Teil1",
        ExamPart.Teil2 => "Part.Teil2",
        ExamPart.WiSo => "Part.WiSo",
        _ => "Cat.Title"
    };

    public static string TopicAccent(ExamPart part, string topicId)
    {
        var family = part switch
        {
            ExamPart.Teil1 => Teil1,
            ExamPart.Teil2 => Teil2,
            ExamPart.WiSo => WiSo,
            _ => Teil1
        };
        return family[StableIndex(topicId, family.Length)];
    }

    public static string Soft(string hex, byte alpha = 0x33)
    {
        var h = (hex ?? "").Trim().TrimStart('#');
        if (h.Length == 8) h = h[2..];
        return h.Length == 6 ? $"#{alpha:X2}{h}" : "#3364748B";
    }

    public static (string Accent, string Soft) ForTopic(ExamPart part, string topicId)
    {
        var accent = TopicAccent(part, topicId);
        return (accent, Soft(accent));
    }

    public static (string Accent, string Soft) ForPart(ExamPart part)
    {
        var accent = PartAccent(part);
        return (accent, Soft(accent, 0x28));
    }

    public static (string Accent, string Soft) ForActivity(ActivityKind kind, string topicId)
    {
        var part = kind switch
        {
            ActivityKind.WiSoSimulation => ExamPart.WiSo,
            ActivityKind.MockTeil2 or ActivityKind.SqlLab => ExamPart.Teil2,
            ActivityKind.MockTeil1 or ActivityKind.Algorithm => ExamPart.Teil1,
            _ => topicId.StartsWith("t3", StringComparison.OrdinalIgnoreCase) ? ExamPart.WiSo
                : topicId.StartsWith("t2", StringComparison.OrdinalIgnoreCase) ? ExamPart.Teil2
                : ExamPart.Teil1
        };
        var id = string.IsNullOrWhiteSpace(topicId) ? kind.ToString() : topicId;
        return ForTopic(part, id);
    }

    internal static int StableIndex(string id, int modulo)
    {
        if (modulo <= 0) return 0;
        unchecked
        {
            uint hash = 2166136261;
            foreach (var ch in id ?? "")
                hash = (hash ^ ch) * 16777619;
            return (int)(hash % (uint)modulo);
        }
    }
}

public readonly record struct TopicCardStats(int Total, int Due);

public sealed record TopicDeck(
    string Id,
    string Title,
    ExamPart Part,
    string AccentHex,
    string SoftHex,
    int TotalCards,
    int DueCards,
    int EstimatedMinutes,
    IReadOnlyList<string> ScopeIds);

public sealed record TopicClusterBand(
    string Id,
    string Title,
    ExamPart Part,
    string AccentHex,
    string SoftHex,
    int TotalCards,
    int DueCards,
    IReadOnlyList<string> ScopeIds,
    IReadOnlyList<TopicDeck> Decks);

public sealed record ExamPartBoard(
    ExamPart Part,
    string AccentHex,
    string SoftHex,
    int TopicCount,
    int TotalCards,
    int DueCards,
    IReadOnlyList<TopicClusterBand> Clusters);

/// <summary>
/// Baut aus flachen Topics eine Tafel: Prüfungsteil → Themengruppe → einzelne Decks.
/// Wurzeln (t1/t2/t3) werden zu Teil-Köpfen, ihre Kinder zu farbigen Bändern.
/// </summary>
public static class TopicBoardBuilder
{
    public static IReadOnlyList<ExamPartBoard> Build(
        IReadOnlyList<Topic> topics,
        IReadOnlyDictionary<string, TopicCardStats>? stats = null)
    {
        stats ??= new Dictionary<string, TopicCardStats>();
        var list = topics ?? [];
        var byParent = list.ToLookup(t => t.ParentTopicId ?? "", StringComparer.Ordinal);
        var roots = list.Where(t => string.IsNullOrEmpty(t.ParentTopicId)).OrderBy(t => t.SortOrder).ToList();
        var rootIds = roots.Select(t => t.Id).ToHashSet(StringComparer.Ordinal);

        var boards = new List<ExamPartBoard>();
        foreach (var part in new[] { ExamPart.Teil1, ExamPart.Teil2, ExamPart.WiSo })
        {
            var (partAccent, partSoft) = TopicPalette.ForPart(part);
            var partRoots = roots.Where(t => t.ExamPart == part).ToList();
            var clusters = new List<TopicClusterBand>();

            foreach (var root in partRoots)
            {
                var groups = byParent[root.Id].OrderBy(t => t.SortOrder).ToList();
                if (groups.Count == 0)
                {
                    clusters.Add(BandFrom(root, Descendants(root.Id, byParent), stats));
                    continue;
                }
                foreach (var group in groups)
                    clusters.Add(BandFrom(group, Descendants(group.Id, byParent), stats));
            }

            var orphaned = list.Where(t => t.ExamPart == part
                                           && !rootIds.Contains(t.Id)
                                           && !string.IsNullOrEmpty(t.ParentTopicId)
                                           && list.All(p => p.Id != t.ParentTopicId)).ToList();
            if (orphaned.Count > 0)
            {
                var (accent, soft) = TopicPalette.ForTopic(part, "orphan-" + part);
                clusters.Add(new TopicClusterBand(
                    "orphan-" + part,
                    "Weitere Themen",
                    part,
                    accent,
                    soft,
                    Sum(orphaned.Select(t => t.Id), stats).Total,
                    Sum(orphaned.Select(t => t.Id), stats).Due,
                    orphaned.Select(t => t.Id).ToList(),
                    orphaned.Select(t => DeckFrom(t, [t.Id], stats)).ToList()));
            }

            var allIds = clusters.SelectMany(c => c.ScopeIds).Distinct(StringComparer.Ordinal).ToList();
            var totals = Sum(allIds, stats);
            boards.Add(new ExamPartBoard(
                part,
                partAccent,
                partSoft,
                clusters.Sum(c => c.Decks.Count),
                totals.Total,
                totals.Due,
                clusters));
        }

        return boards;
    }

    private static TopicClusterBand BandFrom(
        Topic group,
        IReadOnlyList<Topic> descendants,
        IReadOnlyDictionary<string, TopicCardStats> stats)
    {
        var (accent, soft) = TopicPalette.ForTopic(group.ExamPart, group.Id);
        var decks = new List<TopicDeck>();
        var scope = new List<string> { group.Id };
        scope.AddRange(descendants.Select(d => d.Id));

        if (descendants.Count == 0)
        {
            decks.Add(DeckFrom(group, [group.Id], stats));
        }
        else
        {
            if (stats.ContainsKey(group.Id) && stats[group.Id].Total > 0)
                decks.Add(DeckFrom(group, [group.Id], stats));
            foreach (var leaf in descendants)
                decks.Add(DeckFrom(leaf, [leaf.Id], stats));
        }

        var totals = Sum(scope, stats);
        return new TopicClusterBand(group.Id, group.Title, group.ExamPart, accent, soft, totals.Total, totals.Due, scope, decks);
    }

    private static TopicDeck DeckFrom(Topic topic, IReadOnlyList<string> scope, IReadOnlyDictionary<string, TopicCardStats> stats)
    {
        var (accent, soft) = TopicPalette.ForTopic(topic.ExamPart, topic.Id);
        var totals = Sum(scope, stats);
        return new TopicDeck(topic.Id, topic.Title, topic.ExamPart, accent, soft, totals.Total, totals.Due, topic.EstimatedMinutes, scope);
    }

    private static List<Topic> Descendants(string id, ILookup<string, Topic> byParent)
    {
        var result = new List<Topic>();
        var queue = new Queue<string>();
        queue.Enqueue(id);
        var seen = new HashSet<string>(StringComparer.Ordinal) { id };
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in byParent[current].OrderBy(t => t.SortOrder))
            {
                if (!seen.Add(child.Id)) continue;
                result.Add(child);
                queue.Enqueue(child.Id);
            }
        }
        return result;
    }

    private static TopicCardStats Sum(IEnumerable<string> ids, IReadOnlyDictionary<string, TopicCardStats> stats)
    {
        var total = 0;
        var due = 0;
        foreach (var id in ids)
        {
            if (!stats.TryGetValue(id, out var s)) continue;
            total += s.Total;
            due += s.Due;
        }
        return new TopicCardStats(total, due);
    }
}
