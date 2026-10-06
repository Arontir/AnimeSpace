using AnimeSpace.Application.DTOs;

namespace AnimeSpace.Application.Abstractions.Storage
{
    public interface IFileService
    {
        public Task<string> SavePosterAsync(FileUploadDto file);

        public Task DeletePosterAsync(string? posterUrl);
    }
}