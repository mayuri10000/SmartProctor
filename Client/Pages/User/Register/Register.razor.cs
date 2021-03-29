using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Requests;

namespace SmartProctor.Client.Pages.User
{
    public partial class Register
    {
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Inject]
        public IUserServices UserServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }

        private RegisterRequestModel _model = new RegisterRequestModel();

        private async Task OnFinish(EditContext editContext)
        {
            var result = await UserServices.RegisterAsync(_model);

            if (result != ErrorCodes.Success)
            {
                Modal.Error(new ConfirmOptions()
                {
                    Title = "Register failed",
                    Content = ErrorCodes.MessageMap[result]
                });
            }
            else
            {
                await Modal.SuccessAsync(new ConfirmOptions()
                {
                    Content = "Register OK"
                });
                NavManager.NavigateTo("/User/Login");
            }
        }
    }
}