using DailyReport.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyReport.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        public static readonly bool CanViewOtherTasks = bool.Parse(ConfigurationManager.AppSettings["view-all-tasks"]);

        public ActionResult Index()
        {
            return View(GetWorkingHistoryByUser());
        }
        public ActionResult DoCallbackWorkingHistory()
        {

            return PartialView("~/Views/Home/Index.cshtml", GetWorkingHistoryByUser());
        }
        public ActionResult DoDeleteWorkingHistory(int id)
        {
            using (var dbContext = new DataContext())
            {
                var entity = dbContext.WorkingHistories.Find(id);
                if (entity != null)
                {
                    if (entity.WorkingDate.Date <= DateTime.Today.AddDays(-2)) throw new InvalidOperationException("Không thể xóa công việc nếu vượt quá 2 ngày");
                    dbContext.WorkingHistories.Remove(entity);
                    dbContext.SaveChanges();
                }
            }
            return DoCallbackWorkingHistory();
        }
        public ActionResult DoUpdateWorkingHistory(WorkingHistoryModel workingHistoryModel)
        {
            if (ModelState.IsValid)
            {
                workingHistoryModel.UserId = User.Id;
                DoSaveWorkingHistory(workingHistoryModel);
            }
            return DoCallbackWorkingHistory();
        }
        public ActionResult DoAddNewWorkingHistory(WorkingHistoryModel workingHistoryModel)
        {
            if (ModelState.IsValid)
            {
                workingHistoryModel.UserId = User.Id;
                DoSaveWorkingHistory(workingHistoryModel);
            }
            return DoCallbackWorkingHistory();
        }
        private List<WorkingHistoryModel> GetWorkingHistoryByUser()
        {
            using (var dbContext = new DataContext())
            {
                var lstWorking = dbContext.WorkingHistories.Where(e => e.UserId == User.Id || User.IsAdmin || CanViewOtherTasks).Select(e => new WorkingHistoryModel()
                {
                    Id = e.Id,
                    PercentageComplete = e.PercentageComplete,
                    Problem = e.Problem,
                    UserId = e.UserId,
                    WorkInDay = e.WorkInDay,
                    WorkingDate = e.WorkingDate,
                    Username = e.User.Username

                }).OrderByDescending(e => e.WorkingDate).ToList();
                return lstWorking;
            }
        }


        private void DoSaveWorkingHistory(WorkingHistoryModel workingHistoryModel)
        {
            using (var dbContext = new DataContext())
            {
                var entity = dbContext.WorkingHistories.Find(workingHistoryModel.Id);
                if (entity == null)
                {
                    entity = new WorkingHistory();
                    dbContext.WorkingHistories.Add(entity);
                }
                else if(entity.UserId != workingHistoryModel.UserId)
                {
                    throw new Exception("Bạn không có quyền chỉnh sửa công việc của người khác");
                }
                entity.UserId = workingHistoryModel.UserId;
                entity.PercentageComplete = workingHistoryModel.PercentageComplete;
                entity.Problem = workingHistoryModel.Problem;
                entity.WorkInDay = workingHistoryModel.WorkInDay;
                entity.WorkingDate = workingHistoryModel.WorkingDate.Date;
                dbContext.SaveChanges();
            }
        }
    }
}