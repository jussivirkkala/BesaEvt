/* Creating -Besa2.evt based on Brain Quick .evt and -Besa1.evt 
 * @jussivirkkala
 * 2022-04-18 v1.0.2 Clean code. Link to github, cheackin EXT_ and start time .000  
 * 2022-04-14 v1.0.1 Final pause. 
 * 2022-04-13 v1.0.0 First version based on keegz. 
 * 
 * dotnet publish -r win-x64 -c Release --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true
 */

using System;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Globalization; // Time


Line("Creating -Besa2.evt based on Brain Quick .evt and -Besa1.evt v" + 
    FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion +
    "\ngithub.com/jussivirkkala/BesaEvt");
if (args.Length!=1)
{
    Line(@"Drag Brain Quick .evt over exe or over shortcut. Or use SendTo with %APPDATA%\Microsoft\Windows\SendTo");
    End();
    return;
}

// Filename as parameter
string f = args[0];
Line($"Brain Quick File parameter: {f}");
if (!File.Exists(f))
{
    Line("File does not exist");
    End();
    return;
}

if ( (!f.StartsWith("EXT_")) | (!f.ToUpper().EndsWith(".EVT"))   )
{
    Line("File should be EXT_*.EVT"); 
    End();
    return;
}

// Without extension
f=f.Substring(0,f.Length-4);
DateTime start;

if (File.Exists(f+"-Besa2.evt"))
{
    Line($"Output file {f}-Besa2.evt already exist");
    End();
    return;
}

// Read 
start=besaStart(f+"-Besa1.evt");
if (start==new DateTime(0))
    return;

// Write
besaWrite(f,start);

//  Line("Completed");
End();


// Read file start from Besa .evt
static DateTime besaStart(string f)
{
    DateTime start=new DateTime(0);
    Line($"Besa export file: {f}");

    if (!File.Exists(f))
    {
        Line($"File does not exist");
        End();
    }
    CultureInfo culture = CultureInfo.InvariantCulture;
    string s=File.ReadAllText(f);
    string[] sub = s.Split('\t',StringSplitOptions.TrimEntries);
    foreach (var ss in sub)
    {
    //    Console.WriteLine($"Substring: {ss}");
    }

    if (sub.Length<6) 
    {
        Line($"Error in {f} format");
        End();
        return start;
    }

    if (!sub[4].Equals("Ver-C\r\n0")) 
    {
        Line($"Error in {f} format. Version must be Ver-C and first line should be 0");
        End();
        return start;
    }

    if (!sub[5].Equals("41")) 
    {
        Line($"Error in {f} format. First line should be 0 41");
        End();
        return start;
    }
    // Problem with Besa 7.1.2.1 time 2021-11-11T22:36:6.000
    
    if (sub[6].Length==22)
        sub[6]=sub[6].Insert(17,"0");
    if (DateTime.TryParse(sub[6], culture, DateTimeStyles.None, out start))
    {
        Line("File start event: "+start.ToString("o"));
    }
    else
    {
        Line($"Problem with start: {sub[6]}");
        End();
    }
    if (!sub[6].EndsWith(".000"))
    {
        Line($"Start {sub[6]} should end with .000");
        End();
    }
    return start;
}

// Write besa Event
static void besaWrite(string f, DateTime start)
{

    // Header for -
    string r="Tmu\tCode\tTriNo\tComnt\tVer-C\r\n";
    CultureInfo culture = CultureInfo.InvariantCulture;

    XmlDocument doc=null;
    try
    {
        string s=File.ReadAllText(f+".evt");
        doc = new XmlDocument();
        doc.LoadXml(s);
        XmlNodeList elemList = doc.GetElementsByTagName("Event");   

            string type="";
        foreach (XmlNode elem in elemList)
        {
            DateTime time1=new DateTime(0);
            DateTime time2=new DateTime(0);
            
            foreach (XmlNode child in elem.ChildNodes)
            {
                string t=child.InnerText;

                switch(child.Name) 
                {
                case "Begin":
                    if (!DateTime.TryParse(t, culture, DateTimeStyles.None, out time1))
                    {
                        Line("Error parsing Begin");
                        return;
                    }
                    break;
                case "End":
                    if (!DateTime.TryParse(t, culture, DateTimeStyles.None, out time2))
                    {
                        Line("Error parsing End");
                        return;
                    }
                    break;
                case "Text":
                    type=t;
                    break;
                }
            }
            string offset=(time1.Subtract(start).TotalMilliseconds*1000.0).ToString("0");
            r+=offset+"\t2\t0\t"+type+"\r\n";
            Line($"Event:{time1.ToString("o")} {type}");
        }
        File.WriteAllText(f+"-Besa2.evt",r);
        Line($"File {f}-Besa2.evt created");
        }            
    catch
    {
        Line($"Error parsing Brain Quick {f}.evt");
        return;
    }

}

// Display line
static string Line(string s)
{
    Console.WriteLine(s);
    return s + "\n";
}

// Close info
static void End()
{
    Console.Write("Press any key or close window...");
    Console.ReadKey();
}

// End