﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Impl
{
   public static class _wfs_note_numbers
   {
      public static string[,] NoteNumberListFromTable(int lUnitCount, string logLine)
      {
         // resize the lpNoteNumberList array to hold all note numbers for all logical units
         string [,] lpNoteNumberList = new string[lUnitCount, 20];

         int indexOfTable = logLine.IndexOf("lpNoteNumberList->");
         string subLogLine = logLine.Substring(indexOfTable + "lpNoteNumberList->\r\n".Length);

         indexOfTable = subLogLine.IndexOf("\t\t\tLCU ETC");
         if (indexOfTable > 0)
         {
            // this is the note number list which we process
            subLogLine = subLogLine.Substring(0, indexOfTable);
         }

         // iterate over each line of the block and load up the array
         int colCount = 0; 
         char[] trimChars = { '\t' };
         (bool found, string oneLine, string subLogLine) result = LogLine.ReadNextLine(subLogLine);

         while (result.found)
         {
            result.oneLine = result.oneLine.TrimStart(trimChars).Replace("\t\t", ",");
            string[] match = result.oneLine.Split(',');
            for (int i = 0; i < lUnitCount; i++)
            {
               // i + 1 because the first column is throw away
               match[i + 1] = match[i + 1].TrimStart(trimChars);
               if (match[i + 1] == "")
               {
                  lpNoteNumberList[i, colCount] = string.Empty;
               }
               else
               {
                  // store as a string 'noteID:count'
                  lpNoteNumberList[i, colCount] = match[i + 1].Replace(']', ':').Replace("[","");
               }
            }
            // move onto the next log line, to fill in the next column
            result = LogLine.ReadNextLine(result.subLogLine);
            colCount++;
         }
         return lpNoteNumberList;
      }

      public static string[,] NoteNumberListFromList(int lUnitCount, string logLine)
      {
         string[,] lppNoteNumbers = new string[lUnitCount, 20];
         for (int i = 0; i < lUnitCount; i++)
            for (int j = 0; j < 20; j++)
               lppNoteNumbers[i, j] = null;


         // how many baknote types are there? 
         (bool success, string xfsMatch, string subLogLine) result = usNumOfNoteNumbers(logLine);
         if (result.success && int.Parse(result.xfsMatch.Trim()) == 0)
            return lppNoteNumbers; 

         (string thisUnit, string nextUnits) logicalUnits = _wfs_base.NextLogicalUnit(logLine);

         for (int i = 0; i < lUnitCount; i++)
         {
            string[] usNoteIDs = usNoteIDsFromList(logicalUnits.thisUnit);
            string[] ulCounts = ulCountsFromList(logicalUnits.thisUnit);

            for (int j = 0; j < usNoteIDs.Length; j++)
            {
               lppNoteNumbers[i, j] = usNoteIDs[j] + ":" + ulCounts[j];
            }

            logicalUnits = _wfs_base.NextLogicalUnit(logicalUnits.nextUnits);
         }
            
         return lppNoteNumbers; 
      }

      // usNumOfNoteNumbers  - number of BankNote Types 
      public static (bool success, string xfsMatch, string subLogLine) usNumOfNoteNumbers(string logLine)
      {
         return _wfs_base.GenericMatchList(logLine, "(?<=usNumOfNoteNumbers = \\[)(\\d+)");
      }

      public static string[] usNoteIDsFromList(string logLine)
      {
         List<string> values = new List<string>();
         (bool success, string xfsMatch, string subLogLine) result = usNumOfNoteNumbers(logLine);
         int usCount = int.Parse(result.xfsMatch.Trim());
         (string thisUnit, string nextUnits) logicalUnits = _wfs_base.NextLogicalUnit(result.subLogLine);

         for (int i = 0; i < usCount; i++)
         {
            values.Add(usNoteID(logicalUnits.thisUnit).xfsMatch.Trim());
            logicalUnits = _wfs_base.NextLogicalUnit(logicalUnits.nextUnits);
         }
         return _wfs_base.TrimAll(_wfs_base.Resize(values.ToArray(), usCount));
      }

      public static string[] ulCountsFromList(string logLine)
      {
         List<string> values = new List<string>();
         (bool success, string xfsMatch, string subLogLine) result = usNumOfNoteNumbers(logLine);
         int usCount = int.Parse(result.xfsMatch.Trim());
         (string thisUnit, string nextUnits) logicalUnits = _wfs_base.NextLogicalUnit(result.subLogLine);

         for (int i = 0; i < usCount; i++)
         {
            values.Add(ulCount(logicalUnits.thisUnit).xfsMatch.Trim());
            logicalUnits = _wfs_base.NextLogicalUnit(logicalUnits.nextUnits);
         }
         return _wfs_base.TrimAll(_wfs_base.Resize(values.ToArray(), usCount));
      }

      // usNoteID  
      public static (bool success, string xfsMatch, string subLogLine) usNoteID(string logLine)
      {
         return _wfs_base.GenericMatchList(logLine, "(?<=usNoteID = \\[)(\\d+)");
      }

      // ulCount  - singular search from a list-style log line
      public static (bool success, string xfsMatch, string subLogLine) ulCount(string logLine)
      {
         return _wfs_base.GenericMatchList(logLine, "(?<=ulCount = \\[)(\\d+)");
      }
   }
}
