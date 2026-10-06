using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Services
{
    public interface IAnimeService
    {
        public Task<AnimeDetailsDto> GetDetailsAsync(int id);

        public Task<AnimeUpdateDto> GetForEditAsync(int id);

        public Task<PagedResult<AnimeCardDto>> GetCatalogAsync(AnimeFilterDto filter);

        public Task<IReadOnlyList<AnimeCardDto>> GetLatestAsync(int count);

        public Task<IReadOnlyList<AnimeCardDto>> GetTopRatedAsync(int count);

        public Task<int> CreateAsync(AnimeCreateDto dto, FileUploadDto? poster);

        public Task UpdateAsync(AnimeUpdateDto dto, FileUploadDto? poster);

        public Task DeleteAsync(int id);
    }
}
