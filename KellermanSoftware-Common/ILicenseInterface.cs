using System;
using System.Collections.Generic;
using System.Text;

namespace KellermanSoftware.Common
{
    public interface ILicenseInterface
    {
        bool ExtendTrial(string trialExtension);
        string TrialMessage();
        int TrialDaysLeft();
        //string GenerateLicense();
        //string GenerateTrialExtension();
        bool CheckLicense();
        bool TrialValid();

        string AdditionalInfo
        {
            get;
        }

        string UserName
        {
            get;
            set;
        }
        string License
        {
            get;
            set;
        }

        bool LicensedUser
        {
            get;
        }
    }
}
