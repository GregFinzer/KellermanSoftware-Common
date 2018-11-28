
namespace KellermanSoftware.Common
{
    /// <summary>
    /// Interface for dealing with licenses
    /// </summary>
    public interface ILicenseInterface
    {
        /// <summary>
        /// Returns true if the trial extension is valid
        /// </summary>
        /// <param name="trialExtension"></param>
        /// <returns></returns>
        bool ExtendTrial(string trialExtension);

        /// <summary>
        /// Get the message about the trial
        /// </summary>
        /// <returns></returns>
        string TrialMessage();

        /// <summary>
        /// How many days left in the trial
        /// </summary>
        /// <returns></returns>
        int TrialDaysLeft();

        /// <summary>
        /// If true, the license is valid
        /// </summary>
        /// <returns></returns>
        bool CheckLicense();

        /// <summary>
        /// If true, the trial is still valid
        /// </summary>
        /// <returns></returns>
        bool TrialValid();

        /// <summary>
        /// Additional information about the license
        /// </summary>
        string AdditionalInfo
        {
            get;
        }

        /// <summary>
        /// The user name of the licensor
        /// </summary>
        string UserName
        {
            get;
            set;
        }

        /// <summary>
        /// The License Key
        /// </summary>
        string License
        {
            get;
            set;
        }

        /// <summary>
        /// True if it is a licensed user
        /// </summary>
        bool LicensedUser
        {
            get;
        }
    }
}
