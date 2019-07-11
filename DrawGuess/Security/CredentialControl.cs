using DrawGuess.Models;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Storage;

namespace DrawGuess.Security
{
    public class CredentialControl
    {
        private static bool _running = true;

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
            catch (Exception e)
            {
                return null;
            }
        }

        public static async Task SystemLogIn(string email, string password, User user = null)
        {
            try
            {
                if (user == null)
                {
                    user = Models.User.GetUser(email, password);
                }

                var vault = new PasswordVault();

                if (vault.RetrieveAll().Count < 1)
                {
                    vault.Add(new PasswordCredential((App.Current as App).ResourceName, email, password));
                }

                if (user != null)
                {
                    (App.Current as App).User = user;
                }

                //Log in for game engine PlayFab
                await PlayFabLogIn();

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static async Task PlayFabLogIn()
        {
            var request = new LoginWithCustomIDRequest { CustomId = (App.Current as App).User.Id.ToString(), CreateAccount = true };
            var loginTask = await PlayFabClientAPI.LoginWithCustomIDAsync(request);

            while (_running)
            {
                if (loginTask.Error != null)
                {
                    _running = false;
                    throw new Exception("Could not log user on PlayFab");
                }

                _running = false;
                Thread.Sleep(1);
            }
        }
    }
}
