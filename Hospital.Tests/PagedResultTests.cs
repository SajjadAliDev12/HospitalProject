using FluentAssertions;
using Hospital.Core.DTOs;

namespace Hospital.Tests;

/// <summary>
/// Guards the paging contract every Desktop list VM depends on:
/// <c>ApiService.GetAsync&lt;PagedResult&lt;T&gt;&gt;</c> reads the computed
/// <see cref="PagedResult{T}.TotalPages"/>, so the API must send
/// TotalCount + PageSize (anonymous {Items, TotalPages} responses deserialize
/// TotalPages as 0/undefined and break paging).
/// </summary>
public class PagedResultTests
{
    [Theory]
    [InlineData(0, 15, 0)]
    [InlineData(1, 15, 1)]
    [InlineData(15, 15, 1)]
    [InlineData(16, 15, 2)]
    [InlineData(30, 15, 2)]
    [InlineData(31, 15, 3)]
    public void TotalPages_is_ceiling_of_count_over_size(int total, int size, int expected)
    {
        var page = new PagedResult<object> { TotalCount = total, PageSize = size, CurrentPage = 1 };
        page.TotalPages.Should().Be(expected);
    }

    [Fact]
    public void Items_defaults_to_empty_list()
    {
        new PagedResult<object>().Items.Should().NotBeNull().And.BeEmpty();
    }
}
