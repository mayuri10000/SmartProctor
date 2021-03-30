using System;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SmartProctor.Client.Services;
using SmartProctor.Client.Utils;
using SmartProctor.Shared.Requests;

namespace SmartProctor.Client.Pages.Exam
{
    public class FormItemLayout
    {
        public ColLayoutParam LabelCol { get; set; }
        public ColLayoutParam WrapperCol { get; set; }
    }
    public partial class CreateExam
    {
        [Inject]
        public NavigationManager NavManager { get; set; }
        
        [Inject]
        public IExamServices ExamServices { get; set; }
        
        [Inject]
        public ModalService Modal { get; set; }
        
        private readonly CreateExamRequestModel _model = new CreateExamRequestModel()
        {
            StartTime = DateTime.Now.AddDays(1),
            Duration = DateTime.Parse("1:00:00")
        };

        private DateTime _durationInDateTime;

        private readonly FormItemLayout _formItemLayout = new FormItemLayout
        {
            LabelCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty {Span = 24},
                Sm = new EmbeddedProperty {Span = 7},
            },

            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty {Span = 24},
                Sm = new EmbeddedProperty {Span = 12},
                Md = new EmbeddedProperty {Span = 10},
            }
        };

        private readonly FormItemLayout _submitFormLayout = new FormItemLayout
        {
            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24, Offset = 0},
                Sm = new EmbeddedProperty { Span = 10, Offset = 7},
            }
        };

        private async Task HandleSubmit(EditContext editContext)
        {
            var res = await ExamServices.CreateExam(_model);

            if (res != ErrorCodes.Success)
            {
                await Modal.ErrorAsync(new ConfirmOptions()
                {
                    Title = "Error",
                    Content = ErrorCodes.MessageMap[res]
                });
            }
            else
            {
                await Modal.SuccessAsync(new ConfirmOptions()
                {
                    Title = "Success",
                    Content = "Click 'OK' to add questions to exam"
                });
            }
        }
    }
}