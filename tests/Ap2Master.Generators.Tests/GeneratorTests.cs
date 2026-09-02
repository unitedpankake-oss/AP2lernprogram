using Ap2Master.Content;
using Ap2Master.Domain;
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
            a.Solution.Should().NotBeNullOrWhiteSpace();
        }
    }
}

public class SortEvalTests
{
    [Fact]
    public void Bubble_like_result_is_sorted()
    {
        Domain.Evaluation.SortingEvaluator.IsSorted(new[] { 1, 2, 2, 5 }).Should().BeTrue();
    }
}
