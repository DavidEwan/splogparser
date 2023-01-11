﻿using Contract;
using Impl;
using System;
using System.Collections.Generic;
using System.Data;

namespace HDCUView
{
   /// <summary>
   /// class for processing loglines containing 'HCDUSensor::UpdateSensor'
   /// </summary>
   internal class HDCUTable : BaseTable
   {

      /// <summary>
      /// constructor
      /// </summary>
      /// <param name="ctx">Context for the command.</param>
      /// <param name="viewName">The (unique) name of the view being created.</param>
      public HDCUTable(IContext ctx, string viewName) : base(ctx, viewName)
      {
         // for our view we want '0' to render as ' ' in the worksheet
         _zeroAsBlank = true;

         InitDataTable();
      }

      protected override void InitDataTable()
      {
         base.InitDataTable();

         this.dTable.Columns.Add("RjFull", typeof(string));
         this.dTable.Columns.Add("RjMiss", typeof(string));

         for (int i = 1; i <= 6; i++)
         {
            this.dTable.Columns.Add("C" + i.ToString() + "Miss", typeof(string));
            this.dTable.Columns.Add("C" + i.ToString() + "Emty", typeof(string));
            this.dTable.Columns.Add("C" + i.ToString() + "Low", typeof(string));
         }
      }

      /// <summary>
      /// Wrapper function to call Utilities.FindByMarker but also handle return error logging
      /// </summary>
      /// <param name="logLine">the current log line</param>
      /// <param name="mark">string to search for in the log line (start marker)</param>
      /// <param name="endMark"></param>
      /// <returns>substring of logline book-ended by mark and endMark, or error.</returns>
      private (bool found, string foundStr, string subLogLine) Find(string logLine, string mark, string endMark)
      {
         (bool found, string foundStr, string subLogLine) result;
         result = LogFind.FindByMarker(logLine, mark, endMark);
         if (!result.found)
         {
            // can't continue 
            ctx.ConsoleWriteLogLine("CashInTable.ProcessRow - Failed to find '" + mark + "'");
            return (false, string.Empty, logLine);
         }
         return (true, result.foundStr, result.subLogLine);
      }

      /// <summary>
      /// Process one line from the merged log file. 
      /// </summary>
      /// <param name="logLine">logline from the file</param>
      public override void ProcessRow(string traceFile, string logLine)
      {
         try
         {
            if (string.IsNullOrEmpty(logLine))
            {
               return;
            }

            // This log line is for us if it contains HCDUSensor::UpdateSensor
            if (!(logLine.Contains("HCDUSensor::UpdateSensor") && 
                  logLine.Contains("Shutter Open = [") &&
                  logLine.Contains("ITem Taken = [") &&
                  logLine.Contains("Stacker Empty = [") &&
                  logLine.Contains("Reject Full = [") &&
                  logLine.Contains("Carriage Home Position = [") &&
                  logLine.Contains("Cst#1 Missing = [") &&
                  logLine.Contains("Cst#2 Missing = [") &&
                  logLine.Contains("Cst#3 Missing = [")))
            {
               return;
            }

            base.ProcessRow(traceFile, logLine);

            DataRow dataRow = dTable.NewRow();

            dataRow["File"] = _traceFile;
            dataRow["Time"] = _logDate; 

            string subLogLine = logLine;

            // Shutter Open = [0], Lock = [1], Close = [1]

            (bool found, string foundStr, string subLogLine) result;

            // Reject Full = [0], Missing = [0]

            result = Find(subLogLine, "Reject Full = [", "]");
            if (!result.found)
            {
               return;
            }
            subLogLine = result.subLogLine;

            dataRow["RjFull"] = result.foundStr;


            result = Find(subLogLine, "Missing = [", "]");
            if (!result.found)
            {
               return;
            }
            subLogLine = result.subLogLine;

            dataRow["RjMiss"] = result.foundStr;

            // Cst#1 Missing = [0], Empty = [0], Low = [0]

            for (int i = 1; i <= 6; i++)
            {
               result = Find(subLogLine, "Missing = [", "]");
               if (!result.found)
               {
                  return;
               }
               subLogLine = result.subLogLine;

               dataRow["C" + i.ToString() + "Miss"] = result.foundStr;


               result = Find(subLogLine, "Empty = [", "]");
               if (!result.found)
               {
                  return;
               }
               subLogLine = result.subLogLine;

               dataRow["C" + i.ToString() + "Emty"] = result.foundStr;

               result = Find(subLogLine, "Low = [", "]");
               if (!result.found)
               {
                  return;
               }
               subLogLine = result.subLogLine;

               dataRow["C" + i.ToString() + "Low"] = result.foundStr;
            }

            dTable.Rows.Add(dataRow);

         }
         catch (Exception e)
         {
            ctx.LogWriteLine("HDCUTable.ProcessRow EXCEPTION:" + e.Message);
         }

         return;
      }
   }
}
