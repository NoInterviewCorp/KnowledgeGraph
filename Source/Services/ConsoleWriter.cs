using System;
using System.Text;

namespace KnowledgeGraph.Services
{
    public static class ConsoleWriter
    {
        public static void ConsoleAnException(Exception e){
            var sb = new StringBuilder();
            sb.Append("\n---------------------EXCEPTION-MESSAGE----------------------------\n");
            sb.Append(e.Message);
            sb.Append("\n---------------------STACK-TRACE----------------------------------\n");
            sb.Append(e.StackTrace);
            sb.Append("\n---------------------INNER-EXCEPTION------------------------------\n");
            sb.Append(e.InnerException);
            sb.Append("\n------------------------------------------------------------------\n");
            Console.WriteLine(sb.ToString());
        }

    }
}