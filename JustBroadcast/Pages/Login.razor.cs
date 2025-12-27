using Blazored.LocalStorage;
using JustBroadcast.Models;
using Microsoft.AspNetCore.Components;

namespace JustBroadcast.Pages
{
    public partial class Login
    {
        [Inject]
        private ILocalStorageService LocalStorage { get; set; } = default!;

        private LoginRequest loginModel = new();
        private string errorMessage = string.Empty;
        private bool isLoading = false;
        private bool rememberMe = false;

        protected override async Task OnInitializedAsync()
        {
            // Load saved credentials if they exist
            var savedUsername = await LocalStorage.GetItemAsync<string>("rememberedUsername");
            var savedPassword = await LocalStorage.GetItemAsync<string>("rememberedPassword");

            if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
            {
                loginModel.Username = savedUsername;
                loginModel.Password = savedPassword;
                rememberMe = true;
            }
        }

        private async Task HandleLogin()
        {
            errorMessage = string.Empty;
            isLoading = true;

            try
            {
                var result = await AuthService.LoginAsync(loginModel);

                if (result != null)
                {
                    // Save credentials if "Remember Me" is checked
                    if (rememberMe)
                    {
                        await LocalStorage.SetItemAsync("rememberedUsername", loginModel.Username);
                        await LocalStorage.SetItemAsync("rememberedPassword", loginModel.Password);
                    }
                    else
                    {
                        // Clear saved credentials if "Remember Me" is unchecked
                        await LocalStorage.RemoveItemAsync("rememberedUsername");
                        await LocalStorage.RemoveItemAsync("rememberedPassword");
                    }

                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    errorMessage = "Invalid username or password";
                }
            }
            catch (Exception)
            {
                errorMessage = "An error occurred during login. Please try again.";
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}