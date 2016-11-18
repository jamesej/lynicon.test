﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Lynicon.Utility;
using Lynicon.Membership;
using Lynicon.Collation;
using System.Runtime.InteropServices;
using System.Diagnostics;
using EnvDTE;
using System.Reflection;

namespace Lynicon.Tools
{
    // Windows PowerShell assembly.

    // Declare the class as a cmdlet and specify and 
    // appropriate verb and noun for the cmdlet name.
    [Cmdlet(VerbsData.Initialize, "LyniconAdmin")]
    public class InitializeLyniconAdminCommand : Cmdlet
    {
        // Declare the parameters for the cmdlet.
        [Parameter(Mandatory = true)]
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        private string password;

        protected override void ProcessRecord()
        {
            ProjectContextLoader.InitialiseDataApi(this);

            using (AppConfig.Change(ProjectContextLoader.WebConfigPath))
            {
                try
                {
                    var adminUser = Collator.Instance.Get<User, User>(iq => iq.Where(u => u.UserName == "administrator")).FirstOrDefault();
                    if (adminUser == null)
                    {
                        Guid adminUserId = Guid.NewGuid();
                        adminUser = Collator.Instance.GetNew<User>(new Address(typeof(User), adminUserId.ToString()));
                        adminUser.Email = "admin@lynicon-user.com";
                        adminUser.Id = adminUserId;
                        adminUser.Roles = "AEU";
                        adminUser.UserName = "administrator";
                        Collator.Instance.Set(adminUser, true);
                    }

                    LyniconSecurityManager.Current.SetPassword(adminUser.IdAsString, Password);
                }
                catch (Exception ex)
                {
                    WriteVerbose(string.Format("Exception was: {0} at: {1}", ex.Message, ex.StackTrace));
                    ThrowTerminatingError(new ErrorRecord(ex, "USERACTIONSFAIL", ErrorCategory.InvalidOperation, null));
                }
            }
        }
    }
}