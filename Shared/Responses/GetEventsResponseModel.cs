using System;
using System.Collections.Generic;

namespace SmartProctor.Shared.Responses
{
    public class EventItem
    {
        public int Type { get; set; }
        public string Sender { get; set; }
        public string Receipt { get; set; }
        public string Message { get; set; }
        public string Attachment { get; set; }
        public DateTime Time { get; set; }
    }
    
    public class GetEventsResponseModel : BaseResponseModel
    {
        public IList<EventItem> Events { get; set; }
    }
}