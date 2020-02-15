using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class TopicsAttended
    {
        public int TopicID { get; set; }
        public string TopicName { get; set; }
        public string TopicDescription { get;set;} 
        public int UserID { get; set; }
        public string AssignedTask { get; set; }
        public string TaskDescription { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskEndDate { get; set; }
        public string TaskStatus { get; set; }
    }
}