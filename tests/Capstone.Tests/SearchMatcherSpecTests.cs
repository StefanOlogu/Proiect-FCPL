using SharpQuest.Contracts;

namespace Capstone.Tests;

/// <summary>One test per SearchMatcher rule (R1–R8). The grader runs these against 8 broken matchers.</summary>
public class SearchMatcherSpecTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void R1_empty_query_matches_everything(string query)
    {
        Assert.True(SearchMatcher.Create().Matches("Mama cu Papucul", query));
    }

    [Fact]
    public void R2_case_is_ignored()
    {
        Assert.True(SearchMatcher.Create().Matches("Mama cu Papucul", "PAPUCUL"));
    }

    [Theory]
    [InlineData("Café", "cafe")]     // accent in the text
    [InlineData("Sosea", "șosea")]   // accent in the query
    public void R3_diacritics_are_ignored(string text, string query)
    {
        Assert.True(SearchMatcher.Create().Matches(text, query));
    }

    [Fact]
    public void R4_every_word_must_occur()
    {
        Assert.False(SearchMatcher.Create().Matches("Vecinul cu Bormașina", "vecinul doge"));
    }

    [Fact]
    public void R5_word_order_does_not_matter()
    {
        Assert.True(SearchMatcher.Create().Matches("This Is Fine Dog", "dog fine"));
    }

    [Fact]
    public void R6_word_can_be_inside_a_longer_word()
    {
        Assert.True(SearchMatcher.Create().Matches("Stonks Man", "onk"));
    }

    [Fact]
    public void R7_null_text_never_matches_and_does_not_throw()
    {
        var matcher = SearchMatcher.Create();
        var result = true;

        var ex = Record.Exception(() => { result = matcher.Matches(null, "doge"); });

        Assert.Null(ex);
        Assert.False(result);
    }

    [Fact]
    public void R8_null_query_throws()
    {
        Assert.Throws<ArgumentNullException>(() => SearchMatcher.Create().Matches("Trollface", null!));
    }
}
