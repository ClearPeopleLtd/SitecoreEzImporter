using EzImporter.Configuration;
using EzImporter.Pipelines.ImportItems;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// Part of this code has been taken from: https://growlofowl.com/2016/11/18/simple-csv-parser-in-c-with-comma-in-cell-support/
/// </summary>
namespace EzImporter.DataReaders
{
    public class CsvDataReader : IDataReader
    {
        public void ReadData(ImportItemsArgs args)
        {
            Log.Info("EzImporter:Reading CSV input data...", this);
            try
            {
                var reader = new StreamReader(args.FileStream);
                var insertLineCount = 0;
                var readLineCount = 0;
                do
                {
                    var line = reader.ReadLine();
                    readLineCount++;
                    if (line == null
                        || (readLineCount == 1 && args.ImportOptions.FirstRowAsColumnNames))
                    {
                        continue;
                    }

                    var row = args.ImportData.NewRow();

                    var csvDelimiters = new HashSet<char>();
                    foreach (var delimiter in args.ImportOptions.CsvDelimiter) foreach (var delimiterChar in delimiter) csvDelimiters.Add(delimiterChar);
                    var quotationMark = (!string.IsNullOrEmpty(args.ImportOptions.QuotationMark) ? args.ImportOptions.QuotationMark[0]:'"');
                    var values = ParseLine(line,csvDelimiters, quotationMark);
                    for (int j = 0; j < args.Map.InputFields.Count; j++)
                    {
                        if (j < values.Length)
                        {
                            row[j] = values[j];
                        }
                        else
                        {
                            row[j] = "";
                        }
                    }
                    args.ImportData.Rows.Add(row);
                    insertLineCount++;

                } while (!reader.EndOfStream);
                Log.Info(string.Format("EzImporter:{0} records read from input data.", insertLineCount), this);
            }
            catch (Exception ex)
            {
                Log.Error("EzImporter:" + ex.ToString(), this);
            }
        }

    private string[] ParseLine(string line, HashSet<char> csvDelimiters, char quotationMark)
    {
      Stack<string> result = new Stack<string>();

      int i = 0;
      while (true)
      {
        string cell = ParseNextCell(line, ref i, csvDelimiters, quotationMark);
        if (cell == null)
          break;
        result.Push(cell);
      }

      // remove last elements if they're empty
      while (string.IsNullOrEmpty(result.Peek()))
      {
        result.Pop();
      }

      var resultAsArray = result.ToArray();
      Array.Reverse(resultAsArray);
      return resultAsArray;
    }

    // returns iterator after delimiter or after end of string
    private string ParseNextCell(string line, ref int i, HashSet<char> csvDelimiters, char quotationMark)
    {
      if (i >= line.Length)
        return null;

      if (line[i] != quotationMark)
        return ParseNotEscapedCell(line, ref i, csvDelimiters);
      else
        return ParseEscapedCell(line, ref i, csvDelimiters, quotationMark);
    }

    // returns iterator after delimiter or after end of string
    private string ParseNotEscapedCell(string line, ref int i, HashSet<char> csvDelimiters)
    {
      StringBuilder sb = new StringBuilder();
      while (true)
      {
        if (i >= line.Length) // return iterator after end of string
          break;
        if (csvDelimiters.Contains(line[i]))
        {
          i++; // return iterator after delimiter
          break;
        }
        sb.Append(line[i]);
        i++;
      }
      return sb.ToString();
    }

    // returns iterator after delimiter or after end of string
    private string ParseEscapedCell(string line, ref int i, HashSet<char> csvDelimiters, char quotationMark)
    {
      i++; // omit first character (quotation mark)
      StringBuilder sb = new StringBuilder();
      while (true)
      {
        if (i >= line.Length)
          break;
        if (line[i] == quotationMark)
        {
          i++; // we're more interested in the next character
          if (i >= line.Length)
          {
            // quotation mark was closing cell;
            // return iterator after end of string
            break;
          }
          if (csvDelimiters.Contains(line[i]))
          {
            // quotation mark was closing cell;
            // return iterator after delimiter
            i++;
            break;
          }
          if (line[i] == quotationMark)
          {
            // it was doubled (escaped) quotation mark;
            // do nothing -- we've already skipped first quotation mark
          }

        }
        sb.Append(line[i]);
        i++;
      }

      return sb.ToString();
    }

    public string[] GetColumnNames(ImportItemsArgs args)
        {
            Log.Info("EzImporter:Reading column names from input CSV file...", this);
            try
            {
                using (var reader = new StreamReader(args.FileStream))
                {
                    var line = reader.ReadLine();
                    if (line != null)
                    {
                        return line.Split(args.ImportOptions.CsvDelimiter, StringSplitOptions.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("EzImporter:" + ex.ToString(), this);
            }
            return new string[] { };
        }
    }
}