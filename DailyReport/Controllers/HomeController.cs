using DailyReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DailyReport.Controllers
{
    public class HomeController : Controller
    {
        User currentUser = null;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var forwardAdd = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            var clientAddress = forwardAdd ?? Request.ServerVariables["REMOTE_ADDR"];
            using (var dbContext = new DataContext())
            {
                bool isLocal = Request.IsLocal;
                currentUser = isLocal ? dbContext.Users.FirstOrDefault(e=>e.Username == "test"):dbContext.Users.FirstOrDefault(e => e.TrustedIP.Contains(clientAddress));
                if (currentUser == null)
                {
                    filterContext.Result = Content($"Địa chỉ {clientAddress} không có quyền truy cập. Vui lòng kiểm tra lại.");
                    return;
                }
            }
            base.OnActionExecuting(filterContext);
        }

        // GET: Home
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
                workingHistoryModel.UserId = currentUser.Id;
                DoSaveWorkingHistory(workingHistoryModel);
            }
            return DoCallbackWorkingHistory();
        }
        public ActionResult DoAddNewWorkingHistory(WorkingHistoryModel workingHistoryModel)
        {
            if (ModelState.IsValid)
            {
                workingHistoryModel.UserId = currentUser.Id;
                DoSaveWorkingHistory(workingHistoryModel);
            }
            return DoCallbackWorkingHistory();
        }
        private List<WorkingHistoryModel> GetWorkingHistoryByUser()
        {
            using (var dbContext = new DataContext())
            {
                var lstWorking = dbContext.WorkingHistories.Where(e => e.UserId == currentUser.Id).Select(e => new WorkingHistoryModel()
                {
                    Id = e.Id,
                    PercentageComplete = e.PercentageComplete,
                    Problem = e.Problem,
                    UserId = e.UserId,
                    WorkInDay = e.WorkInDay,
                    WorkingDate = e.WorkingDate

                }).OrderByDescending(e=>e.WorkingDate).ToList();
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