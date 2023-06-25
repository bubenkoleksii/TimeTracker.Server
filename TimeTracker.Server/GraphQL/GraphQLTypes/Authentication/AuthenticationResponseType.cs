using GraphQL.Types;
using TimeTracker.Server.Models.Authentication;

namespace TimeTracker.Server.GraphQL.GraphQLTypes.Authentication
{
    public class AuthenticationResponseType : ObjectGraphType<AuthenticationResponse>
    {
        public AuthenticationResponseType() 
        {
            Field(r => r.AccessToken, type: typeof(StringGraphType));
            Field(r => r.RefreshToken, type: typeof(StringGraphType));
            Field(r => r.Message, type: typeof(StringGraphType));
        }
    }
}