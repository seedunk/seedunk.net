
using System;
using System.IO; 
using System.Xml;
using System.Xml.Linq;

namespace  Seedunk.Net.Utils
{
    public class Setting
    {
        XmlDocument doc = new XmlDocument();
        private String SettingName { get; set; } = "";


#if DEBUG 
    public Setting(string name = "settings.xml")
    {
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><settings></settings>";
            if (File.Exists(name))
            {
                xml = File.ReadAllText(name).Decrypt("seedunk");

            }
            else {
                File.WriteAllText(name, xml.Encrypt("seedunk"));
            }  
           doc.LoadXml(xml); 
           SettingName = name;
    }

     public void Save()
      {
           File.WriteAllText(SettingName, doc.OuterXml.Encrypt("seedunk"));
      }

#else
    public Setting(string name = "settings.xml")
    {
        if (!File.Exists(name))
        {
            File.WriteAllText(name, "<?xml version=\"1.0\" encoding=\"UTF-8\"?><settings></settings>");
        }  
        doc.Load(name); 
        SettingName = name;
    }   
    
    public void Save() { 
            doc.Save(SettingName);
        }
#endif



        public bool TryGetString(string name, out string value)
        {


            XmlNode node = doc.SelectSingleNode("settings/"+ name);

            if (node == null)
            {
                value = "";

                return false;
            }
            else { 
              value=node.InnerText;
                return true;
            }
           
        }


        public bool Set<T>(string name, T value,bool isSubmit=true  ) { 
        
             XmlNode node = doc.SelectSingleNode("settings/" + name);
             if (node != null)  {
                 node.ParentNode.RemoveChild(node);
                } 
             XmlNode newNode = doc.CreateElement(name);
                     newNode.InnerText=Convert.ToString(value);

              doc.DocumentElement.AppendChild(newNode);
             if(isSubmit)
                  Save();
             return true;
        }

    }
}
