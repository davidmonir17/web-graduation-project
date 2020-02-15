using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class Task
    {
        public int TaskID { get; set; }
        public string TaskInitiatorEmail { get; set; }
        public string TaskOwnerEmail { get; set; }
        public string TaskName { get; set; }
        public string TaskDescription { get; set; }
        public DateTime TaskStartDate { get; set; }
        public DateTime TaskEndDate { get; set; }
        public string TaskStatus { get; set; }
        public int UserID { get; set; }
        public int MeetingID { get; set; }
    }
}