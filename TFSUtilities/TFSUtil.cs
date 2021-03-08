using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.Console;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TFSUtilities
{
    public class TFSUtil
    {
        static WorkItemStore workItemStore;

        static string teamProjectName = "iLendingPro";
        static Uri tfsUri = new Uri("http://sptserver.ists.com.vn:8080/tfs/" + teamProjectName);
        static TfsTeamProjectCollection tpc = new TfsTeamProjectCollection(tfsUri);
        static VersionControlServer vcs;
        Dictionary<string, string> variables = new Dictionary<string, string> { { "project", teamProjectName } };
        static TFSUtil()
        {
            tpc.Authenticate();
            vcs = tpc.GetService<VersionControlServer>();
            workItemStore = new WorkItemStore(tpc);
        }

        public List<WorkItemWrapper> GetWorkItemsChangeInToday()
        {
            string query = @"SELECT
        [System.Id],
        [System.WorkItemType],
        [System.Title],
        [System.State],
        [System.AreaPath],
        [System.IterationPath]
FROM workitems
WHERE
        [System.TeamProject] = @project
        AND System.ChangedDate >= @today
        AND System.WorkItemType in (""Product Backlog Item"",""Customer Backlog Item"", ""Bug"")
        AND System.State IN(""Committed"", ""Testing"", ""Done"")
ORDER BY[System.ChangedDate] DESC";
            
            var workItemCollection = workItemStore.Query(query, variables).OfType<WorkItem>().ToList();
            return workItemCollection.Select(e => new WorkItemWrapper(vcs, e)).ToList();
        }

        public WorkItemWrapper GetById(int witId)
        {
            return new WorkItemWrapper(vcs, workItemStore.GetWorkItem(witId));
        }
        //        public List<WorkItemInfo> GetNewWit()
        //        {
        //            string query = @"SELECT
        //        [System.Id],
        //        [System.WorkItemType],
        //        [System.Title],
        //        [System.State],
        //        [System.AreaPath],
        //        [System.IterationPath]
        //FROM workitems
        //WHERE
        //        [System.TeamProject] = @project
        //        and [System.WorkItemType] in(""Product Backlog Item"", ""Customer Backlog Item"")
        //       AND [SYStem.State] in (""New"",""InProgress"", ""In Progress"", ""Transfer Requirement"")
        //       AND [System.IterationPath] NOT IN(""iLendingPro\LVPB - Performance Test"", ""iLendingPro\NCB - Credit Rating"", ""iLendingPro\Next Release"", ""iLendingPro\VIB Demo"", ""iLendingPro\LOS - Version 2.0"")
        //ORDER BY[System.IterationPath], [SYStem.State], [System.ChangedDate] DESC";
        //            Dictionary<string, string> variables = new Dictionary<string, string> { { "project", teamProjectName } };
        //            var workItemColl = workItemStore.Query(query, variables).OfType<WorkItem>().ToList();

        //            var lstWorkItem = workItemColl.Select(e => new WorkItemInfo { WorkItem = e }).ToList();
        //            return lstWorkItem;
        //        }

    }

}
