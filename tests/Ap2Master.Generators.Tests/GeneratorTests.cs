using Ap2Master.Content;
using Ap2Master.Generators;
using FluentAssertions;

namespace Ap2Master.Generators.Tests;

public class SeedReproTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(99991)]
    public void Templates_are_stable(int seed)
    {
        foreach (var tpl in ExerciseTemplateCatalog.All)
        {
            var a = SeededTemplateEngine.GenerateFrom(tpl, seed);
            var b = SeededTemplateEngine.GenerateFrom(tpl, seed);
            a.TaskText.Should().Be(b.TaskText);
            a.Solution.Should().Be(b.Solution);
            a.ParametersJson.Should().Be(b.ParametersJson);
        }
    }

    [Fact]
    public void Different_seeds_change_the_wirtschaftlichkeit_task()
    {
        var tpl = ExerciseTemplateCatalog.All.First(t => t.Id == "tpl-wirtschaftlichkeit");
        SeededTemplateEngine.GenerateFrom(tpl, 1).TaskText
            .Should().NotBe(SeededTemplateEngine.GenerateFrom(tpl, 2).TaskText);
    }
}

public class PseudocodeTests
{
    [Fact]
    public void Maximum_algorithm_returns_expected_value()
    {
        var src = """
            SET max = liste[0]
            FOR i FROM 1 TO n-1
            IF liste[i] > max THEN
            SET max = liste[i]
            ENDIF
            ENDFOR
            RETURN max
            """;
        var engine = new PseudocodeEngine();
        var r = engine.Run(src, new Dictionary<string, object?> { ["liste"] = new List<int> { 3, 9, 2 }, ["n"] = 3 });
        r.Success.Should().BeTrue();
        Convert.ToInt32(r.ReturnValue).Should().Be(9);
    }
}
