using System.Text;
using Ap2Master.Content;
using Ap2Master.Infrastructure.Persistence;
using FluentAssertions;

namespace Ap2Master.Infrastructure.Tests;

public class CatalogStorageTests
{
    [Fact]
    public void Encoded_catalog_fits_the_atlas_budget()
    {
        var catalog = new ContentCatalogFactory();
        var texts = new List<string?>();
        foreach (var content in catalog.Contents)
        {
            texts.Add(content.Title);
            texts.Add(content.Question);
            texts.Add(content.Answer);
            texts.Add(content.Explanation);
            texts.Add(content.HintsJson);
            texts.Add(content.PayloadJson);
            foreach (var option in content.Options)
            {
                texts.Add(option.Text);
                texts.Add(option.Explanation);
            }
        }
        foreach (var card in catalog.Flashcards)
        {
            texts.Add(card.Front);
            texts.Add(card.Back);
            texts.Add(card.Formula);
        }
        foreach (var template in catalog.Templates)
        {
            texts.Add(template.TemplateText);
            texts.Add(template.SolutionTemplate);
            texts.Add(template.ExplanationTemplate);
            texts.Add(template.HintsJson);
            texts.Add(template.ParameterSchemaJson);
        }
        foreach (var fact in catalog.LegalFacts)
        {
            texts.Add(fact.Statement);
            texts.Add(fact.HistoricalStatement);
            texts.Add(fact.Notes);
        }

        var raw = texts.Sum(t => Encoding.UTF8.GetByteCount(t ?? ""));
        var blobs = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        var inline = 0;
        foreach (var text in texts)
        {
            if (string.IsNullOrEmpty(text)) continue;
            var payload = TextCodec.TryCompress(text);
            if (payload is null)
            {
                inline += Encoding.UTF8.GetByteCount(text);
                continue;
            }
            var hash = TextCodec.Hash(text);
            blobs.TryAdd(hash, payload);
            TextCodec.Decompress(payload).Should().Be(text);
        }

        var packed = inline + blobs.Sum(b => b.Value.Length);
        packed.Should().BeLessThan(raw);
        packed.Should().BeLessThan(8 * 1024 * 1024, "Atlas free tier is 512 MB and the catalog must stay a small fraction of that");
        blobs.Should().NotBeEmpty();
    }

    [Fact]
    public void Errors_do_not_reveal_the_connection_string()
    {
        var leaked = "timeout mongodb://learner:s3cret@ac-j74dz4y.example.net:27017/?authSource=admin";
        var safe = MongoGateway.Redact(leaked);
        safe.Should().NotContain("s3cret");
        safe.Should().NotContain("learner:");
        safe.Should().Contain("mongodb://***");
    }

    [Fact]
    public void Token_roundtrip_restores_the_original_text()
    {
        var library = new MaterialLibrary();
        var original = new string('ä', 200) + " Textform bleibt Textform.";
        var token = library.Encode(original);
        TextCodec.IsToken(token!).Should().BeTrue();
        library.Decode(token).Should().Be(original);
        library.Encode("kurz").Should().Be("kurz");
    }
}
