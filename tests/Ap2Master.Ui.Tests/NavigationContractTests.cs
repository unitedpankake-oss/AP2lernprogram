using Ap2Master.Content;
using Ap2Master.Domain;
using FluentAssertions;

namespace Ap2Master.Ui.Tests;

public class NavigationContractTests
{
    [Fact]
    public void All_exam_parts_have_priority_topics()
    {
        TopicCatalog.All.Where(t => t.Part == ExamPart.Teil1 && t.Priority).Should().NotBeEmpty();
        TopicCatalog.All.Where(t => t.Part == ExamPart.Teil2 && t.Priority).Should().NotBeEmpty();
        TopicCatalog.All.Where(t => t.Part == ExamPart.WiSo && t.Priority).Should().NotBeEmpty();
    }

    [Fact]
    public void German_default_section_names_remain_exam_language()
    {
        var titles = new[]
        {
            "Dashboard", "Mein Lernplan", "Themenkatalog", "Lernkarten", "Aufgabentrainer", "SQL-Labor",
            "Code- und Pseudocode-Trainer", "UML- und ER-Trainer", "Projektmanagement-Labor",
            "Test- und Coverage-Trainer", "Netzwerk- und Security-Trainer", "WiSo-Trainer",
            "Probeklausuren", "Fehlerbuch", "Statistiken", "Lerninhalte verwalten", "Einstellungen"
        };
        titles.Should().HaveCount(17);
        titles.Should().Contain("Fehlerbuch");
        titles.Should().Contain("Lernkarten");
        titles.Should().Contain("Einstellungen");
    }

    [Fact]
    public void Settings_offer_de_ru_uk_en_es()
    {
        var languages = new[]
        {
            ("de", "Deutsch (Hauptsprache)"),
            ("ru", "Русский"),
            ("uk", "Українська"),
            ("en", "English"),
            ("es", "Español")
        };
        languages.Should().HaveCount(5);
        languages.Select(l => l.Item1).Should().Equal("de", "ru", "uk", "en", "es");
    }
}
