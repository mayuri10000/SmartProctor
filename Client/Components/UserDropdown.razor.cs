using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;

namespace SmartProctor.Client.Components
{
    public partial class UserDropdown
    {
        [Parameter]
        public string UserAvatar { get; set; }
    
        [Parameter]
        public string UserName { get; set; }
        
        [Inject]
        public IUserServices UserServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        [Inject]
        public NavigationManager NavManager { get; set; }

        private void GoToSettings()
        {
            NavManager.NavigateTo("/User/Details");
        }

        private async Task Logout()
        {
            var confirm = await Modal.ConfirmAsync(new ConfirmOptions()
            {
                Title = "Are you sure to log out?",
            });

            if (!confirm)
            {
                return;
            }
            
            var res = await UserServices.LogoutAsync();

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot logout",
                    Content = ErrorCodes.MessageMap[res]
                });
                
                return;
            }

            await Modal.SuccessAsync(new ConfirmOptions()
            {
                Title = "Logout successfully"
            });
            
            NavManager.NavigateTo("/User/Login");
        }
    }
}