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
    public class ClientApiClient
    {
        private readonly HttpClient _http;

        public ClientApiClient(HttpClient http)
        {
            _http = http;
        }

        // =============================
        // GET: Obtener todos los clientes
        // =============================
        public async Task<List<ClientDTO>> GetClientsAsync()
        {
            return await _http.GetFromJsonAsync<List<ClientDTO>>("api/clients");
        }

        // =============================
        // GET: Obtener cliente por ID
        // =============================
        public async Task<ClientDTO> GetClientByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<ClientDTO>($"api/clients/{id}");
        }

        // =============================
        // POST: Crear nuevo cliente
        // =============================
        public async Task<Result<ClientDTO>> CreateClientAsync(ClientDTO client)
        {
            client.CreatedAt = DateTime.UtcNow;
            client.CreatedBy = "System";

            Console.WriteLine("Enviando cliente al microservicio:");
            Console.WriteLine(JsonSerializer.Serialize(client));

            var response = await _http.PostAsJsonAsync("api/clients", client);

            try
            {
                await HandleApiResponse(response);

                var createdClient = await response.Content.ReadFromJsonAsync<ClientDTO>();
                Console.WriteLine("Cliente creado correctamente: " + JsonSerializer.Serialize(createdClient));
                return Result<ClientDTO>.Success(createdClient);
            }
            catch (ApiException ex)
            {
                Console.WriteLine("Error recibido del microservicio: " + ex.Message);
                return Result<ClientDTO>.Failure(ex.Message);
            }
        }

        // =============================
        // PUT: Actualizar cliente existente
        // =============================
        public async Task<Result<ClientDTO>> UpdateClientAsync(int id, ClientDTO client)
        {
            client.LastModification = DateTime.UtcNow;
            client.LastModifiedBy ??= "System";

            var response = await _http.PutAsJsonAsync($"api/clients/{id}", client);

            try
            {
                await HandleApiResponse(response);

                var updatedClient = await response.Content.ReadFromJsonAsync<ClientDTO>();
                return Result<ClientDTO>.Success(updatedClient);
            }
            catch (ApiException ex)
            {
                return Result<ClientDTO>.Failure(ex.Message);
            }
        }

        // =============================
        // DELETE: Eliminar cliente
        // =============================
        public async Task<bool> DeleteClientAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/clients/{id}");

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

        // =============================
        // Método centralizado de manejo de errores
        // =============================
        private async Task HandleApiResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            string errorContent = await response.Content.ReadAsStringAsync();
            string errorMessage = "La API devolvió un error, pero no se pudo leer el mensaje.";

            try
            {
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
                if (!string.IsNullOrEmpty(errorContent))
                    errorMessage = errorContent;
            }

            throw new ApiException(errorMessage, response.StatusCode);
        }
    }
}