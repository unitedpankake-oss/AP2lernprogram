using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Ap2Master.App.Localization;
using Ap2Master.Domain;
using Ap2Master.Domain.Learning;

namespace Ap2Master.App.ViewModels;

public sealed partial class PartChipVm : ObservableObject
{
    public ExamPart? Part { get; init; }
    [ObservableProperty] private string title = "";
    public string AccentHex { get; init; } = "#94A0B8";
    public string SoftHex { get; init; } = "#3394A0B8";
    public int TopicCount { get; init; }
    public int DueCount { get; init; }
    [ObservableProperty] private bool isSelected;
}

public sealed partial class TopicCardVm : ObservableObject
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public ExamPart Part { get; init; }
    public string AccentHex { get; init; } = "#3B82F6";
    public string SoftHex { get; init; } = "#333B82F6";
    public int Total { get; init; }
    public int Due { get; init; }
    public int EstimatedMinutes { get; init; }
    public IReadOnlyList<string> ScopeIds { get; init; } = [];
    [ObservableProperty] private bool isSelected;
}

public sealed class ClusterBandVm
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public ExamPart Part { get; init; }
    public string AccentHex { get; init; } = "#3B82F6";
    public string SoftHex { get; init; } = "#333B82F6";
    public int Due { get; init; }
    public int Total { get; init; }
    public IReadOnlyList<string> ScopeIds { get; init; } = [];
    public ObservableCollection<TopicCardVm> Cards { get; init; } = [];
}

public sealed partial class PartBoardVm : ObservableObject
{
    public ExamPart Part { get; init; }
    [ObservableProperty] private string title = "";
    public string AccentHex { get; init; } = "#3B82F6";
    public string SoftHex { get; init; } = "#283B82F6";
    public int Due { get; init; }
    public int Total { get; init; }
    public IReadOnlyList<string> ScopeIds { get; init; } = [];
    public ObservableCollection<ClusterBandVm> Clusters { get; init; } = [];
}

public static class TopicBoardUi
{
    public static ObservableCollection<PartChipVm> Chips(IReadOnlyList<ExamPartBoard> boards)
    {
        var chips = new ObservableCollection<PartChipVm>
        {
            new()
            {
                Part = null,
                Title = Loc.T("Part.All"),
                AccentHex = "#94A0B8",
                SoftHex = "#3394A0B8",
                TopicCount = boards.Sum(b => b.TopicCount),
                DueCount = boards.Sum(b => b.DueCards),
                IsSelected = true
            }
        };
        foreach (var b in boards)
        {
            chips.Add(new PartChipVm
            {
                Part = b.Part,
                Title = Loc.T(TopicPalette.LocKey(b.Part)),
                AccentHex = b.AccentHex,
                SoftHex = b.SoftHex,
                TopicCount = b.TopicCount,
                DueCount = b.DueCards
            });
        }
        return chips;
    }

    public static ObservableCollection<PartBoardVm> Map(IReadOnlyList<ExamPartBoard> boards)
    {
        var list = new ObservableCollection<PartBoardVm>();
        foreach (var b in boards)
        {
            var scope = b.Clusters.SelectMany(c => c.ScopeIds).Distinct(StringComparer.Ordinal).ToList();
            var part = new PartBoardVm
            {
                Part = b.Part,
                Title = Loc.T(TopicPalette.LocKey(b.Part)),
                AccentHex = b.AccentHex,
                SoftHex = b.SoftHex,
                Due = b.DueCards,
                Total = b.TotalCards,
                ScopeIds = scope
            };
            foreach (var c in b.Clusters)
            {
                var band = new ClusterBandVm
                {
                    Id = c.Id,
                    Title = c.Title,
                    Part = c.Part,
                    AccentHex = c.AccentHex,
                    SoftHex = c.SoftHex,
                    Due = c.DueCards,
                    Total = c.TotalCards,
                    ScopeIds = c.ScopeIds
                };
                foreach (var d in c.Decks)
                {
                    band.Cards.Add(new TopicCardVm
                    {
                        Id = d.Id,
                        Title = d.Title,
                        Part = d.Part,
                        AccentHex = d.AccentHex,
                        SoftHex = d.SoftHex,
                        Total = d.TotalCards,
                        Due = d.DueCards,
                        EstimatedMinutes = d.EstimatedMinutes,
                        ScopeIds = d.ScopeIds
                    });
                }
                part.Clusters.Add(band);
            }
            list.Add(part);
        }
        return list;
    }

    public static void RefreshTitles(IEnumerable<PartChipVm> chips, IEnumerable<PartBoardVm> boards)
    {
        foreach (var chip in chips)
            chip.Title = chip.Part is { } p ? Loc.T(TopicPalette.LocKey(p)) : Loc.T("Part.All");
        foreach (var board in boards)
            board.Title = Loc.T(TopicPalette.LocKey(board.Part));
    }

    public static IEnumerable<TopicCardVm> AllCards(IEnumerable<PartBoardVm> boards)
        => boards.SelectMany(b => b.Clusters).SelectMany(c => c.Cards);
}
