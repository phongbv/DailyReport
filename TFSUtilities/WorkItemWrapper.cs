using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities
{
    public class WorkItemWrapper
    {
        private VersionControlServer _versionControlServer;
        private static readonly string TechSpecFolderPath = "$/iLendingPro/Documents/17. Technical Spec/";
        public WorkItemWrapper(VersionControlServer versionControlServer, WorkItem workItem)
        {
            _versionControlServer = versionControlServer;
            WorkItem = workItem;
        }
        public WorkItem WorkItem { get; private set; }
        public int WorkItemId => WorkItem.Id;
        public string State => WorkItem.State;
        public DateTime ChangeDate => WorkItem.ChangedDate;
        private string _sourcePrefix
        {
            get
            {
                string tmp = "";
                switch (WorkItem.Type.Name)
                {
                    case "Product Backlog Item":
                    case "Customer Backlog Item":
                        tmp = "BL";
                        break;

                    default:
                        break;
                }
                return tmp + "_" + WorkItem.Id;
            }
        }
        private string DocumentFolderPath => TechSpecFolderPath + _sourcePrefix;
        private string FilePrefix => DocumentFolderPath + "/" + _sourcePrefix + "_";
        public string TechSpecFilePath => FilePrefix + "Tech spec.docx";
        public string MinimumFilePath => FilePrefix + "Minimum TC.xlsx";
        public string QAndAFilePath => FilePrefix + "Q&A.xlsx";

        public bool IsByPassQAndA => (WorkItem.Fields.TryGetField("ISTS.ByPassQAndA") + "").ToUpper() == "YES";
        public bool IsByPassTechSpec => (WorkItem.Fields.TryGetField("ISTS.ByPassTechSpec") + "").ToUpper() == "YES";
        public bool IsByPassMinimumTC => (WorkItem.Fields.TryGetField("ISTS.ByPassMinimumTC") + "").ToUpper() == "YES";

        public bool IsMissingQAndA => WorkItem.State == "Done" && IsByPassQAndA == false && _versionControlServer.TryGetItem(QAndAFilePath) == null;
        public bool IsMissingTechSpec => IsByPassTechSpec == false && _versionControlServer.TryGetItem(TechSpecFilePath) == null;
        public bool IsMissingMinimumTC => IsByPassMinimumTC == false && _versionControlServer.TryGetItem(MinimumFilePath) == null;

        public string Summary
        {
            get
            {
                List<string> detailList = new List<string>();
                if (IsMissingTechSpec)
                {
                    detailList.Add("Thiếu tài liệu tech spec");
                }
                if (IsMissingMinimumTC)
                {
                    detailList.Add("Thiếu tài liệu minimum test case");
                }
                if (IsMissingQAndA)
                {
                    detailList.Add("Thiếu tài liệu Q&A");
                }
                return detailList.Any() ? string.Join(", ", detailList) : null;
            }
        }

    }

    public static class TFSExtension
    {
        public static object TryGetField(this FieldCollection collection, string fieldName)
        {
            var fields = collection.OfType<Field>();
            return fields.FirstOrDefault(e => e.FieldDefinition.ReferenceName == fieldName)?.Value;
        }
        public static Item TryGetItem(this VersionControlServer version, string path)
        {
            try
            {
                return version.GetItem(path);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
