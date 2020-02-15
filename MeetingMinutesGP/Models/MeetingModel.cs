using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class MeetingModel
    { 

     public int MeetingID { set; get; }
    public int MeetingDuration { set; get; }
    public string MeetingDate { set; get; }
    public string Status { set; get; }
    public int locationID { set; get; }

    }
}