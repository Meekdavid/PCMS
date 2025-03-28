﻿using Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ConfigurationSettings.ConfigModels
{
    public class EmailDetails
    {
        public string SMTPServer { get; set; }
        public int Port { get; set; }
        public string MemberName { get; set; }
        public string Password { get; set; }
        public string WelcomeEmail { get; set; }

        //public EmailDetails()
        //{
        //    MemberName =  AESHelper.Decrypt(MemberName) ?? string.Empty;
        //    Password =  AESHelper.Decrypt(Password) ?? string.Empty;
        //}
    }

}
