using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeetingMinutesGP.Models
{
    public class TopicTimePred
    {
        public string TopicName { get; set; }
        public int TopicTime { get; set; }
        public double similarity { get; set; }
    }
}