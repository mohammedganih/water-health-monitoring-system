using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            DataTable log_dataTable = new DataTable();
            log_dataTable.Columns.AddRange(new DataColumn[4] { new DataColumn("Date", typeof(DateTime)),
                    new DataColumn("Start Time", typeof(DateTime)),
                    new DataColumn("End Time",typeof(DateTime)),
                    new DataColumn("Status", typeof(string))});


            string headers = string.Empty;
            foreach (DataColumn column in log_dataTable.Columns)
            {
                headers += column.ColumnName + ",";
            }
            stringBuilder.AppendLine(headers);
            File.AppendAllText("Log-analysis-report.csv", stringBuilder.ToString());
            log_dataTable.Clear();

            for (int i = 1; i <= 31; i++)
            {

                string fileName = "OperaWater_Edge.log.2019-12-" + i.ToString("00");

                //the path of the file
                FileStream logfile = new FileStream(@"D:\WHMS\UBANLOG\" + fileName, FileMode.Open, FileAccess.Read);
                StreamReader reader = new StreamReader(logfile);

                string first_line;
                string newLine = "";

                List<string> startTime = new List<string>();
                List<string> endTime = new List<string>();
                List<string> date = new List<string>();

                List<string> no_connection_startTime = new List<string>();
                List<string> no_connection_endTime = new List<string>();
                List<string> no_connection_date = new List<string>();

                StringBuilder str = new StringBuilder();

                try
                {
                    //the program reads the first_line       
                    first_line = reader.ReadLine();
                    //str.Insert(0, "");
                    while (first_line != null)
                    {

                        if (!string.IsNullOrWhiteSpace(first_line))
                        {
                            //checking for the keyword "sending payload" if exist in that line
                            if (first_line.Contains("sending payload"))
                            {
                                string first_line_Time = first_line.Substring(10, 10);
                                startTime.Add(first_line_Time);

                                reader.ReadLine();
                                string third_line = reader.ReadLine();


                                //checking if connection was available for that timestamp

                                if (!string.IsNullOrWhiteSpace(third_line) && third_line.Contains("connection not available"))
                                {
                                    str.AppendLine(date.First() + "," + startTime.First() + "," + endTime.Last() + "," + "Connection Success");
                                    date.Clear();
                                    startTime.Clear();
                                    endTime.Clear();

                                    string third_line_Date = third_line.Substring(0, 10);
                                    string third_line_Time = third_line.Substring(10, 10);
                                    string first_line_Date = first_line.Substring(0, 10);
                                    first_line_Time = first_line.Substring(10, 10);

                                    newLine = string.Format("{0},{1},{2},{3}", first_line_Date, first_line_Time, third_line_Time, "Connection Failed");
                                    str.AppendLine(newLine);

                                }
                                else if (!string.IsNullOrWhiteSpace(third_line))
                                {
                                    string third_line_Date = third_line.Substring(0, 10);
                                    string third_line_Time = third_line.Substring(10, 10);
                                    string first_line_Date = first_line.Substring(0, 10);

                                    date.Add(first_line_Date);
                                    endTime.Add(third_line_Time);
                                }

                            }

                            //checking for whole day no connection

                            else if (first_line.Contains("connection not available"))
                            {

                                string second_line = reader.ReadLine();


                                if (!string.IsNullOrWhiteSpace(second_line) && second_line.Contains("connection not available"))
                                {

                                    string first_line_Time = first_line.Substring(10, 10);
                                    string first_line_Date = first_line.Substring(0, 10);
                                    string second_line_Time = second_line.Substring(10, 10);
                                    no_connection_date.Add(first_line_Date);
                                    no_connection_startTime.Add(first_line_Time);
                                    no_connection_endTime.Add(second_line_Time);

                                }

                            }

                        }

                        first_line = reader.ReadLine();

                    }
                    if (startTime.Count > 0 && endTime.Count > 0)
                    {
                        str.AppendLine(date.First() + "," + startTime.First() + "," + endTime.Last() + "," + "Connection Success");
                    }
                    if (no_connection_startTime.Count > 0 && no_connection_endTime.Count > 0)
                    {
                        str.AppendLine(no_connection_date.First() + "," + no_connection_startTime.First() + "," + no_connection_endTime.Last() + "," + "Connection was not established");
                        no_connection_date.Clear();
                        no_connection_startTime.Clear();
                        no_connection_endTime.Clear();
                    }

                    File.AppendAllText("Log-analysis-report.csv", str.ToString());

                }

                finally
                {
                    //after entire lines of the files has been read, the progam and reader closes
                    reader.Close();
                    logfile.Close();
                }
            }
        }
    }
}
