using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

public class GetFile
{

    string path;
    DateTime last_Time;
    //存放资源的字典
    public Dictionary<string, string> FileResources_dir = new Dictionary<string, string>();
    public List<string> files = new List<string>();
    public List<string> clear_list = new List<string>();
    public GetFile(string path)
    {
        this.path = path;
        GetAllResources(path);
        
    }
    
    public void  GetAllFile()
    {
        
        // Console.WriteLine(files.Count);

        if (!File.Exists(path + "/res_report.json"))
        {
            Console.WriteLine("第一次加载耗时较长请勿关闭");
            File.CreateText(path + "/res_report.json").Close();
            foreach (var item in files)
            {
                Console.WriteLine("正在加载：{0}",item);
                string name = item.Substring(item.IndexOf("cocosstudio") + 12).Replace('\\', '/');
                FileResources_dir[GetHasCode(new FileInfo(item)).ToString()] = name;

                //Console.WriteLine(item.FullName);
            }
            //Console.WriteLine("2");
        }
        else
        {
            Console.WriteLine("正在加载");
            FileInfo file = new FileInfo(path + "/res_report.json");
            last_Time = file.LastWriteTime;
            StreamReader reder = new StreamReader(file.Open(FileMode.Open));
            string file_content = reder.ReadToEnd() ;
            var json =JsonConvert.DeserializeObject(file_content);
            FileResources_dir = JsonConvert.DeserializeObject<Dictionary<string, string>>(file_content);
            
            reder.Close();
            reder.Dispose();
            
            //判断时间
            for (int i = 0; i < files.Count; i++)
            {
                FileInfo _file = new FileInfo(files[i]);
                if (_file.LastWriteTime.CompareTo(last_Time)>0)
                {
                    string name = files[i].Substring(files[i].IndexOf("cocosstudio") + 12).Replace('\\', '/');
                    if (FileResources_dir.Values.Contains(name))
                    {
                        string key = GetKey(name);
                        FileResources_dir.Remove(key);

                    }
                    FileResources_dir[GetHasCode(new FileInfo(files[i])).ToString()] = name;
                    
                   
                }
              
            }
          


        }
        ClearUnuseResources();
        for (int i = 0; i < clear_list.Count; i++)
        {
            FileResources_dir.Remove(clear_list[i]);
        }

        string new_json = JsonConvert.SerializeObject(FileResources_dir);
        new_json = new_json.Replace(",", ",\n");
        new_json = new_json.Replace("{", "{\n");
        new_json = new_json.Replace("}", "\n}");
        StreamWriter writer = new StreamWriter(File.Open(path + "/res_report.json", FileMode.Create));
        writer.Write(new_json);
        writer.Close();
        writer.Dispose();
        Console.WriteLine("dowm");

    }

    public void GetAllResources(string path)
    {
        FileSystemInfo[] fileSystems = new DirectoryInfo(path).GetFileSystemInfos();
        for (int i = 0; i < fileSystems.Length; i++)
        {
            if (File.Exists(fileSystems[i].FullName))
            {
                if (fileSystems[i].Name.EndsWith(".jpg") || fileSystems[i].Name.EndsWith(".png"))
                {
                    files.Add(fileSystems[i].FullName);
                }
            }
            else if(Directory.Exists(fileSystems[i].FullName))
            {
                GetAllResources(fileSystems[i].FullName);
            }
        }
    }
    public StringBuilder GetHasCode(FileInfo fileInfo)
    {
        SHA1 sHA1 = new SHA1Managed();
        var hashcode = sHA1.ComputeHash(fileInfo.Open(FileMode.Open));
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < hashcode.Length; i++)
        {
            stringBuilder.Append(hashcode[i].ToString("X2").ToLower());
        }
        return stringBuilder;

       

    }
    public void ClearUnuseResources()
    {
        foreach (var item in FileResources_dir)
        {
            string _path =path+"\\"+ item.Value.Replace('/', '\\');
            if (files.IndexOf(_path)==-1)
            {
                clear_list.Add(GetKey(item.Value));
            }
            
        }
    }
    public string GetKey(string value)
    {
        foreach (var item in FileResources_dir)
        {
            if(item.Value==value)
            {
                return item.Key;
            }
        }
        return null;
    }

  

}
