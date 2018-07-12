using Excel;
using EzImporter.Pipelines.ImportItems;
using HtmlAgilityPack;
using Sitecore.Diagnostics;
using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace EzImporter.DataReaders
{
    public class ZipedFolderDataReader : IDataReader
    {
        public void ReadData(ImportItemsArgs args)
        {
            Log.Info("EzImporter:Reading Ziped folder input data", this);
            try
            {
        //IExcelDataReader excelReader;
        //if (args.FileExtension == "xls")
        //{
        //    excelReader = ExcelReaderFactory.CreateBinaryReader(args.FileStream, ReadOption.Loose);
        //}
        //else
        //{
        //    excelReader = ExcelReaderFactory.CreateOpenXmlReader(args.FileStream);
        //}
        FileInfo fi = new FileInfo(@"C:\darcy-out\www.shoosmiths.co.uk.zip");
        string folder = Decompress(fi);
        getFilesRecursive(@"C:\darcy-out\www.shoosmiths.co.uk", args);
        //excelReader.IsFirstRowAsColumnNames = args.ImportOptions.FirstRowAsColumnNames;
        //if (!excelReader.IsValid)
        //{
        //    Log.Error("EzImporter:Invalid Excel file '" + excelReader.ExceptionMessage + "'", this);
        //    return;
        //}
        //DataSet result = excelReader.AsDataSet();
        //if (result == null)
        //{
        //    Log.Error("EzImporter:No data could be retrieved from Excel file.", this);
        //}
        //if (result.Tables == null || result.Tables.Count == 0)
        //{
        //    Log.Error("EzImporter:No worksheets found in Excel file", this);
        //    return;
        //}
        //var readDataTable = result.Tables[0];
        //foreach (var readDataRow in readDataTable.AsEnumerable())
        //{
        //    var row = args.ImportData.NewRow();
        //    for (int i = 0; i < args.Map.InputFields.Count; i++)
        //    {
        //        if (i < readDataTable.Columns.Count && readDataRow[i] != null)
        //        {
        //            row[i] = Convert.ToString(readDataRow[i]);
        //        }
        //        else
        //        {
        //            row[i] = "";
        //        }
        //    }
        //    args.ImportData.Rows.Add(row);
        //}
        //Log.Info(string.Format("EzImporter:{0} records read from input data.", readDataTable.Rows.Count), this);
      }
            catch (Exception ex)
            {
                Log.Error("EzImporter:" + ex.ToString(), this);
            }
        }

    

    private void getFilesRecursive(string sDir, ImportItemsArgs args, string parent = "")
    {
      try
      {
        DirectoryInfo sDirinfo = new DirectoryInfo(sDir);
        foreach (string d in Directory.GetDirectories(sDir))
        {
          Log.Info(d, this);
          var row = args.ImportData.NewRow();
          DirectoryInfo di = new DirectoryInfo(d);
          row["Name"] = di.Name.Replace("-", " ");
          row["father"] = parent;
          row["ID"] = Guid.NewGuid();
          args.ImportData.Rows.Add(row);
          getFilesRecursive(d, args, row["ID"].ToString());
        }        
        foreach (var file in Directory.GetFiles(sDir,"*.aspx"))
        {
          //This is where you would manipulate each file found, e.g.:
          Log.Info(file, this);
          var row = args.ImportData.NewRow();
          row["father"] = parent;
          row["ID"] = Guid.NewGuid();
          FileInfo fi = new FileInfo(file);
          var doc = new HtmlDocument();
          doc.Load(file);
          if (doc.DocumentNode != null)
          {
            
              foreach (var field in args.Map.InputFields)
              {
                if (field.Name.Equals("name", StringComparison.InvariantCultureIgnoreCase))
                {
                  row[field.Name] = fi.Name.Replace(fi.Extension, "").Replace("-", " ").Replace(@"%20", "");
                }
                else
                {
                  if (!string.IsNullOrWhiteSpace(field.XsltSelector))
                  {
                    var node = doc.DocumentNode.SelectSingleNode(field.XsltSelector);
                    if (node != null)
                    {
                      var content = node.Attributes["content"];
                      if (content!= null)
                        row[field.Name] = content.Value;
                    }
                  }
                }
              
              doc = null;


              args.ImportData.Rows.Add(row);             
            }
          }
          }
          
      }
      catch (System.Exception e)
      {
        Log.Error(e.Message, this);
      }
    }

    

    public string Decompress(FileInfo fileToDecompress)
    {
      using (FileStream originalFileStream = fileToDecompress.OpenRead())
      {
        string currentFileName = fileToDecompress.FullName;
        string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length) + "tmp";

        using (FileStream decompressedFileStream = File.Create(newFileName))
        {
          if (!File.Exists(newFileName))
          {

            using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
            {
              decompressionStream.CopyTo(decompressedFileStream);
              Console.WriteLine("Decompressed: {0}", fileToDecompress.Name);
            }
          }
          else
          {
            Log.Warn("The target folder already exists", this);
          }
        }
        return newFileName;
      }
    }

    public string[] GetColumnNames(ImportItemsArgs args)
    {
      return new string[0];
    }
  }
}