using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Requests;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Pages.User
{
    public partial class UserDetails
    {
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Inject]
        public IUserServices UserServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        private UserDetailsResponseModel _userDetails = new UserDetailsResponseModel();
        private UserDetailsRequestModel _model = new UserDetailsRequestModel();

        private bool _modifyPasswordModalVisible = false;
        private ModifyPasswordRequestModel _modifyPasswordModel = new ModifyPasswordRequestModel();
            
        protected override async Task OnInitializedAsync()
        {
            var (res, details) = await UserServices.GetUserDetails();

            if (res != ErrorCodes.Success)
            {
                if (res != ErrorCodes.NotLoggedIn)
                {
                    await Modal.ErrorAsync(new ConfirmOptions()
                    {
                        Title = "You must log in first"
                    });
                    NavManager.NavigateTo("/User/Login");
                }
                else
                {
                    await Modal.ErrorAsync(new ConfirmOptions()
                    {
                        Title = "Cannot get user information",
                        Content = ErrorCodes.MessageMap[res]
                    });
                }
            }
            else
            {
                _userDetails = details;
                _model.Email = details.Email;
                _model.Phone = details.Phone;
                _model.NickName = details.NickName;
                StateHasChanged();
            }
        }

        private async Task HandleFinish(EditContext ctx)
        {
            var res = await UserServices.UpdateUserDetails(_model);

            if (res == ErrorCodes.Success)
            {
                await Modal.SuccessAsync(new ConfirmOptions()
                {
                    Content = "User information has been successfully updated"
                });

                await OnInitializedAsync();
            }
            else
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot update user information",
                    Content = ErrorCodes.MessageMap[res]
                });
            }
        }

        private void OnModifyPassword()
        {
            _modifyPasswordModalVisible = true;
        }

        private void OnModifyPasswordCancel()
        {
            _modifyPasswordModalVisible = false;
        }

        private async Task OnModifyPasswordFinish(EditContext ctx)
        {
            var res = await UserServices.ModifyPassword(_modifyPasswordModel);

            if (res == ErrorCodes.Success)
            {
                await Modal.SuccessAsync(new ConfirmOptions()
                {
                    Content = "Password has been successfully updated"
                });

                _modifyPasswordModalVisible = false;
            }
            else
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Cannot update password",
                    Content = ErrorCodes.MessageMap[res]
                });
            }
        }

        private void OnAvatarUploadStatusChange(UploadInfo u)
        {
            if (u.File.State == UploadState.Success)
            {
                Modal.Success(new ConfirmOptions()
                {
                    Content = "Avatar upload success"
                });
            }
            else if (u.File.State == UploadState.Fail)
            {
                Modal.Error(new ConfirmOptions()
                {
                    Content = "Avatar upload failed"
                });
            }
        }
    }
}