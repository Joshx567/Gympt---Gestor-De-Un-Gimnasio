using Gympt.Common;
using Gympt.DTO;
using Gympt.Services;
using System.Net.Http;

namespace Gympt.Application.Facades
{
    public class GymFacade
    {
        private readonly UserApiClient _userApi;
        private readonly ClientApiClient _clientApi;
        private readonly MembershipApiClient _membershipApi;

        public GymFacade(
            UserApiClient userApi,
            ClientApiClient clientApi,
            MembershipApiClient membershipApi)
        {
            _userApi = userApi;
            _clientApi = clientApi;
            _membershipApi = membershipApi;
        }

        // ============================================================
        // 📌 LOGIN USUARIO (JWT)
        // ============================================================
        public async Task<LoginResponseDTO> LoginUserAsync(LoginRequestDTO login)
        {
            // Llamamos al UserApiClient
            var result = await _userApi.LoginAsync(login);

            // 🔹 Guardamos token en DB o lo imprimimos (opcional)
            if (!string.IsNullOrEmpty(result.token))
            {
                Console.WriteLine("=== [GymFacade] Token recibido desde LoginAsync ===");
                Console.WriteLine(result.token);
                Console.WriteLine("===================================================");
            }

            return result;
        }


        public async Task<string> GetTestTokenAsync(string email)
        {
            return await _userApi.GetTestTokenAsync(email);
        }

        public void SetJwtToken(string token)
        {
            _userApi.SetJwtToken(token);
        }

        // 🔹 En GymFacade
        public void SaveTokenToSession(string token)
        {
            _userApi.SaveTokenToSession(token);
        }

        public string? GetTokenFromSession()
        {
            return _userApi.GetTokenFromSession();
        }


        // ============================================================
        // 📌 CRUD DE USUARIOS
        // ============================================================
        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(string role = "")
        {
            return await _userApi.GetAllUsersAsync(role);
        }

        public async Task<UserDTO> GetUserByIdAsync(int id)
        {
            return await _userApi.GetByIdAsync(id);
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO newUser)
        {
            return await _userApi.CreateUserAsync(newUser);
        }

        public async Task UpdateUserAsync(int id, UserDTO userToUpdate)
        {
            await _userApi.UpdateUserAsync(id, userToUpdate);
        }

        public async Task DeleteUserAsync(int id)
        {
            await _userApi.DeleteUserAsync(id);
        }

        public async Task LogoutUserAsync()
        {
            await _userApi.LogoutAsync();
        }

        // ============================================================
        // 📌 REGISTRAR UNA NUEVA MEMBRESÍA PARA CLIENTE
        // ============================================================
        public async Task<Result<MembershipDTO>> RegistrarMembresiaAsync(int clientId, MembershipDTO nueva)
        {
            var cliente = await _clientApi.GetClientByIdAsync(clientId);
            if (cliente == null)
                return Result<MembershipDTO>.Failure("Cliente no encontrado.");

            var result = await _membershipApi.CreateMembershipAsync(nueva);

            if (result.IsSuccess)
            {
                cliente.MembershipId = result.Value.Id;
                await _clientApi.UpdateClientAsync(clientId, cliente);
            }

            return result;
        }

        // ============================================================
        // 📌 RENOVAR MEMBRESÍA
        // ============================================================
        public async Task<Result<MembershipDTO>> RenovarMembresiaAsync(int membershipId)
        {
            var membresia = await _membershipApi.GetMembershipByIdAsync(membershipId);
            if (membresia == null)
                return Result<MembershipDTO>.Failure("Membresía no encontrada.");

            membresia.LastModification = DateTime.UtcNow;
            membresia.LastModifiedBy = "System";

            return await _membershipApi.UpdateMembershipAsync(membershipId, membresia);
        }

        // ============================================================
        // 📌 VIEWMODELS PARA LA UI
        // ============================================================
        public class GymDashboardViewModel
        {
            public int TotalUsuarios { get; set; }
            public int TotalClientes { get; set; }
            public int TotalMembresias { get; set; }
            public int MembresiasActivas { get; set; }
            public int MembresiasInactivas { get; set; }
        }

        public class ClientProfileViewModel
        {
            public ClientDTO Client { get; set; }
            public MembershipDTO Membership { get; set; }
            public UserDTO Usuario { get; set; }
        }

        public class TokenResponseDTO
        {
            public string Token { get; set; }
        }
    }
}
