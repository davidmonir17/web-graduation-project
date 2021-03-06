//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MeetingMinutesGP.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Topic
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Topic()
        {
            this.Topic1 = new HashSet<Topic>();
            this.Votes = new HashSet<Vote>();
        }
    
        public int TopicID { get; set; }
        public string TopicName { get; set; }
        public string TopicDescription { get; set; }
        public Nullable<int> TopicTime { get; set; }
        public string TopicPriority { get; set; }
        public int agendaId { get; set; }
        public string ListOfItems { get; set; }
        public string FileLocation { get; set; }
        public Nullable<int> subTopicId { get; set; }
        public string TopicImpWords { get; set; }
    
        public virtual Agendum Agendum { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Topic> Topic1 { get; set; }
        public virtual Topic Topic2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vote> Votes { get; set; }
    }
}
