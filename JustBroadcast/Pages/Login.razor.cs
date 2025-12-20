using JustBroadcast.Models;
using Microsoft.AspNetCore.Components;

namespace JustBroadcast.Pages
{
    public partial class Login
    {
        private LoginRequest loginModel = new();
        private string errorMessage = string.Empty;
        private bool isLoading = false;

        private async Task HandleLogin()
        {
            errorMessage = string.Empty;
            isLoading = true;

            try
            {
                var result = await AuthService.LoginAsync(loginModel);

                if (result != null)
                {
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