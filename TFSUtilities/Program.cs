using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;
namespace TFSUtilities
{
    class Program
    {
        static void Main(string[] args)
        {
            TFSUtil util = new TFSUtil();
            var lstMissingQAndA = util.GetWorkItemsChangeInToday().Where(e => e.IsMissingQAndA).ToList();
            WriteLine("Danh sach thieu Q&A: ");
            foreach (var item in lstMissingQAndA)
            {
                WriteLine($"{item.WorkItem.Type.Name}: {item.WorkItem.Id}");
            }
            Console.ReadKey();
        }
    }
}
