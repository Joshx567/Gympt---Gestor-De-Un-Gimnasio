    // Ruta: Gympt/Services/UserApiClient.cs
    using Gympt.DTO;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;

    namespace Gympt.Services
    {
        public class UserApiClient
        {
            private readonly HttpClient _httpClient;
            private string? _jwtToken;
            private readonly IHttpContextAccessor _httpContextAccessor;

            public UserApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            {
                _httpClient = httpClient;
                _httpContextAccessor = httpContextAccessor;
            }

            /// <summary>
            /// Configura el token JWT que se usará en todas las solicitudes.
            /// </summary>
            public void SetJwtToken(string token, bool saveToSession = false)
            {
                _jwtToken = token;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _jwtToken);

                if (saveToSession)
                {
                    var context = _httpContextAccessor.HttpContext;
                    if (context != null)
                    {
                        context.Session.SetString("jwt_token", token);
                        Console.WriteLine("=== Token guardado en Session ===");
                    }
                }
            }


        // --- MÉTODOS CRUD ACTUALIZADOS ---
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(string role = "")
        {
            EnsureTokenSet(); // asegura que _jwtToken está configurado

            var request = new HttpRequestMessage(HttpMethod.Get, "api/users");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwtToken);

            Console.WriteLine("=== HEADER Authorization que se enviará ===");
            Console.WriteLine(request.Headers.Authorization);  // Esto debería mostrar: Bearer <token>


            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDTO>>() ?? new List<UserDTO>();

            // Debug
            Console.WriteLine("=== [UserApiClient] Usuarios recibidos desde la API ===");
            foreach (var u in users)
                Console.WriteLine($"ID: {u.id}, Email: {u.email}, Role recibido: {u.role}");

            return users;
        }


        public async Task<UserDTO> GetByIdAsync(int id)
            {
                EnsureTokenSet();
                var response = await _httpClient.GetAsync($"api/users/{id}");
                await HandleApiResponse(response);
                return await response.Content.ReadFromJsonAsync<UserDTO>() ?? new UserDTO();
            }

            public async Task<UserDTO> CreateUserAsync(UserDTO newUser)
            {
                EnsureTokenSet();
                var response = await _httpClient.PostAsJsonAsync("api/users", newUser);
                await HandleApiResponse(response);
                return await response.Content.ReadFromJsonAsync<UserDTO>() ?? new UserDTO();
            }

            public async Task UpdateUserAsync(int id, UserDTO userToUpdate)
            {
                EnsureTokenSet();
                var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", userToUpdate);
                await HandleApiResponse(response);
            }

            public async Task DeleteUserAsync(int id)
            {
                EnsureTokenSet();
                var response = await _httpClient.DeleteAsync($"api/users/{id}");
                await HandleApiResponse(response);
            }

            public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO login)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", login);
                await HandleApiResponse(response);

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponseDTO>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

                // 🔹 Guardamos automáticamente el token si existe
                if (!string.IsNullOrEmpty(result.token))
                {
                    SetJwtToken(result.token);

                    // 🔹 Guardar en Session
                    SaveTokenToSession(result.token);

                    // 🔹 Imprimir en consola
                    Console.WriteLine("=== [UserApiClient] Token guardado automáticamente ===");
                    Console.WriteLine(result.token);
                    Console.WriteLine("======================================================");
                }

                return result;
            }

            public async Task LogoutAsync()
            {
                EnsureTokenSet();
                var response = await _httpClient.PostAsync("api/Auth/logout", null);
                await HandleApiResponse(response);

                // Limpiamos el token local
                _jwtToken = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
            public async Task UpdateUserTokenAsync(int userId, string token, DateTime expiresAt)
            {
                var payload = new { Token = token, ExpiresAt = expiresAt };
                var response = await _httpClient.PutAsJsonAsync($"api/users/{userId}/token", payload);
                response.EnsureSuccessStatusCode();
            }

            public async Task<string> GetTestTokenAsync(string email)
            {
                // Llamada al endpoint /test-token/{email}
                var response = await _httpClient.GetAsync($"api/Auth/test-token/{email}");
                await HandleApiResponse(response);

                var json = await response.Content.ReadAsStringAsync();

                // Deserialize simple para obtener el token
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("token", out var tokenElement))
                    return tokenElement.GetString()!;

                throw new InvalidOperationException("No se pudo obtener el token del endpoint /test-token.");
            }

            public void SaveTokenToSession(string token)
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null)
                {
                    context.Session.SetString("jwt_token", token);
                    Console.WriteLine("=== Token guardado en Session ===");
                }
            }

            public string? GetTokenFromSession()
            {
                var context = _httpContextAccessor.HttpContext;
                return context?.Session.GetString("jwt_token");
            }



        // --- MÉTODOS PRIVADOS ---
        private void EnsureTokenSet()
            {
                if (string.IsNullOrEmpty(_jwtToken))
                    throw new InvalidOperationException("JWT token no configurado. Llama a SetJwtToken o LoginAsync primero.");
            }

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
                            errorMessage = errorElement.GetString()!;
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