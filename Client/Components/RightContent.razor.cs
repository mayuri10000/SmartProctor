using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Responses;

namespace SmartProctor.Client.Components
{
    public partial class RightContent
    {
        [Inject]
        public IUserServices UserServices { get; set; }

        private string _avatar = "";

        private string _userName = "Not logged in";

        protected override async Task OnInitializedAsync()
        {
            var (res, details) = await UserServices.GetUserDetails();
            
            Console.WriteLine(details == null);

            if (res == ErrorCodes.Success)
            {
                _avatar = details.Avatar ?? "";
                _userName = details.NickName;
            }
            
            Console.WriteLine(res);
            
            StateHasChanged();
        }
    }
}