namespace AnimeSpace.Application.DTOs
{
    /// <summary>
    /// Завантажений файл у вигляді, не залежному від ASP.NET.
    /// </summary>
    public record FileUploadDto
    {
        /// <summary>
        /// Вміст файлу. Потік відкриває й закриває той, хто його створив.
        /// </summary>
        public required Stream Content { get; init; }

        /// <summary>
        /// Початкове ім'я файлу (потрібне лише для визначення розширення; на диск файл зберігається під новим іменем).
        /// </summary>
        public required string FileName { get; init; }
    }
}
