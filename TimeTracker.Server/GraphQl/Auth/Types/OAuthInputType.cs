using GraphQL.Types;
using TimeTracker.Server.Models.Auth;

namespace TimeTracker.Server.GraphQl.Auth.Types;

public class OAuthInputType : InputObjectGraphType<OAuthRequest>
{
    public OAuthInputType()
    {
        Field(i => i.ClientId, type: typeof(StringGraphType), nullable: false)
            .Name("clientId")
            .Description("Client id field for auth object");

        Field(i => i.Credential, type: typeof(StringGraphType), nullable: false)
            .Name("credential")
            .Description("Credential field for auth object");

        Field(i => i.SelectBy, type: typeof(StringGraphType), nullable: false)
            .Name("select_by")
            .Description("Credential field for auth object");
    }
}