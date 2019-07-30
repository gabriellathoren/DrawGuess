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
using UnityEngine;
using Photon.Realtime;
using DrawGuess.Exceptions;

namespace DrawGuess.Security
{
    public class CredentialControl 
    {
        private static bool _running = true;

        public PasswordCredential GetCredentialFromLocker()
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
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SystemLogIn(string email, string password, User user = null)
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

                GameEngine gameEngine = new GameEngine();
                //Start game loop to have continues connection to Photon
                Task task = Task.Run((Action)gameEngine.GameLoop);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task PlayFabLogIn()
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
                else if (loginTask.Result != null)
                {
                    await RequestPhotonToken(loginTask.Result);
                }

                _running = false;
                Thread.Sleep(1);
            }
        }

        private async Task RequestPhotonToken(LoginResult obj)
        {
            //We can player PlayFabId. This will come in handy during next step
            string _playFabPlayerIdCache = obj.PlayFabId;
            (App.Current as App).User.PlayFabId = obj.PlayFabId;

            var photonAuthTokenTask = await PlayFabClientAPI.GetPhotonAuthenticationTokenAsync(new GetPhotonAuthenticationTokenRequest()
            {
                PhotonApplicationId = (App.Current as App).PhotonAppId
            });

            if (photonAuthTokenTask.Error != null)
            {
                _running = false;
                throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Could not authenticate user");
            }
            else if (photonAuthTokenTask.Result != null)
            {
                AuthenticateWithPhoton(photonAuthTokenTask.Result, obj.PlayFabId);
            }
        }

        private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj, string playFabPlayerId)
        {
            //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
            var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };

            //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
            customAuth.AddAuthParameter("username", playFabPlayerId);    // expected by PlayFab custom auth service

            //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
            customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

            //We finally tell Photon to use this authentication parameters throughout the entire application.
            (App.Current as App).LoadBalancingClient.AuthValues = customAuth;
            (App.Current as App).LoadBalancingClient.UserId = playFabPlayerId;
            (App.Current as App).LoadBalancingClient.AuthValues.Token = obj.PhotonCustomAuthenticationToken;
            (App.Current as App).LoadBalancingClient.AuthValues.UserId = playFabPlayerId;
            (App.Current as App).LoadBalancingClient.NickName = (App.Current as App).User.FirstName + " " + (App.Current as App).User.LastName;

        }
    }
}
