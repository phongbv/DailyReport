using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DailyReport.Models
{
    public class WorkingHistoryModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Display(Name = "Ngày")]
        [Required(ErrorMessage = "Trường này không được để trống")]
        public System.DateTime WorkingDate { get; set; }
        [Display(Name = "Công việc xử lý")]
        [Required(ErrorMessage = "Trường này không được để trống")]
        public string WorkInDay { get; set; }
        [Required(ErrorMessage = "Trường này không được để trống")]
        [Display(Name = "% Hoàn thành")]
        public double PercentageComplete { get; set; }
        [Display(Name = "Khó khăn gặp phải")]
        public string Problem { get; set; }

    }
}