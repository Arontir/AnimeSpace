namespace AnimeSpace.Web.Extensions
{
    /// <summary>
    /// Допоміжні методи для HTTP-запиту.
    /// </summary>
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Чи очікує клієнт JSON-відповідь (запит із fetch/AJAX): заголовок Accept містить "application/json".
        /// fetch сам не надсилає X-Requested-With, тому наш ajax.js завжди ставить Accept.
        /// </summary>
        /// <param name="request">HTTP-запит.</param>
        public static bool IsAjax(this HttpRequest request)
            => request.Headers["Accept"].ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
