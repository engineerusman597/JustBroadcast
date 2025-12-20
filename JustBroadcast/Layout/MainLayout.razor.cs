using Microsoft.AspNetCore.Components;
using JustBroadcast.Models;

namespace JustBroadcast.Layout
{
    public partial class MainLayout
    {
        private bool showUserMenu = false;
        private UserInfoDto? userInfo;

        protected override async Task OnInitializedAsync()
        {
            await LoadUserInfo();
        }

        private async Task LoadUserInfo()
        {
            userInfo = await AuthService.GetCurrentUserAsync();
            StateHasChanged();
        }

        private void ToggleUserMenu()
        {
            showUserMenu = !showUserMenu;
        }

        private async Task HandleLogout()
        {
            showUserMenu = false;
            await AuthService.LogoutAsync();
            NavigationManager.NavigateTo("/login", forceLoad: true);
        }

        private string GetUserInitials()
        {
            if (userInfo == null) return "U";

            // Try to get first letter of first and last name
            if (!string.IsNullOrEmpty(userInfo.Name))
            {
                var names = userInfo.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (names.Length >= 2)
                    return $"{names[0][0]}{names[1][0]}".ToUpper();
                return names[0].Substring(0, Math.Min(2, names[0].Length)).ToUpper();
            }

            // Fall back to username
            if (!string.IsNullOrEmpty(userInfo.Username))
                return userInfo.Username.Substring(0, Math.Min(2, userInfo.Username.Length)).ToUpper();

            return "U";
        }

        private string GetUserName()
        {
            if (userInfo == null) return "User";
            return userInfo.Name ?? userInfo.Username ?? "User";
        }

        private string GetUserRole()
        {
            if (userInfo?.Role == null) return "User";
            return userInfo.Role switch
            {
                (int)UserRole.Supervisor => "Supervisor",
                (int)UserRole.Administrator => "Administrator",
                (int)UserRole.Operator => "Operator",
                (int)UserRole.Viewer => "Viewer",
                _ => "User"
            };
        }
    }
}