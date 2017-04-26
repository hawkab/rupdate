using System;
using System.Management;
using xNet.Net;
using xNet.Collections;

namespace rupdate
{
    public static class HWID
    {
        public static bool checkIt()
        {
            try
            {
                using (var Request = new HttpRequest())
                {
                    string hw = HWID.getHWID().ToString();
                    string resp =  Request.Get("http://www.resumeupdater.ru/gate.php?id=" + hw).ToString();
                    //HttpRequest req = new HttpRequest(); HttpResponse res; var reqParams = new RequestParams();
                    //reqParams["resume"] = "";
                    //reqParams["undirectable"] = "true";
                    //res = req.Post("http://www.resumeupdater.ru/pays.php", reqParams);
                    if (!String.Equals(resp, HWID.MD5Hash(hw)))
                    {
                        return false;
                    }
                }
            }
                catch 
            {
                return false;
            }
            return true;
        }
        public static string getHWID()
        {
            ManagementObjectCollection mbsList = null;
            ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_processor");
            mbsList = mbs.Get();
            string id = "";
            foreach (ManagementObject mo in mbsList)
            {
                id= mo["ProcessorID"].ToString();
            }
            return id;
        }
        public static string MD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }
}