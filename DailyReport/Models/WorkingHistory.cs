//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DailyReport.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WorkingHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public System.DateTime WorkingDate { get; set; }
        public string WorkInDay { get; set; }
        public double PercentageComplete { get; set; }
        public string Problem { get; set; }
    
        public virtual User User { get; set; }
    }
}
