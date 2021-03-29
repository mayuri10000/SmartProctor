using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Requests;

namespace SmartProctor.Client.Pages.User
{
    public partial class Login
    {
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Inject]
        public IUserServices UserServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        private LoginRequestModel _model = new LoginRequestModel();

        private async Task HandleSubmit(EditContext editContext)
        {
            var result = await UserServices.LoginAsync(_model);

            if (result != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Login failed",
                    Content = ErrorCodes.MessageMap[result]
                });
            }
            else
            {
                // Login success
                NavManager.NavigateTo("/");
            }
        } 
    }
}