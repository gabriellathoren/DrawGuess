using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace DrawGuess.Security
{
    public class CredentialControl
    {
        public static PasswordCredential GetCredentialFromLocker()
        {
            try
            {
                PasswordCredential credential = null;

                var vault = new PasswordVault();
                var credentialList = vault.FindAllByResource((App.Current as App).ResourceName);

                if (credentialList.Count > 0)
                {
                    if (credentialList.Count == 1)
                    {
                        credential = credentialList[0];
                    }
                    else
                    {
                        credential = null;
                    }
                }

                return credential;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public static void SystemLogIn(string email, string password)
        {
            try
            {
                Models.User User = Models.User.GetUser(email, password);

                var vault = new PasswordVault();

                if(vault.RetrieveAll().Count < 1) { 
                    vault.Add(new PasswordCredential((App.Current as App).ResourceName, email, password));
                }

                if (User != null)
                {
                    (App.Current as App).User = User;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
