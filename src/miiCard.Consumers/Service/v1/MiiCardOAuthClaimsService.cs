using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using miiCard.Consumers.Service.v1.Claims;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.IO;

namespace miiCard.Consumers.Service.v1
{
    public class MiiCardOAuthClaimsService : MiiCardOAuthServiceBase
    {
        public MiiCardOAuthClaimsService(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret)
        {

        }

        public MiiCardOAuthClaimsService(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, bool useDeclarativeBindings)
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret, useDeclarativeBindings)
        {

        }

        public MiiCardOAuthClaimsService(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Binding binding, EndpointAddress endpointAddress)
            : base(consumerKey, consumerSecret, accessToken, accessTokenSecret, binding, endpointAddress)
        {

        }

        public MiiApiResponse<MiiUserProfile> GetClaims()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.GetClaims());

            return new MiiApiResponse<MiiUserProfile>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public Stream AssuranceImage(string type)
        {
            return this.GetAuthenticatedServiceMethodResult
            (
                x => x.AssuranceImage(type)
            );
        }

        public MiiApiResponse<bool> IsSocialAccountAssured(string socialAccountId, string socialAccountType)
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.IsSocialAccountAssured(socialAccountId, socialAccountType));
            return new MiiApiResponse<bool>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<bool> IsUserAssured()
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.IsUserAssured());
            return new MiiApiResponse<bool>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<IEnumerable<IdentitySnapshotDetails>> GetIdentitySnapshotDetails(string snapshotId = null)
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.GetIdentitySnapshotDetails(snapshotId));
            return new MiiApiResponse<IEnumerable<IdentitySnapshotDetails>>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public MiiApiResponse<IdentitySnapshot> GetIdentitySnapshot(string snapshotId)
        {
            var response = this.GetAuthenticatedServiceMethodResult(x => x.GetIdentitySnapshot(snapshotId));
            return new MiiApiResponse<IdentitySnapshot>(response.Status, response.ErrorCode, response.ErrorMessage, response.Data, response.IsTestUser);
        }

        public Stream GetIdentitySnapshotPdf(string type)
        {
            return this.GetAuthenticatedServiceMethodResult
            (
                x => x.GetIdentitySnapshotPdf(type)
            );
        }

        private T GetAuthenticatedServiceMethodResult<T>(Func<ClaimsClient, T> serviceMethodInvoker)
        {
            ClaimsClient client = null;
            try
            {
                if (this.UseDeclarativeBindingConfiguration)
                {
                    client = new ClaimsClient();
                }
                else
                {
                    client = new ClaimsClient(this.Binding, this.EndpointAddress);
                }

                client.Endpoint.Contract = ContractDescription.GetContract(typeof(IClaims));

                using (OperationContextScope scope = new OperationContextScope(client.InnerChannel))
                {
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = this.GetOAuthHeader(client.Endpoint.Address.Uri, true);
                    return serviceMethodInvoker(client);
                }
            }
            finally
            {
                client.Close();
            }
        }

        protected override EndpointAddress GetDefaultEndpointAddress()
        {
            return new EndpointAddress(ServiceUrls.ClaimsServiceEndpoint);
        }
    }
}
