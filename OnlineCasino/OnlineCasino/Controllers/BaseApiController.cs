namespace OnlineCasino.Controllers
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc;

    using OnlineCasino.Models;

    /// <summary>
    /// Базов контролер за всички API контролери в OnlineCasino.
    /// Предоставя:
    /// <list type="bullet">
    ///   <item>Унифицирано преобразуване на <see cref="Result{T}"/> към <see cref="IActionResult"/>.</item>
    ///   <item>Управление на JWT auth cookie (set / clear).</item>
    ///   <item>Извличане на текущия userId от JWT claims.</item>
    ///   <item>Скриване на чувствителни данни (напр. JWT token) от response body-то.</item>
    /// </list>
    /// Всички наследници автоматично получават <c>[ApiController]</c> и <c>[Route("api/[controller]")]</c>.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Преобразува <see cref="Result{T}"/> в подходящ <see cref="IActionResult"/>.
        /// <para>
        /// Поведение:
        /// <list type="bullet">
        ///   <item>Ако <paramref name="result"/> е <c>null</c> → 500 с generic error.</item>
        ///   <item>Ако е success и Data е <c>null</c> → 204 No Content.</item>
        ///   <item>Ако е success и Data не е <c>null</c> → 200 OK с Data.</item>
        ///   <item>Ако е failure → status code от <see cref="ErrorResponse.StatusCode"/> с <see cref="ErrorResponse"/> като body.</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <typeparam name="T">Тип на payload-а при успех.</typeparam>
        /// <param name="result">Резултатът, върнат от service layer-а.</param>
        /// <returns>Готов <see cref="IActionResult"/> за връщане от контролера.</returns>
        protected IActionResult AsActionResult<T>(Result<T> result)
        {
            if (result is null)
                return StatusCode(500, new ErrorResponse(500, "Unexpected error."));

            if (result.IsSuccess)
            {
                if (result.Data is null) return NoContent();
                return Ok(result.Data);
            }

            var err = result.Error ?? new ErrorResponse(500, "Unexpected error.");
            return StatusCode(err.StatusCode, err);
        }

        /// <summary>
        /// Записва JWT access token в HttpOnly cookie с име <c>access_token</c>.
        /// <para>
        /// Cookie настройки:
        /// <list type="bullet">
        ///   <item><c>HttpOnly = true</c> – недостъпна от JavaScript (защита срещу XSS).</item>
        ///   <item><c>Secure = true</c> – само по HTTPS. ⚠️ Ако тестваш React Native срещу HTTP-only backend (напр. емулатор), тази настройка ще накара браузъра/клиента да игнорира cookie-то. За development може да я направиш условна според environment-а.</item>
        ///   <item><c>SameSite = Strict</c> – не се изпраща при cross-site заявки (защита срещу CSRF).</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="token">JWT токенът, генериран от <c>IAuthService</c>.</param>
        /// <param name="expires">UTC дата на изтичане на токена.</param>
        protected void SetAuthCookie(string token, DateTime expires)
        {
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires
            });
        }

        /// <summary>
        /// Изтрива auth cookie-то <c>access_token</c>.
        /// Използва се при logout.
        /// <para>
        /// ⚠️ Настройките (<c>HttpOnly</c>, <c>Secure</c>, <c>SameSite</c>) трябва да съвпадат с тези в <see cref="SetAuthCookie"/>, иначе някои браузъри няма да изтрият cookie-то.
        /// </para>
        /// </summary>
        protected void ClearAuthCookie()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }

        /// <summary>
        /// Скрива съдържанието на <see cref="Result{T}"/> и връща само <c>{ message = "OK" }</c> при успех.
        /// <para>
        /// Използва се след <see cref="SetAuthCookie"/> в AuthController, за да не изтече JWT токенът в response body-то.
        /// </para>
        /// </summary>
        /// <typeparam name="T">Оригинален тип на Data-та (напр. <c>(string Token, DateTime Expires)</c>).</typeparam>
        /// <param name="r">Оригиналният <see cref="Result{T}"/>.</param>
        /// <returns><see cref="Result{T}"/> от тип <c>object</c> с generic success или оригиналната грешка.</returns>
        protected Result<object> ResultMap<T>(Result<T> r)
        {
            if (r.IsSuccess)
                return Result<object>.Success(new { message = "OK" });

            return Result<object>.Failure(
                r.Error?.Message ?? "Unexpected error.",
                r.Error?.StatusCode ?? 500);
        }

        /// <summary>
        /// Извлича userId на логнатия потребител от JWT claim <see cref="ClaimTypes.NameIdentifier"/>.
        /// </summary>
        /// <returns>
        /// <c>int</c> userId при валиден claim;
        /// <c>null</c> ако claim-ът липсва, празен е или не е валиден integer.
        /// </returns>
        /// <remarks>
        /// Контролерите трябва да третират <c>null</c> като 401 Unauthorized:
        /// <code>
        /// var userId = GetUserId();
        /// if (userId is null) return Unauthorized(new { message = "Unauthorized." });
        /// </code>
        /// </remarks>
        protected int? GetUserId()
        {
            var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(val, out var id) ? id : null;
        }
    }
}
