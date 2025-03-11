using Blazored.LocalStorage;
using Frontend.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace Frontend.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        private readonly ISyncLocalStorageService localStorage;

        public CustomAuthStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage)
        {
            this.httpClient = httpClient;
            this.localStorage = localStorage;

            //validamos una vez si ya esta autenticado ante el refresco de la pagina
            var accesToken = localStorage.GetItem<string>("accessToken");
            if(accesToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesToken);
            }
        }
        public override async  Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity()); //usuario no authenticado
            try
            {
                var response = await httpClient.GetAsync("manage/info");
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var email = jsonResponse!["email"]!.ToString();

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, email),
                        new(ClaimTypes.Email, email)
                    };

                    var identity = new ClaimsIdentity(claims, "Token");
                    user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return new AuthenticationState(user);
        }

        public async Task<Resultado> LoginAsincrono(string email, string password)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("login", new { email, password });
                if(response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();

                    //Guarda el accestoken y el refreshtoken en el almac local de la pagina
                    localStorage.SetItem("accessToken", accessToken);
                    localStorage.SetItem("refreshToken", refreshToken);
                    //Agrega el token de acceso al Encabezado de autorizacion del client http - Header
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


                    //Actualiza el estado del autenticado
                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    //Autenticado!!
                    return new Resultado { Exito = true };
                }
            }
            catch (Exception e)
            {
                return new Resultado { Exito = false, Errors = [e.Message] };
            }
            return new Resultado { Exito = false, Errors = ["Connection Error"] };
        }

        public void Logout()
        {
            //eliminar los tokens del localstorage
            localStorage.RemoveItem("accessToken");
            localStorage.RemoveItem("refreshToken");
            httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        public async Task<Resultado> Registro(RegistroDTO registroDTO)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("register", registroDTO);
                if(!response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(strResponse);
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var errorObject = jsonResponse["errors"].AsObject();
                    var errorList = new List<string>();
                    foreach(var error in errorObject)
                    {
                        errorList.Add(error.Value![0]!.ToString());
                    }
                    return new Resultado { Exito = false, Errors = errorList.ToArray()  };
                    
                }
                var loginResponse = await LoginAsincrono(registroDTO.Email, registroDTO.Password);
                return loginResponse;
            }
            catch (Exception ex)
            {

                return new Resultado { Exito = true, Errors = [ex.Message] };
            }

        }
    }

    public class Resultado
    {
        public bool Exito { get; set; }
        public string[] Errors { get; set; } = [];
    }
}
