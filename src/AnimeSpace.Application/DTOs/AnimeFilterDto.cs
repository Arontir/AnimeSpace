using AnimeSpace.Domain.Enums;

namespace AnimeSpace.Application.DTOs
{
    public class AnimeFilterDto
    {
        public string? SearchQuery { get; set; }
        public int? GenreId { get; set; }
        public int? ReleaseYear { get; set; }
        public AnimeStatus? Status { get; set; }
        public AnimeSortBy SortBy { get; set; } = AnimeSortBy.Newest;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
