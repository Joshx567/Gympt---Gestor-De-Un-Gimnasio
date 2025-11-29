using Gympt.Common;
using Gympt.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;


namespace Gympt.Services
{
    public class MembershipApiClient
    {
        private readonly HttpClient _http;

        public MembershipApiClient(HttpClient http)
        {
            _http = http;
        }

        // =============================
        // GET: Obtener todas las membresías
        // =============================
        public async Task<List<MembershipDTO>> GetMembershipsAsync()
        {
            return await _http.GetFromJsonAsync<List<MembershipDTO>>("api/memberships");
        }

        // =============================
        // GET: Obtener membresía por ID
        // =============================
        public async Task<MembershipDTO> GetMembershipByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<MembershipDTO>($"api/memberships/{id}");
        }

        // =============================
        // POST: Crear membresía
        // =============================
        public async Task<Result<MembershipDTO>> CreateMembershipAsync(MembershipDTO membership)
        {
            membership.CreatedAt = DateTime.UtcNow;
            membership.CreatedBy = "System";

            var response = await _http.PostAsJsonAsync("api/memberships", membership);

            try
            {
                // Usa HandleApiResponse para lanzar excepción si hay error
                await HandleApiResponse(response);

                // Si llegamos aquí, el response es exitoso
                var created = await response.Content.ReadFromJsonAsync<MembershipDTO>();
                return Result<MembershipDTO>.Success(created);
            }
            catch (ApiException ex)
            {
                // Retornamos failure con el mensaje real de error
                return Result<MembershipDTO>.Failure(ex.Message);
            }
        }

        public async Task<Result<MembershipDTO>> UpdateMembershipAsync(int id, MembershipDTO membership)
        {
            membership.LastModification = DateTime.UtcNow;
            membership.LastModifiedBy ??= "System";

            var response = await _http.PutAsJsonAsync($"api/memberships/{id}", membership);

            try
            {
                await HandleApiResponse(response);

                // Caso donde el microservicio responde 204 o sin JSON
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent ||
                    response.Content.Headers.ContentLength == 0)
                {
                    return Result<MembershipDTO>.Success(membership);
                }

                var updated = await response.Content.ReadFromJsonAsync<MembershipDTO>();
                return Result<MembershipDTO>.Success(updated ?? membership);
            }
            catch (ApiException ex)
            {
                return Result<MembershipDTO>.Failure(ex.Message);
            }
        }

        public async Task<bool> DeleteMembershipAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/memberships/{id}");

            try
            {
                await HandleApiResponse(response);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task HandleApiResponse(HttpResponseMessage response)
        {
            // Si la respuesta es exitosa (código 2xx), no hacemos nada.
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            // Si no es exitosa, leemos el mensaje de error del cuerpo.
            string errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage = "La API devolvió un error, pero no se pudo leer el mensaje.";

            try
            {
                // Intentamos parsear el JSON para encontrar la propiedad "error"
                using (JsonDocument doc = JsonDocument.Parse(errorContent))
                {
                    if (doc.RootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        errorMessage = errorElement.GetString();
                    }
                }
            }
            catch (JsonException)
            {
                // Si el cuerpo no es JSON, usamos el contenido directamente (si no está vacío).
                if (!string.IsNullOrEmpty(errorContent))
                {
                    errorMessage = errorContent;
                }
            }

            // Lanzamos nuestra excepción personalizada con el mensaje de error real.
            throw new ApiException(errorMessage, response.StatusCode);
        }
    }
}
