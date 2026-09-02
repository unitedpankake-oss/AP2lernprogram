using Ap2Master.Domain;
using Ap2Master.Domain.Learning;
using Ap2Master.Domain.Ui;
using FluentAssertions;

namespace Ap2Master.Domain.Tests;

public class ExamNavigatorTests
{
    [Theory]
    [InlineData(true, false, false, ExamNavKind.Current)]
    [InlineData(true, true, true, ExamNavKind.Current)]
    [InlineData(false, false, true, ExamNavKind.Marked)]
    [InlineData(false, true, true, ExamNavKind.Marked)]
    [InlineData(false, true, false, ExamNavKind.Answered)]
    [InlineData(false, false, false, ExamNavKind.Unanswered)]
    public void Classify_prefers_current_then_marked(bool current, bool answered, bool marked, ExamNavKind expected)
        => ExamNavigator.Classify(current, answered, marked).Should().Be(expected);

    [Fact]
    public void Glyphs_distinguish_states_without_relying_on_color()
    {
        ExamNavigator.Glyph(ExamNavKind.Answered).Should().NotBeNullOrEmpty();
        ExamNavigator.Glyph(ExamNavKind.Marked).Should().NotBeNullOrEmpty();
        ExamNavigator.Glyph(ExamNavKind.Answered).Should().NotBe(ExamNavigator.Glyph(ExamNavKind.Marked));
        ExamNavigator.Glyph(ExamNavKind.Unanswered).Should().BeEmpty();
    }
}

public class ActivityPaletteTests
{
    [Fact]
    public void Plan_blocks_get_stable_colors_from_kind_and_topic()
    {
        var sql = TopicPalette.ForActivity(ActivityKind.SqlLab, "t2-sql-join");
        var wiso = TopicPalette.ForActivity(ActivityKind.WiSoSimulation, "t3-textform");
        sql.Accent.Should().NotBe(wiso.Accent);
        TopicPalette.ForActivity(ActivityKind.SqlLab, "t2-sql-join").Accent.Should().Be(sql.Accent);
    }
}
