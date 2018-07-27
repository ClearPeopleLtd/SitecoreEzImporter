using Excel;
using EzImporter.Pipelines.ImportItems;
using HtmlAgilityPack;
using Sitecore.Diagnostics;
using System;
using System.Collections.Generic;
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
        PreProcessFilesRecursive(@"C:\darcy-out\www.shoosmiths.co.uk");
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
        IList<string> skipfolders = new List<string>();
        skipfolders.Add(@"files");
        skipfolders.Add(@"images");
        skipfolders.Add(@"webvirtuals");
        
        var mainroots = sDirinfo.GetFiles("Index.aspx");
        if (mainroots != null && mainroots.Count() == 1)
        {
          var row = args.ImportData.NewRow();
          row["Name"] = sDirinfo.Name.Replace("-", " ");
          row["father"] = parent;
          row["ID"] = Guid.NewGuid();
          parent = row["ID"].ToString();
          row["Path"] = mainroots[0].FullName;//.Replace(sDir, "");
          PopulateContent(mainroots[0].FullName, row, args);
        }

        foreach (string d in Directory.GetDirectories(sDir))
        {
          Log.Info(d, this);
          var row = args.ImportData.NewRow();
          DirectoryInfo di = new DirectoryInfo(d);
          if (!skipfolders.Contains(di.Name))
          {
            row["Name"] = di.Name.Replace("-", " ");
            row["father"] = parent;
            row["ID"] = Guid.NewGuid();
            row["Path"] = di.FullName;//.Replace(sDir, "");
            var roots = di.GetFiles("Index.aspx");
            if(roots!= null && roots.Count()==1)
            {
              PopulateContent(roots[0].FullName, row, args);
            }
            else
              args.ImportData.Rows.Add(row);
            getFilesRecursive(d, args, row["ID"].ToString());
          }
        }
        int filecounter = 0;
        foreach (var file in Directory.GetFiles(sDir, "*.aspx*"))
        {
          
            Log.Info(file, this);
            var row = args.ImportData.NewRow();
            row["father"] = parent;
            row["ID"] = Guid.NewGuid();
          row["Path"] = file;//.Replace(sDir, "");
            PopulateContent(file, row, args);
          //if (++filecounter >= 10)
          //  break;
        }

      }
      catch (System.Exception e)
      {
        Log.Error(e.Message, this);
      }
    }

    private void PopulateContent(string file, DataRow row, ImportItemsArgs args)
    {
      FileInfo fi = new FileInfo(file);
      var doc = new HtmlDocument();
      doc.Load(file);
      if (doc.DocumentNode != null)
      {

        foreach (var field in args.Map.InputFields)
        {
          if (field.Name.Equals("name", StringComparison.InvariantCultureIgnoreCase))
          {
            row[field.Name] = fi.Name.Substring(0, fi.Name.IndexOf(".aspx")).Replace("-", " ").Replace(@"%20", "");
          }
          else if (field.Name.Equals("originalvalue", StringComparison.InvariantCultureIgnoreCase))
          {
            row[field.Name] = doc.DocumentNode.InnerHtml;
          }
          else
          {
            if (!string.IsNullOrWhiteSpace(field.XsltSelector))
            {
              var node = doc.DocumentNode.SelectSingleNode(field.XsltSelector);
              if (node != null)
              {
                if (!string.IsNullOrWhiteSpace(field.Property))
                {
                  var content = node.Attributes[field.Property];
                  if (content != null)
                  {
                    row[field.Name] = content.Value;
                  }
                }
                else
                {

                  var content = node.Attributes["content"];
                  if (content != null)
                  {
                    row[field.Name] = content.Value;
                  }
                  else
                  {
                    row[field.Name] = GetCleanContent(node, field);
                  }
                }
              }
            }
          }

          //



        }
        args.ImportData.Rows.Add(row);
        doc = null;
      }
    }

    private void PreProcessFilesRecursive(string sDir)
    {
      try
      {
        DirectoryInfo sDirinfo = new DirectoryInfo(sDir);
        IList<string> skipfolders = new List<string>();
        skipfolders.Add(@"files");
        skipfolders.Add(@"images");
        skipfolders.Add(@"webvirtuals");

        foreach (string d in Directory.GetDirectories(sDir))
        {
          Log.Info(d, this);
          DirectoryInfo di = new DirectoryInfo(d);
          if (!skipfolders.Contains(di.Name))
          {
            PreProcessFilesRecursive(d);
          }
        }
        foreach (var file in Directory.GetFiles(sDir, "*.aspx*"))
        {
          //This is where you would manipulate each file found, e.g.:
          Log.Info(file, this);      
          if(!file.EndsWith(".aspx") && !file.Contains("search.aspx"))
          {
            DirectoryInfo di = new DirectoryInfo(file.Substring(0, file.IndexOf(".aspx")));
            MoveToFolder(new FileInfo(file), di);
          }
        }

      }
      catch (System.Exception e)
      {
        Log.Error(e.Message, this);
      }
    }

    private void MoveToFolder(FileInfo file, DirectoryInfo directory)
    {
      if (!directory.Exists)
        directory.Create();
      int startindex = file.FullName.IndexOf(".aspx") + 5;
      string firstpart = file.FullName.Substring(startindex,file.FullName.Length - startindex);
      firstpart = Utils.GetValidItemName(firstpart);
      file.MoveTo(directory.FullName + "/" + firstpart + ".aspx");      
    }
    private string GetCleanContent(HtmlNode node, Map.InputField field)
    {
      var form = node.SelectSingleNode("//form");
      if (form != null)
      {
        node = form.ParentNode;
        foreach (var item in form.ChildNodes.Where(x => x.Attributes["class"] == null || x.Attributes["class"].Value != "aspNetHidden"))
        {
          node.AppendChild(item.CloneNode(true));
        }
        node.RemoveChild(form);
      }
      foreach (var item in field.Fields)
      {
        var noredestobereplaced = node.SelectNodes(item.XsltSelector);
        if (noredestobereplaced != null)
        {
          foreach (var n in noredestobereplaced)
          {
            n.ParentNode.ReplaceChild(HtmlNode.CreateNode(item.ReplacementText), n);
          }
        }
      }
      return field.TextOnly ? new string(node.InnerText.Where(c => !char.IsControl(c)).ToArray()) : node.InnerHtml;
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