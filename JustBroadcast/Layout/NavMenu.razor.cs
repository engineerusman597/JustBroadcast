using JustBroadcast.Models;

namespace JustBroadcast.Layout
{
    public partial class NavMenu
    {
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

        private bool CanShowAdministration()
        {
            if (userInfo?.Role == null) return false;
            return userInfo.Role == (int)UserRole.Supervisor || userInfo.Role == (int)UserRole.Administrator;
        }

        private bool CanShowPlayoutControl()
        {
            if (userInfo?.Role == null) return false;
            // Hide for Viewer and Administrator
            if (userInfo.Role == (int)UserRole.Viewer || userInfo.Role == (int)UserRole.Administrator) return false;
            // Show if RemotePlayouts list is not empty
            return userInfo.RemotePlayouts != null && userInfo.RemotePlayouts.Any();
        }

        private bool CanShowScheduler()
        {
            if (userInfo?.Role == null) return false;
            // Hide for Viewer and Administrator
            if (userInfo.Role == (int)UserRole.Viewer || userInfo.Role == (int)UserRole.Administrator) return false;
            // Show if SchedulerChannels list is not empty
            return userInfo.SchedulerChannels != null && userInfo.SchedulerChannels.Any();
        }

        private string GetUserInitial()
        {
            if (userInfo == null) return "U";
            if (!string.IsNullOrEmpty(userInfo.Username))
                return userInfo.Username.Substring(0, 1).ToUpper();
            if (!string.IsNullOrEmpty(userInfo.Name))
                return userInfo.Name.Substring(0, 1).ToUpper();
            return "U";
        }

        private string GetRoleName()
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