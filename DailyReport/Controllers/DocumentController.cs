using DailyReport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TFSUtilities;

namespace DailyReport.Controllers
{
    public class DocumentController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (User.IsAdmin == false)
            {
                filterContext.Result = Content("Không có quyền nha cưng");
            }
            base.OnActionExecuting(filterContext);
        }
        static TFSUtil util = new TFSUtil();
        // GET: Document
        public ActionResult Index()
        {
            UpdateWorkItemInBackground();
            return View(GetDocumentReporting());
        }
        public ActionResult DoCallbackDocumentReporting()
        {
            return PartialView("~/Views/Document/Index.cshtml" ,GetDocumentReporting());
        }
        private List<DocumentReporting> GetDocumentReporting()
        {
            using (var dbContext = new DataContext())
            {
                var lstWorking = dbContext.DocumentReportings.OrderByDescending(e => e.UpdateDate).ToList();
                return lstWorking;
            }
        }
        static Object lockObj = new Object();
        TimeSpan timeout = TimeSpan.FromMilliseconds(1);
        private void UpdateWorkItemInBackground()
        {
            Task.Run(() =>
            {
                bool lockTaken = false;

                try
                {
                    Monitor.TryEnter(lockObj, timeout, ref lockTaken);
                    if (lockTaken)
                    {
                        foreach (var item in util.GetWorkItemsChangeInToday())
                        {
                            using (var dbContext = new DataContext())
                            {
                                var reportInfo = dbContext.DocumentReportings.FirstOrDefault(e => e.WorkItemId == item.WorkItemId);
                                if (reportInfo == null)
                                {
                                    reportInfo = new DocumentReporting()
                                    {
                                        WorkItemId = item.WorkItemId
                                    };
                                    dbContext.DocumentReportings.Add(reportInfo);
                                }
                                reportInfo.State = item.State;
                                reportInfo.UpdateDate = item.ChangeDate;
                                if (string.IsNullOrEmpty(item.Summary))
                                {
                                    reportInfo.IsComplete = true;
                                }
                                reportInfo.Summary = item.Summary;
                                dbContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                finally
                {
                    // Ensure that the lock is released.
                    if (lockTaken)
                    {
                        Monitor.Exit(lockObj);
                    }
                }
            });
        }

        public void DoCompleteWorkItem(int id)
        {
            
            using (var dbContext = new DataContext())
            {
                var wit = dbContext.DocumentReportings.FirstOrDefault(e => e.Id == id);
                if (wit != null)
                {
                    wit.IsComplete = true;
                    dbContext.SaveChanges();
                }
            }
            //return RedirectToAction(nameof(Index));
        }

    }
}